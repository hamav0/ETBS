using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Fish
{
    public partial class FormAfisha : Form
    {
        private FlowLayoutPanel flpSessions;

        public FormAfisha()
        {
            InitializeComponent();
            InitRightPanel();

            dtpDate.MinDate = DateTime.Today;
            dtpDate.Value = DateTime.Today;
            dtpDate.ValueChanged += (s, e) => LoadAfisha();

            LoadFilters();
            LoadAfisha();
            if (string.IsNullOrEmpty(UserSession.Login)) { BtnLogin.Visible = true; BtnMyTickets.Visible = false; }
            else { BtnLogin.Visible = false; BtnMyTickets.Visible = true; }
        }

        private void InitRightPanel()
        {
            this.Size = new Size(1200, 560);

            pnlRightDetails = new Panel();
            pnlRightDetails.Dock = DockStyle.Right;
            pnlRightDetails.Width = 359;
            pnlRightDetails.Visible = false;
            pnlRightDetails.BackColor = Color.White;
            pnlRightDetails.BorderStyle = BorderStyle.FixedSingle;

            lblRightTitle = new Label();
            lblRightTitle.Dock = DockStyle.Top;
            lblRightTitle.Height = 59;
            lblRightTitle.Font = new Font("Arial", 11, FontStyle.Bold);
            lblRightTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblRightTitle.Padding = new Padding(10, 0, 0, 0);
            lblRightTitle.BackColor = Color.FromArgb(240, 240, 240);

            flpSessions = new FlowLayoutPanel();
            flpSessions.Dock = DockStyle.Fill;
            flpSessions.AutoScroll = true;
            flpSessions.Padding = new Padding(10);
            flpSessions.BorderStyle = BorderStyle.FixedSingle;

            pnlRightDetails.Controls.Add(flpSessions);
            pnlRightDetails.Controls.Add(lblRightTitle);
            this.Controls.Add(pnlRightDetails);

            flpEvents.BringToFront();
        }
        private void LoadFilters()
        {
            using (SqlConnection conn = new SqlConnection(DatabaseHelper.connectionString))
            {
                conn.Open();

                // 1. Загрузка площадок (только те, где есть будущие события)
                using (SqlDataAdapter da = new SqlDataAdapter(@"
            SELECT DISTINCT P.Площадка_ИД, P.Название 
            FROM Площадки P
            JOIN События S ON P.Площадка_ИД = S.Площадка_ИД
            WHERE S.Дата_Время >= GETDATE()", conn))
                {
                    DataTable dtVenue = new DataTable();
                    da.Fill(dtVenue);
                    InsertAllOption(dtVenue, "Площадка_ИД", "Название", "Все площадки");

                    cmbVenue.DisplayMember = "Название";
                    cmbVenue.ValueMember = "Площадка_ИД";
                    cmbVenue.DataSource = dtVenue;

                    cmbVenue.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                    cmbVenue.AutoCompleteSource = AutoCompleteSource.ListItems;
                }

                // 2. Загрузка Тегов (Типов)
                using (SqlDataAdapter da = new SqlDataAdapter(@"
            SELECT DISTINCT T.Тип_ИД, T.Название 
            FROM Тип T
            JOIN Событие_Типы ST ON T.Тип_ИД = ST.Тип_ИД
            JOIN События S ON ST.Событие_ИД = S.Событие_ИД
            WHERE S.Дата_Время >= GETDATE()", conn))
                {
                    DataTable dtTag = new DataTable();
                    da.Fill(dtTag);
                    InsertAllOption(dtTag, "Тип_ИД", "Название", "Все теги");

                    cmbTag.DisplayMember = "Название";
                    cmbTag.ValueMember = "Тип_ИД";
                    cmbTag.DataSource = dtTag;
                }

                // 3. Загрузка Личностей
                using (SqlDataAdapter da = new SqlDataAdapter(@"
            SELECT DISTINCT L.Личность_ИД, L.Имя + ' (' + L.Роль + ')' AS ИмяРоль 
            FROM Личности L
            JOIN Событие_Личность SL ON L.Личность_ИД = SL.Личность_ИД
            JOIN События S ON SL.Событие_ИД = S.Событие_ИД
            WHERE S.Дата_Время >= GETDATE()", conn))
                {
                    DataTable dtPers = new DataTable();
                    da.Fill(dtPers);
                    InsertAllOption(dtPers, "Личность_ИД", "ИмяРоль", "Все личности");

                    cmbPersonality.DisplayMember = "ИмяРоль";
                    cmbPersonality.ValueMember = "Личность_ИД";
                    cmbPersonality.DataSource = dtPers;

                    cmbPersonality.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                    cmbPersonality.AutoCompleteSource = AutoCompleteSource.ListItems;
                }

                // Так как адрес хранится строкой, проще вытащить уникальные адреса и распарсить их в C#
                LoadCitiesFromAddresses(conn);
            }
        }

        // Вспомогательный метод для добавления строки "Все ..."
        private void InsertAllOption(DataTable dt, string idColumn, string nameColumn, string text)
        {
            DataRow row = dt.NewRow();
            row[idColumn] = DBNull.Value;
            row[nameColumn] = text;
            dt.Rows.InsertAt(row, 0);
        }

        private void LoadCitiesFromAddresses(SqlConnection conn)
        {
            List<string> cities = new List<string> { "Все города" };
            using (SqlCommand cmd = new SqlCommand(@"
        SELECT DISTINCT P.Адрес 
        FROM Площадки P 
        JOIN События S ON P.Площадка_ИД = S.Площадка_ИД 
        WHERE S.Дата_Время >= GETDATE()", conn))
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string address = reader["Адрес"].ToString();
                        // Логика извлекает город по структуре (Страна, г. Город, Улица, Дом)
                        string[] parts = address.Split(',');
                        if (parts.Length > 1)
                        {
                            string city = parts[1].Trim();
                            if (!cities.Contains(city)) cities.Add(city);
                        }
                    }
                }
            }

            cmbCity.DataSource = cities;
        }

        private void LoadAfisha()
        {
            if (string.IsNullOrEmpty(UserSession.Login)) { BtnLogin.Visible = true; BtnMyTickets.Visible = false; }
            else { BtnLogin.Visible = false; BtnMyTickets.Visible = true; }
            if (cmbCity == null || cmbVenue == null || cmbTag == null || cmbPersonality == null || cmbSort == null)
                return;
            flpEvents.Controls.Clear();
            pnlRightDetails.Visible = false;

            // для ОДИНОЧНЫХ мероприятий (концертов), когда группировка схлопывает строки.
            string query = @"
        SELECT 
            S.Информация_ИД,
            I.Название,
            MAX(P.Название) AS Площадка, 
            COUNT(DISTINCT S.Площадка_ИД) AS Кол_Во_Кинотеатров,
            MIN(S.Дата_Время) AS Мин_Дата_Время, 
            MIN(CP.Цена) AS Минимальная_Цена,
            MAX(S.Событие_ИД) AS Дефолтный_Событие_ИД, 
            MAX(S.Планировка_ИД) AS Дефолтный_Планировка_ИД
        FROM События S
        JOIN Информация I ON S.Информация_ИД = I.Информация_ИД
        JOIN Площадки P ON S.Площадка_ИД = P.Площадка_ИД
        LEFT JOIN Событие_Типы ST ON S.Событие_ИД = ST.Событие_ИД
        LEFT JOIN Событие_Личность SL ON S.Событие_ИД = SL.Событие_ИД
        LEFT JOIN Ценовая_Политика CP ON S.Событие_ИД = CP.Событие_ИД
        WHERE CAST(S.Дата_Время AS DATE) = @date ";


            using (SqlConnection conn = new SqlConnection(DatabaseHelper.connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@date", dtpDate.Value.Date);

                    List<SqlParameter> parameters = new List<SqlParameter>();
                    if (cmbVenue.SelectedValue != null && cmbVenue.SelectedValue != DBNull.Value)
                    {
                        query += " AND S.Площадка_ИД = @venueId ";
                        cmd.Parameters.AddWithValue("@venueId", cmbVenue.SelectedValue);
                    }

                    // Фильтр по тегам
                    if (cmbTag.SelectedValue != null && cmbTag.SelectedValue != DBNull.Value)
                    {
                        query += " AND ST.Тип_ИД = @typeId ";
                        cmd.Parameters.AddWithValue("@typeId", cmbTag.SelectedValue);
                    }

                    // Фильтр по личности
                    if (cmbPersonality.SelectedValue != null && cmbPersonality.SelectedValue != DBNull.Value)
                    {
                        query += " AND SL.Личность_ИД = @persId ";
                        cmd.Parameters.AddWithValue("@persId", cmbPersonality.SelectedValue);
                    }

                    // Фильтр по городу (через LIKE по строке Адреса)
                    if (cmbCity.SelectedItem != null && cmbCity.SelectedItem.ToString() != "Все города")
                    {
                        query += " AND P.Адрес LIKE @city ";
                        cmd.Parameters.AddWithValue("@city", "%" + cmbCity.SelectedItem.ToString() + "%");
                    }

                    // Обязательная группировка
                    query += " GROUP BY S.Информация_ИД, I.Название";
                    string sortChoice = cmbSort.SelectedItem?.ToString() ?? "По умолчанию";
                    switch (sortChoice)
                    {
                        case "Название (А-Я)":
                            query += " ORDER BY I.Название ASC";
                            break;
                        case "Название (Я-А)":
                            query += " ORDER BY I.Название DESC";
                            break;
                        case "Сначала поздние":
                            query += " ORDER BY Мин_Дата_Время DESC";
                            break;
                        default: // По умолчанию — Сначала ближайшие
                            query += " ORDER BY Мин_Дата_Время ASC";
                            break;
                    }

                    cmd.CommandText = query;
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        int cardIndex = 0; // Для чередования цветов (зебры)

                        while (reader.Read())
                        {
                            string infoId = reader["Информация_ИД"].ToString();
                            string title = reader["Название"].ToString();
                            int cinemaCount = Convert.ToInt32(reader["Кол_Во_Кинотеатров"]);

                            decimal minPrice = reader["Минимальная_Цена"] != DBNull.Value
                                ? Convert.ToDecimal(reader["Минимальная_Цена"])
                                : 0;

                            EventCardControl card = new EventCardControl();
                            card.SetRowIndex(cardIndex);
                            cardIndex++;

                            // Проверяем если это кино (кинотеатров может быть больше 0, либо проверяем по логике залов)
                            // если площадок больше 1 или имя содержит "Кинотеатр"
                            string testVenue = reader["Площадка"].ToString();
                            bool isMovie = testVenue.ToLower().Contains("кинотеатр") || cinemaCount > 1;


                            if (isMovie)
                            {
                                // Передаем только Название, Количество кинотеатров и Минимальную цену
                                card.SetDataForMovie(title, cinemaCount, minPrice);

                                // При клике на кино — выводим сеансы в правое окно
                                card.Click += (s, e) => ShowMovieSessions(infoId, title, dtpDate.Value.Date);
                            }
                            else
                            {
                                string timeStr = Convert.ToDateTime(reader["Мин_Дата_Время"]).ToString("HH:mm");

                                // Передаем Название, Площадку (из базы), Время строкой и Минимальную цену
                                string venueName = reader["Площадка"] != DBNull.Value ? reader["Площадка"].ToString() : "Главная арена";
                                card.SetDataForEvent(title, venueName, timeStr, minPrice);

                                string evId = reader["Дефолтный_Событие_ИД"].ToString();
                                string plId = reader["Дефолтный_Планировка_ИД"].ToString();

                                // При клике на концерт — скрываем панель и открываем планировку напрямую
                                card.Click += (s, e) => {
                                    pnlRightDetails.Visible = false;
                                    MainForm map = new MainForm(evId, plId);
                                    map.ShowDialog();
                                };
                            }

                            flpEvents.Controls.Add(card);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Метод динамически заполняет правую панель списком Кинотеатров, Залов и Временем сеансов для выбранного фильма
        /// </summary>
        private void ShowMovieSessions(string infoId, string movieTitle, DateTime date)
        {
            flpSessions.Controls.Clear();
            lblRightTitle.Text = $"{movieTitle} ({date.ToString("dd.MM")})";
            pnlRightDetails.Visible = true;

            // Запрос вытаскивает все площадки, залы и время сеансов для конкретного фильма на эту дату
            string sessionQuery = @"
                SELECT 
                    S.Событие_ИД,
                    S.Планировка_ИД,
                    P.Название AS Кинотеатр,
                    S.Номер_Зала,
                    S.Дата_Время
                FROM События S
                JOIN Площадки P ON S.Площадка_ИД = P.Площадка_ИД
                WHERE S.Информация_ИД = @infoId AND CAST(S.Дата_Время AS DATE) = @date
                ORDER BY P.Название, S.Номер_Зала, S.Дата_Время";

            using (SqlConnection conn = new SqlConnection(DatabaseHelper.connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sessionQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@infoId", infoId);
                    cmd.Parameters.AddWithValue("@date", date);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        string currentCinema = "";

                        while (reader.Read())
                        {
                            string cinemaName = reader["Кинотеатр"].ToString();
                            string hallNumber = reader["Номер_Зала"].ToString();
                            DateTime dateTime = Convert.ToDateTime(reader["Дата_Время"]);
                            string eventId = reader["Событие_ИД"].ToString();
                            string planId = reader["Планировка_ИД"].ToString();

                            // Если поменялся кинотеатр — добавляем разделитель-заголовок площадки
                            if (cinemaName != currentCinema)
                            {
                                currentCinema = cinemaName;
                                Label lblCinemaGroup = new Label
                                {
                                    Text = "📍 " + currentCinema,
                                    Font = new Font("Arial", 10, FontStyle.Bold),
                                    ForeColor = Color.DarkSlateGray,
                                    Width = flpSessions.Width - 30,
                                    Height = 25,
                                    Margin = new Padding(0, 10, 0, 5)
                                };
                                flpSessions.Controls.Add(lblCinemaGroup);
                            }

                            // Создаем кнопку сеанса (Кнопка содержит Время + Номер зала)
                            Button btnSession = new Button
                            {
                                Text = $"{dateTime.ToString("HH:mm")}\n(Зал {hallNumber})",
                                Font = new Font("Arial", 9, FontStyle.Regular),
                                Width = 95,
                                Height = 45,
                                BackColor = Color.FromArgb(240, 244, 248),
                                FlatStyle = FlatStyle.Flat,
                                Margin = new Padding(3)
                            };
                            btnSession.FlatAppearance.BorderColor = Color.LightGray;

                            // При клике на конкретное время — открывается схема зала кинотеатра
                            btnSession.Click += (s, e) =>
                            {
                                MainForm map = new MainForm(eventId, planId);
                                map.ShowDialog();
                            };

                            flpSessions.Controls.Add(btnSession);
                        }
                    }
                }
            }

            if (flpSessions.Controls.Count == 0)
            {
                Label lblEmpty = new Label { Text = "Сеансов не найдено", ForeColor = Color.Gray, AutoSize = true };
                flpSessions.Controls.Add(lblEmpty);
            }
        }

        private void FormAfisha_DoubleClick(object sender, EventArgs e)
        {
            MapDigitizerForm map = new MapDigitizerForm();
            map.ShowDialog();
        }

        private void dox(object sender, EventArgs e)
        {
            LoadAfisha();
        }
        private void BtnLogin_Click(object sender, EventArgs e)
        {
            this.Hide();

            using (FormLogin loginForm = new FormLogin())
            {
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    this.Show();
                    BtnLogin.Visible = false;
                    BtnMyTickets.Visible = true;
                }
            }

            this.Show();
        }

        private void BtnMyTickets_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(UserSession.Login))
            {
                return;
            }

            FormMyTickets ticketsForm = new FormMyTickets();
            ticketsForm.ShowDialog();
        }
    }
}