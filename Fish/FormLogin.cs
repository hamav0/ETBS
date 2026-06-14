using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace Fish
{
    public partial class FormLogin : Form
    {
        private Panel pnlCheck, pnlAuth, pnlReg;
        private TextBox txtIdentity;
        private TextBox txtAuthPassword;
        private TextBox txtRegLogin, txtRegPassword, txtRegEmail, txtRegPhone;
        private Label lblAuthMessage, lblRegMessage;

        private string _inputIdentity = "";
        private string _actualLogin = "";

        public FormLogin()
        {
            InitializeComponent();
            InitUI();
        }

        private void InitUI()
        {
            this.Text = "Вход в систему";
            this.Size = new Size(400, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            Font titleFont = new Font("Arial", 12, FontStyle.Bold);
            Font defaultFont = new Font("Arial", 10, FontStyle.Regular);

            // =========================================================
            // ПАНЕЛЬ 1: Проверка пользователя (Логин/Почта/Телефон)
            // =========================================================
            pnlCheck = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

            Label lblCheckTitle = new Label { Text = "Вход или регистрация", Font = titleFont, Top = 30, Left = 50, Width = 300, TextAlign = ContentAlignment.MiddleCenter };
            Label lblCheckDesc = new Label { Text = "Введите логин, почту или телефон:", Font = defaultFont, Top = 80, Left = 50, Width = 300 };
            txtIdentity = new TextBox { Font = defaultFont, Top = 110, Left = 50, Width = 280 };

            Button btnNext = new Button { Text = "Продолжить", Font = defaultFont, Top = 160, Left = 50, Width = 280, Height = 40, BackColor = Color.LightBlue, FlatStyle = FlatStyle.Flat };
            btnNext.Click += BtnNext_Click;

            pnlCheck.Controls.Add(lblCheckTitle); pnlCheck.Controls.Add(lblCheckDesc);
            pnlCheck.Controls.Add(txtIdentity); pnlCheck.Controls.Add(btnNext);

            // =========================================================
            // ПАНЕЛЬ 2: Авторизация (Пользователь найден)
            // =========================================================
            pnlAuth = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Visible = false };

            lblAuthMessage = new Label { Text = "С возвращением!", Font = titleFont, Top = 30, Left = 50, Width = 300, TextAlign = ContentAlignment.MiddleCenter };
            Label lblAuthDesc = new Label { Text = "Введите пароль для авторизации:", Font = defaultFont, Top = 80, Left = 50, Width = 300 };
            txtAuthPassword = new TextBox { Font = defaultFont, Top = 110, Left = 50, Width = 280, UseSystemPasswordChar = true };

            Button btnAuth = new Button { Text = "Войти", Font = defaultFont, Top = 160, Left = 50, Width = 280, Height = 40, BackColor = Color.LightGreen, FlatStyle = FlatStyle.Flat };
            btnAuth.Click += BtnAuth_Click;

            Button btnBackFromAuth = new Button { Text = "Назад", Font = defaultFont, Top = 210, Left = 50, Width = 280, Height = 30, FlatStyle = FlatStyle.Flat };
            btnBackFromAuth.Click += (s, e) => ShowPanel(pnlCheck);

            pnlAuth.Controls.Add(lblAuthMessage); pnlAuth.Controls.Add(lblAuthDesc);
            pnlAuth.Controls.Add(txtAuthPassword); pnlAuth.Controls.Add(btnAuth); pnlAuth.Controls.Add(btnBackFromAuth);

            // =========================================================
            // ПАНЕЛЬ 3: Регистрация (Пользователь не найден)
            // =========================================================
            pnlReg = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Visible = false };

            lblRegMessage = new Label { Text = "Регистрация нового аккаунта", Font = titleFont, Top = 20, Left = 50, Width = 300, TextAlign = ContentAlignment.MiddleCenter };

            pnlReg.Controls.Add(new Label { Text = "Логин (Обязательно):", Font = defaultFont, Top = 70, Left = 50, AutoSize = true });
            txtRegLogin = new TextBox { Font = defaultFont, Top = 90, Left = 50, Width = 280 };

            pnlReg.Controls.Add(new Label { Text = "Пароль (Обязательно):", Font = defaultFont, Top = 130, Left = 50, AutoSize = true });
            txtRegPassword = new TextBox { Font = defaultFont, Top = 150, Left = 50, Width = 280, UseSystemPasswordChar = true };

            pnlReg.Controls.Add(new Label { Text = "Email (Необязательно):", Font = defaultFont, Top = 190, Left = 50, AutoSize = true });
            txtRegEmail = new TextBox { Font = defaultFont, Top = 210, Left = 50, Width = 280 };

            pnlReg.Controls.Add(new Label { Text = "Телефон (Необязательно):", Font = defaultFont, Top = 250, Left = 50, AutoSize = true });
            txtRegPhone = new TextBox { Font = defaultFont, Top = 270, Left = 50, Width = 280 };

            Button btnRegister = new Button { Text = "Зарегистрироваться", Font = defaultFont, Top = 320, Left = 50, Width = 280, Height = 40, BackColor = Color.LightSkyBlue, FlatStyle = FlatStyle.Flat };
            btnRegister.Click += BtnRegister_Click;

            Button btnBackFromReg = new Button { Text = "Назад", Font = defaultFont, Top = 370, Left = 50, Width = 280, Height = 30, FlatStyle = FlatStyle.Flat };
            btnBackFromReg.Click += (s, e) => ShowPanel(pnlCheck);

            pnlReg.Controls.Add(lblRegMessage); pnlReg.Controls.Add(txtRegLogin); pnlReg.Controls.Add(txtRegPassword);
            pnlReg.Controls.Add(txtRegEmail); pnlReg.Controls.Add(txtRegPhone);
            pnlReg.Controls.Add(btnRegister); pnlReg.Controls.Add(btnBackFromReg);

            this.Controls.Add(pnlCheck);
            this.Controls.Add(pnlAuth);
            this.Controls.Add(pnlReg);
        }

        private void ShowPanel(Panel pnl)
        {
            pnlCheck.Visible = false;
            pnlAuth.Visible = false;
            pnlReg.Visible = false;
            pnl.Visible = true;
        }

        // =========================================================
        // ЛОГИКА 1: Проверка существования пользователя
        // =========================================================
        private void BtnNext_Click(object sender, EventArgs e)
        {
            _inputIdentity = txtIdentity.Text.Trim();
            if (string.IsNullOrWhiteSpace(_inputIdentity))
            {
                MessageBox.Show("Пожалуйста, введите данные.");
                return;
            }

            bool exists = false;
            using (SqlConnection conn = new SqlConnection(DatabaseHelper.connectionString))
            {
                // Запрашиваем точный Логин, чтобы красиво вывести его и сохранить в сессию
                string query = "SELECT Логин FROM Аккаунты WHERE Логин = @Id OR Почта = @Id OR Номер = @Id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", _inputIdentity);
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        exists = true;
                        _actualLogin = result.ToString();
                    }
                }
            }

            if (exists)
            {
                // Обращаемся по реальному Логину, а не по почте/телефону
                lblAuthMessage.Text = $"Добро пожаловать, {_actualLogin}!";
                txtAuthPassword.Clear();
                ShowPanel(pnlAuth);
            }
            else
            {
                lblRegMessage.Text = "Аккаунт не найден. Зарегистрируйтесь:";
                txtRegLogin.Clear(); txtRegEmail.Clear(); txtRegPhone.Clear(); txtRegPassword.Clear();

                if (_inputIdentity.Contains("@"))
                    txtRegEmail.Text = _inputIdentity;
                else if (_inputIdentity.StartsWith("+") || System.Text.RegularExpressions.Regex.IsMatch(_inputIdentity, @"^[\d\s\-\(\)]+$"))
                    txtRegPhone.Text = _inputIdentity;
                else
                    txtRegLogin.Text = _inputIdentity;

                ShowPanel(pnlReg);
            }
        }

        // =========================================================
        // ЛОГИКА 2: Авторизация 
        // =========================================================
        private void BtnAuth_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAuthPassword.Text)) return;

            bool success = false;
            using (SqlConnection conn = new SqlConnection(DatabaseHelper.connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("Авторизация_Пользователя", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Ввод", _inputIdentity);
                    cmd.Parameters.AddWithValue("@Пароль", txtAuthPassword.Text);

                    SqlParameter outParam = new SqlParameter("@Успех", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(outParam);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    success = (bool)outParam.Value;
                }
            }

            if (success)
            {
                // СОХРАНЯЕМ В СЕССИЮ
                UserSession.Login = _actualLogin;

                MessageBox.Show("Авторизация успешна!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Неверный пароль.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // ЛОГИКА 3: Регистрация 
        // =========================================================
        private void BtnRegister_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtRegLogin.Text) || string.IsNullOrWhiteSpace(txtRegPassword.Text))
            {
                MessageBox.Show("Логин и Пароль обязательны для заполнения!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SqlConnection conn = new SqlConnection(DatabaseHelper.connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("СоздатьАккаунт", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Логин", txtRegLogin.Text.Trim());
                    cmd.Parameters.AddWithValue("@Пароль", txtRegPassword.Text);
                    cmd.Parameters.AddWithValue("@Почта", string.IsNullOrWhiteSpace(txtRegEmail.Text) ? (object)DBNull.Value : txtRegEmail.Text.Trim());
                    cmd.Parameters.AddWithValue("@Номер", string.IsNullOrWhiteSpace(txtRegPhone.Text) ? (object)DBNull.Value : txtRegPhone.Text.Trim());

                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();

                        // СОХРАНЯЕМ В СЕССИЮ
                        UserSession.Login = txtRegLogin.Text.Trim();

                        MessageBox.Show("Регистрация успешно завершена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 2627 || ex.Number == 2601)
                        {
                            MessageBox.Show("Аккаунт с таким Логином, Почтой или Телефоном уже существует!", "Ошибка регистрации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            MessageBox.Show("Ошибка БД: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }
    }
}