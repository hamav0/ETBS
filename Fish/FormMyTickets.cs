using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Fish
{
    public partial class FormMyTickets : Form
    {
        private TabControl tabViews;
        private CheckedListBox clbTickets;
        private MonthCalendar calPeriod;
        private RadioButton rbExportTab, rbExportPeriod, rbExportSelected;
        private Button btnExportTickets;
        private Label lblTitle;
        private List<TicketData> _myTickets = new List<TicketData>();

        // Внутренний класс для хранения собранных из БД данных
        private class TicketData
        {
            public string TicketId { get; set; }
            public string EventName { get; set; }
            public DateTime EventDate { get; set; }
            public string VenueName { get; set; }
            public string Address { get; set; }
            public string ZoneName { get; set; }
            public string Row { get; set; }
            public string SeatNum { get; set; }
            public decimal Price { get; set; }

            public override string ToString()
            {
                string place = !string.IsNullOrEmpty(Row) ? $"Ряд {Row}, Место {SeatNum}" : "Входной (Без места)";
                return $"{EventDate:dd.MM.yyyy HH:mm} | {EventName} | {place}";
            }
        }

        public FormMyTickets()
        {
            this.Text = "Мои билеты";
            this.Size = new Size(820, 480);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.WhiteSmoke;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            Font mainFont = new Font("Arial", 10, FontStyle.Regular);
            Font boldFont = new Font("Arial", 10, FontStyle.Bold);

            lblTitle = new Label { Text = $"Управление билетами пользователя: {UserSession.Login}", Font = new Font("Arial", 12, FontStyle.Bold), Top = 15, Left = 20, AutoSize = true };
            this.Controls.Add(lblTitle);

            // Вкладки
            tabViews = new TabControl { Top = 50, Left = 20, Width = 500, Height = 360, Font = mainFont };
            tabViews.TabPages.Add("Актуальные");
            tabViews.TabPages.Add("В прошлом");
            tabViews.TabPages.Add("Все");
            tabViews.SelectedIndexChanged += TabViews_SelectedIndexChanged;
            this.Controls.Add(tabViews);

            // CheckedListBox один на все вкладки
            clbTickets = new CheckedListBox { Dock = DockStyle.Fill, CheckOnClick = true, Font = mainFont, IntegralHeight = false };

            // Календарь (MonthCalendar)
            calPeriod = new MonthCalendar { Top = 50, Left = 540, MaxSelectionCount = 365 };
            this.Controls.Add(calPeriod);

            // Панель параметров печати
            GroupBox gbExport = new GroupBox { Text = "Параметры печати", Top = 220, Left = 540, Width = 230, Height = 105, Font = boldFont };

            rbExportTab = new RadioButton { Text = "Все (текущая вкладка)", Top = 25, Left = 10, Checked = true, AutoSize = true, Font = mainFont };
            rbExportPeriod = new RadioButton { Text = "За период (Календарь)", Top = 50, Left = 10, AutoSize = true, Font = mainFont };
            rbExportSelected = new RadioButton { Text = "Выбранное (Галочки)", Top = 75, Left = 10, AutoSize = true, Font = mainFont };

            gbExport.Controls.Add(rbExportTab);
            gbExport.Controls.Add(rbExportPeriod);
            gbExport.Controls.Add(rbExportSelected);
            this.Controls.Add(gbExport);

            // Кнопка генерации txt
            btnExportTickets = new Button { Text = "🖨 Сгенерировать чеки (TXT)", Top = 340, Left = 540, Width = 230, Height = 70, Font = boldFont, BackColor = Color.LightGreen, FlatStyle = FlatStyle.Flat };
            btnExportTickets.Click += BtnExportTickets_Click;
            this.Controls.Add(btnExportTickets);

            UpdateCalendarLimits();
            LoadTickets();
        }

        private void TabViews_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateCalendarLimits();
            LoadTickets();
        }

        private void UpdateCalendarLimits()
        {
            // Сначала сбрасываем лимиты, чтобы избежать конфликтов Min > Max при переключении
            calPeriod.MinDate = new DateTime(1900, 1, 1);
            calPeriod.MaxDate = new DateTime(9998, 12, 31);

            if (tabViews.SelectedIndex == 0) // Актуальные
            {
                calPeriod.MinDate = DateTime.Today;
            }
            else if (tabViews.SelectedIndex == 1) // В прошлом
            {
                calPeriod.MaxDate = DateTime.Today.AddDays(-1);
            }
            // Для вкладки "Все" (индекс 2) лимиты остаются сброшенными
        }

        private void LoadTickets()
        {
            if (string.IsNullOrEmpty(UserSession.Login)) return;

            // Перемещаем элемент управления CheckedListBox на текущую активную вкладку
            if (tabViews.SelectedTab != null) tabViews.SelectedTab.Controls.Add(clbTickets);

            _myTickets.Clear();
            clbTickets.Items.Clear();

            using (SqlConnection conn = new SqlConnection(DatabaseHelper.connectionString))
            {
                string query = @"
                    SELECT 
                        B.Заказ_ИД, B.Сумма_Оплаты, S.Дата_Время, 
                        I.Название AS EventName, P.Название AS VenueName, P.Адрес,
                        PM.Ряд, PM.Номер, PZ.Название_Зоны
                    FROM Билеты B
                    JOIN События S ON B.Событие_ИД = S.Событие_ИД
                    JOIN Информация I ON S.Информация_ИД = I.Информация_ИД
                    JOIN Площадки P ON S.Площадка_ИД = P.Площадка_ИД
                    LEFT JOIN Планировки_Мест PM ON B.Место_ИД = PM.Место_ИД AND S.Планировка_ИД = PM.Планировка_ИД
                    LEFT JOIN Планировки_Зон PZ ON (B.Зона_ИД = PZ.Зона_ИД OR PM.Зона_ИД = PZ.Зона_ИД) AND S.Планировка_ИД = PZ.Планировка_ИД
                    WHERE B.Логин = @Login ";

                // Настраиваем условия и сортировку в зависимости от выбранной вкладки
                if (tabViews.SelectedIndex == 0) // Актуальные
                    query += " AND S.Дата_Время >= GETDATE() ORDER BY S.Дата_Время ASC";
                else if (tabViews.SelectedIndex == 1) // В прошлом
                    query += " AND S.Дата_Время < GETDATE() ORDER BY S.Дата_Время DESC";
                else // Все билеты
                    query += " ORDER BY S.Дата_Время ASC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Login", UserSession.Login);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var t = new TicketData
                            {
                                TicketId = reader["Заказ_ИД"].ToString(),
                                Price = Convert.ToDecimal(reader["Сумма_Оплаты"]),
                                EventDate = Convert.ToDateTime(reader["Дата_Время"]),
                                EventName = reader["EventName"].ToString(),
                                VenueName = reader["VenueName"].ToString(),
                                Address = reader["Адрес"].ToString(),
                                ZoneName = reader["Название_Зоны"] != DBNull.Value ? reader["Название_Зоны"].ToString() : "",
                                Row = reader["Ряд"] != DBNull.Value ? reader["Ряд"].ToString() : "",
                                SeatNum = reader["Номер"] != DBNull.Value ? reader["Номер"].ToString() : ""
                            };
                            _myTickets.Add(t);
                            clbTickets.Items.Add(t);
                        }
                    }
                }
            }

            if (_myTickets.Count == 0)
            {
                clbTickets.Items.Add("Нет данных для отображения.");
                clbTickets.Enabled = false;
                btnExportTickets.Enabled = false;
            }
            else
            {
                clbTickets.Enabled = true;
                btnExportTickets.Enabled = true;
            }
        }

        /// ==========================================
        /// ЛОГИКА ГЕНЕРАЦИИ ТЕКСТОВОГО ФАЙЛА
        /// ==========================================
        private void BtnExportTickets_Click(object sender, EventArgs e)
        {
            List<TicketData> ticketsToExport = new List<TicketData>();

            // какие билеты экспортировать, исходя из выбранного RadioButton
            if (rbExportSelected.Checked)
            {
                foreach (var item in clbTickets.CheckedItems)
                {
                    if (item is TicketData t) ticketsToExport.Add(t);
                }
                if (ticketsToExport.Count == 0)
                {
                    MessageBox.Show("Вы не отметили галочками ни одного билета!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            else if (rbExportPeriod.Checked)
            {
                DateTime startDate = calPeriod.SelectionStart.Date;
                DateTime endDate = calPeriod.SelectionEnd.Date.AddDays(1).AddTicks(-1); // До конца выбранного дня

                ticketsToExport = _myTickets.Where(t => t.EventDate >= startDate && t.EventDate <= endDate).ToList();

                if (ticketsToExport.Count == 0)
                {
                    MessageBox.Show("В выбранном периоде нет билетов для печати.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            else // rbExportTab.Checked (Все билеты из текущей вкладки)
            {
                ticketsToExport = new List<TicketData>(_myTickets);
            }

            // формируем файл
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Text Files (*.txt)|*.txt";
                sfd.FileName = $"Чеки_Билетов_{DateTime.Now:yyyyMMdd_HHmm}.txt";
                sfd.Title = "Сохранить билеты как";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    StringBuilder fileContent = new StringBuilder();

                    foreach (var ticket in ticketsToExport)
                    {
                        fileContent.AppendLine(GenerateSingleTicketTxt(ticket));
                        fileContent.AppendLine(); // Пустая строка между билетами
                    }

                    File.WriteAllText(sfd.FileName, fileContent.ToString(), Encoding.UTF8);
                    MessageBox.Show($"Успешно сгенерировано чеков: {ticketsToExport.Count} шт.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        // формирование рамки ровно 52 символа в ширину
        private string GenerateSingleTicketTxt(TicketData t)
        {
            int totalWidth = 52;
            int innerWidth = totalWidth - 4; // "# " и " #" занимают 4 символа
            StringBuilder sb = new StringBuilder();

            string borderLine = new string('#', totalWidth);
            sb.AppendLine(borderLine);

            // Локальная функция для добавления строки с автоматическим переносом слов
            Action<string> AddLine = (text) =>
            {
                List<string> lines = WrapText(text, innerWidth);
                foreach (var line in lines)
                {
                    sb.AppendLine($"# {line.PadRight(innerWidth)} #");
                }
            };

            AddLine("Билет:");
            AddLine(t.TicketId);
            AddLine("");
            AddLine("Описание:");
            AddLine($"Событие: {t.EventName}");
            AddLine($"Дата: {t.EventDate:dd MMMM yyyy, HH:mm}");
            AddLine($"Площадка: {t.VenueName}");
            AddLine($"Адрес: {t.Address}");

            string placeDesc = "";
            if (!string.IsNullOrEmpty(t.ZoneName)) placeDesc += $"Зона: {t.ZoneName}. ";
            if (!string.IsNullOrEmpty(t.Row)) placeDesc += $"Ряд {t.Row}, Место {t.SeatNum}.";
            else placeDesc += "Входной билет без привязки к месту.";
            AddLine($"Размещение: {placeDesc}");

            AddLine($"Стоимость: {t.Price.ToString("0.##")} руб.");

            sb.AppendLine(borderLine);
            return sb.ToString();
        }

        // Вспомогательный алгоритм что разбивает длинный текст чтобы не разрывать слова
        private List<string> WrapText(string text, int maxLineLength)
        {
            List<string> lines = new List<string>();
            string[] words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string currentLine = "";

            foreach (string word in words)
            {
                if ((currentLine + word).Length > maxLineLength)
                {
                    if (currentLine.Length > 0)
                    {
                        lines.Add(currentLine.Trim());
                        currentLine = "";
                    }

                    // Если одно слово длиннее лимита - режем принудительно
                    if (word.Length > maxLineLength)
                    {
                        string longWord = word;
                        while (longWord.Length > maxLineLength)
                        {
                            lines.Add(longWord.Substring(0, maxLineLength));
                            longWord = longWord.Substring(maxLineLength);
                        }
                        currentLine = longWord + " ";
                    }
                    else
                    {
                        currentLine = word + " ";
                    }
                }
                else
                {
                    currentLine += word + " ";
                }
            }
            if (currentLine.Length > 0) lines.Add(currentLine.Trim());

            if (lines.Count > 140) lines = lines.GetRange(0, 140);
            return lines;
        }
    }
}