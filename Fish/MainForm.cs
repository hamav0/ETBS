using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Fish
{
    public partial class MainForm : Form
    {
        public class ZoneData
        {
            public string ZoneId { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public string PolygonString { get; set; }
            public string TextOrientation { get; set; }
            public int FontSize { get; set; }
            public bool IsSelected { get; set; }

            public decimal MinPrice { get; set; }
            public decimal MaxPrice { get; set; }
            public int FreeSeatsCount { get; set; }

            public string CategoryName { get; set; }
            public int CategoryId { get; set; }
            public Color CategoryColor { get; set; }
        }

        public class Seat
        {
            public string Id { get; set; }
            public string ZoneId { get; set; }
            public string Row { get; set; }
            public string SeatNumber { get; set; }
            public int OriginalX { get; set; }
            public int OriginalY { get; set; }
            public Color Color { get; set; }
            public decimal Price { get; set; }
            public bool IsSelected { get; set; }
            public Rectangle CurrentBounds { get; set; }
        }

        // Класс для билетов, не привязанных к конкретным зонам на карте (входные)
        public class UnmappedTicket
        {
            public int CategoryId { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public decimal Price { get; set; }
            public Color Color { get; set; }
            public int SelectedCount { get; set; }
        }

        private List<ZoneData> _zones = new List<ZoneData>();
        private List<Seat> _seats = new List<Seat>();
        private List<UnmappedTicket> _unmappedTickets = new List<UnmappedTicket>();

        private float _scale = 1.0f;
        private bool _isDragging = false;
        private Point _startMousePos;
        private Point _startCanvasPos;

        public int _canvasOriginalWidth = 1200;
        public int _canvasOriginalHeight = 800;

        private string _eventId;
        private string _planId;

        private ZoneData _hoveredZone = null;
        private FlowLayoutPanel _cartPanel;

        private FlowLayoutPanel _unmappedPanel;

        public MainForm(string eventId, string planId)
        {
            InitializeComponent();
            pnlBottom.SendToBack();
            pnlMap.BringToFront();

            picCanvas.BackColor = Color.LightGray;

            // Корзина
            _cartPanel = new FlowLayoutPanel { AutoScroll = true, BackColor = Color.FromArgb(30, 30, 30), Padding = new Padding(10), Font = new Font("Arial", 10, FontStyle.Regular), Dock = DockStyle.Fill };
            pnlBottom.Controls.Add(_cartPanel);
            _cartPanel.BringToFront();

            _unmappedPanel = new FlowLayoutPanel { AutoScroll = true, BackColor = Color.WhiteSmoke, Padding = new Padding(10), Width = 250, Dock = DockStyle.Left, Visible = false };
            this.Controls.Add(_unmappedPanel); 

            _unmappedPanel.SendToBack();

            _eventId = eventId;
            _planId = planId;

            this.DoubleBuffered = true;
            typeof(Panel).InvokeMember("DoubleBuffered", System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, pnlMap, new object[] { true });

            LoadZones(_planId);
            LoadSeats(_eventId);
            LoadUnmappedTickets(_eventId);

            AdjustCanvasSize();
            SetupEvents();
            ApplyZoom(0);
        }

        private void SetupEvents()
        {
            btnZoomIn.Click += (s, e) => ApplyZoom(0.3f);
            btnZoomOut.Click += (s, e) => ApplyZoom(-0.3f);
            pnlMap.MouseWheel += (s, e) => ApplyZoom(e.Delta > 0 ? 0.1f : -0.1f);

            picCanvas.MouseDown += PicCanvas_MouseDown;
            picCanvas.MouseMove += PicCanvas_MouseMove;
            picCanvas.MouseUp += (s, e) => _isDragging = false;
            picCanvas.Paint += picCanvas_Paint;
            picCanvas.MouseClick += picCanvas_MouseClick;

            this.Resize += MainForm_Resize;
        }

        private void AdjustCanvasSize()
        {
            // Точный расчет рамки со всех 4-х сторон
            int minX = int.MaxValue, minY = int.MaxValue;
            int maxX = 0, maxY = 0;

            foreach (var seat in _seats)
            {
                if (seat.OriginalX < minX) minX = seat.OriginalX;
                if (seat.OriginalY < minY) minY = seat.OriginalY;
                if (seat.OriginalX > maxX) maxX = seat.OriginalX;
                if (seat.OriginalY > maxY) maxY = seat.OriginalY;
            }

            foreach (var zone in _zones)
            {
                if (zone.X < minX) minX = zone.X;
                if (zone.Y < minY) minY = zone.Y;
                if (zone.X > maxX) maxX = zone.X;
                if (zone.Y > maxY) maxY = zone.Y;
            }

            if (minX == int.MaxValue) { minX = 0; minY = 0; maxX = 1200; maxY = 800; }

            // Добавляем по 200px безопасного отступа для всплывающих подсказок (Tooltips) и красоты
            _canvasOriginalWidth = maxX + 200;
            _canvasOriginalHeight = maxY + 200;

            picCanvas.Width = (int)(_canvasOriginalWidth * _scale);
            picCanvas.Height = (int)(_canvasOriginalHeight * _scale);
        }

        private void LoadZones(string planId)
        {
            _zones.Clear();
            using (SqlConnection conn = new SqlConnection(DatabaseHelper.connectionString))
            {
                conn.Open();
                // Упрощенный запрос для геометрии
                string querySimple = "SELECT Зона_ИД, Название_Зоны, Тип_Зоны, Коорд_X, Коорд_Y, Координаты_Полигона, Ориентация_Текста, Размер_Текста FROM Планировки_Зон WHERE Планировка_ИД = @planId";

                using (SqlCommand cmd = new SqlCommand(querySimple, conn))
                {
                    cmd.Parameters.AddWithValue("@planId", planId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _zones.Add(new ZoneData
                            {
                                ZoneId = reader["Зона_ИД"].ToString(),
                                Name = reader["Название_Зоны"] != DBNull.Value ? reader["Название_Зоны"].ToString() : "",
                                Type = reader["Тип_Зоны"].ToString(),
                                X = Convert.ToInt32(reader["Коорд_X"]),
                                Y = Convert.ToInt32(reader["Коорд_Y"]),
                                PolygonString = reader["Координаты_Полигона"] != DBNull.Value ? reader["Координаты_Полигона"].ToString() : "",
                                TextOrientation = reader["Ориентация_Текста"] != DBNull.Value ? reader["Ориентация_Текста"].ToString() : "C-C", // По умолчанию Center-Center
                                FontSize = reader["Размер_Текста"] != DBNull.Value ? Convert.ToInt32(reader["Размер_Текста"]) : 12
                            });
                        }
                    }
                }

                // Подтягиваем цены и лимиты для Танцполов
                string queryZonePrices = @"
                SELECT 
                    SLZ.Зона_ИД, 
                    SLZ.Макс_Мест, 
                    CK.Цвет, 
                    CP.Цена, 
                    CK.Название AS CatName, 
                    CK.Категория_ИД
                FROM Событие_Лимиты_Зон SLZ
                INNER JOIN Планировки_Зон PZ ON PZ.Планировка_ИД = PZ.Планировка_ИД AND SLZ.Зона_ИД = PZ.Зона_ИД
                INNER JOIN Событие_Категории CK ON PZ.Название_Зоны = CK.Название AND CK.Событие_ИД = SLZ.Событие_ИД
                INNER JOIN Ценовая_Политика CP ON CK.Категория_ИД = CP.Категория_ИД AND CK.Событие_ИД = CP.Событие_ИД
                WHERE SLZ.Событие_ИД = @eventId";

                try
                {
                    using (SqlCommand cmdPrices = new SqlCommand(queryZonePrices, conn))
                    {
                        cmdPrices.Parameters.AddWithValue("@eventId", _eventId);
                        using (SqlDataReader r = cmdPrices.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                var z = _zones.FirstOrDefault(x => x.ZoneId == r["Зона_ИД"].ToString());
                                if (z != null)
                                {
                                    z.FreeSeatsCount = Convert.ToInt32(r["Макс_Мест"]);
                                    z.MinPrice = Convert.ToDecimal(r["Цена"]);
                                    z.MaxPrice = z.MinPrice;
                                    z.CategoryName = r["CatName"].ToString();
                                    z.CategoryColor = ColorTranslator.FromHtml(r["Цвет"].ToString());
                                    z.CategoryId = Convert.ToInt32(r["Категория_ИД"]);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке данных зон: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LoadSeats(string eventId)
        {
            _seats.Clear();
            using (SqlConnection conn = new SqlConnection(DatabaseHelper.connectionString))
            {
                conn.Open();
                string query = @"
                SELECT PM.Место_ИД, PM.Зона_ИД, PM.Коорд_X, PM.Коорд_Y, PM.Ряд, PM.Номер, CK.Цвет, CP.Цена
                FROM Планировки_Мест PM
                JOIN Событие_Места_Категории SMK ON PM.Место_ИД = SMK.Место_ИД
                JOIN Событие_Категории CK ON SMK.Категория_ИД = CK.Категория_ИД AND SMK.Событие_ИД = CK.Событие_ИД
                JOIN Ценовая_Политика CP ON CK.Категория_ИД = CP.Категория_ИД AND CK.Событие_ИД = CP.Событие_ИД
                WHERE SMK.Событие_ИД = @eventId 
                AND PM.Место_ИД NOT IN (SELECT Место_ИД FROM Билеты WHERE Событие_ИД = @eventId)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@eventId", eventId);
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            _seats.Add(new Seat
                            {
                                Id = r["Место_ИД"].ToString(),
                                ZoneId = r["Зона_ИД"] != DBNull.Value ? r["Зона_ИД"].ToString() : null,
                                Row = r["Ряд"] != DBNull.Value ? r["Ряд"].ToString() : "",
                                SeatNumber = r["Номер"] != DBNull.Value ? r["Номер"].ToString() : "",
                                OriginalX = Convert.ToInt32(r["Коорд_X"]),
                                OriginalY = Convert.ToInt32(r["Коорд_Y"]),
                                Color = ColorTranslator.FromHtml(r["Цвет"].ToString()),
                                Price = Convert.ToDecimal(r["Цена"])
                            });
                        }
                    }
                }
            }
            CalculateZoneStats();
        }

        private void LoadUnmappedTickets(string eventId)
        {
            _unmappedTickets.Clear();
            using (SqlConnection conn = new SqlConnection(DatabaseHelper.connectionString))
            {
                conn.Open();
                // Ищем категории, у которых есть цена для этого события, НО НЕТ привязки к Местам или Лимитам Зон
                string query = @"
                SELECT CK.Категория_ИД, CK.Название, CK.Цвет, CK.Описание, CP.Цена
                FROM Событие_Категории CK
                JOIN Ценовая_Политика CP ON CK.Категория_ИД = CP.Категория_ИД AND CK.Событие_ИД = CP.Событие_ИД
                WHERE CK.Событие_ИД = @eventId
                AND CK.Категория_ИД NOT IN (SELECT Категория_ИД FROM Событие_Места_Категории WHERE Событие_ИД = @eventId)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@eventId", eventId);
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            _unmappedTickets.Add(new UnmappedTicket
                            {
                                CategoryId = Convert.ToInt32(r["Категория_ИД"]),
                                Name = r["Название"].ToString(),
                                Description = r["Описание"].ToString(),
                                Price = Convert.ToDecimal(r["Цена"]),
                                Color = ColorTranslator.FromHtml(r["Цвет"].ToString()),
                                SelectedCount = 0
                            });
                        }
                    }
                }
            }
            RenderUnmappedPanel();
        }

        private void RenderUnmappedPanel()
        {
            if (_unmappedTickets.Count == 0)
            {
                _unmappedPanel.Visible = false;
                CenterCanvas();
                return;
            }

            _unmappedPanel.Visible = true;
            _unmappedPanel.Controls.Clear();

            Label lblTitle = new Label { Text = "Входные билеты\n(Без места)", Font = new Font("Arial", 12, FontStyle.Bold), AutoSize = true, Margin = new Padding(0, 0, 0, 15) };
            _unmappedPanel.Controls.Add(lblTitle);

            foreach (var tkt in _unmappedTickets)
            {
                Panel pnl = new Panel { Width = 220, Height = 100, BackColor = Color.White, Margin = new Padding(0, 0, 0, 10), BorderStyle = BorderStyle.FixedSingle };

                Panel colorBar = new Panel { Width = 10, Height = 100, BackColor = tkt.Color, Dock = DockStyle.Left };
                pnl.Controls.Add(colorBar);

                Label lblName = new Label { Text = tkt.Name, Font = new Font("Arial", 10, FontStyle.Bold), Location = new Point(20, 10), AutoSize = true };
                Label lblPrice = new Label { Text = $"{tkt.Price.ToString("0.##")} ₽", Font = new Font("Arial", 10, FontStyle.Regular), Location = new Point(20, 30), AutoSize = true };

                Button btnInfo = new Button { Text = "i", Size = new Size(25, 25), Location = new Point(185, 10), FlatStyle = FlatStyle.Flat, ForeColor = Color.Gray, Font = new Font("Georgia", 9, FontStyle.Italic | FontStyle.Bold) };
                btnInfo.FlatAppearance.BorderSize = 0;
                btnInfo.Click += (s, e) => MessageBox.Show(tkt.Description, $"Инфо: {tkt.Name}");

                Button btnMinus = new Button { Text = "-", Size = new Size(30, 30), Location = new Point(20, 60), FlatStyle = FlatStyle.Flat };
                Label lblCount = new Label { Text = "0", Font = new Font("Arial", 12, FontStyle.Bold), Location = new Point(60, 65), Width = 30, TextAlign = ContentAlignment.MiddleCenter };
                Button btnPlus = new Button { Text = "+", Size = new Size(30, 30), Location = new Point(100, 60), FlatStyle = FlatStyle.Flat };

                btnPlus.Click += (s, e) => { tkt.SelectedCount++; lblCount.Text = tkt.SelectedCount.ToString(); UpdateBuyButton(); };
                btnMinus.Click += (s, e) => { if (tkt.SelectedCount > 0) { tkt.SelectedCount--; lblCount.Text = tkt.SelectedCount.ToString(); UpdateBuyButton(); } };

                pnl.Controls.Add(lblName); pnl.Controls.Add(lblPrice); pnl.Controls.Add(btnInfo);
                pnl.Controls.Add(btnMinus); pnl.Controls.Add(lblCount); pnl.Controls.Add(btnPlus);

                _unmappedPanel.Controls.Add(pnl);
            }

            CenterCanvas();
        }


        private void CalculateZoneStats()
        {
            foreach (var zone in _zones)
            {
                var zoneSeats = _seats.Where(s => s.ZoneId == zone.ZoneId).ToList();
                if (zoneSeats.Any())
                {
                    zone.FreeSeatsCount = zoneSeats.Count;
                    zone.MinPrice = zoneSeats.Min(s => s.Price);
                    zone.MaxPrice = zoneSeats.Max(s => s.Price);
                }
            }
        }

        // парсинг строк "L-T" в StringFormat
        private StringFormat GetTextAlignment(string orientationCode)
        {
            StringFormat sf = new StringFormat();
            if (string.IsNullOrWhiteSpace(orientationCode)) { sf.Alignment = StringAlignment.Center; sf.LineAlignment = StringAlignment.Center; return sf; }

            string[] parts = orientationCode.Split('-');
            string horiz = parts.Length > 0 ? parts[0].ToUpper() : "C";
            string vert = parts.Length > 1 ? parts[1].ToUpper() : "C";

            // Horizontal
            if (horiz == "L") sf.Alignment = StringAlignment.Near;
            else if (horiz == "R") sf.Alignment = StringAlignment.Far;
            else sf.Alignment = StringAlignment.Center;

            // Vertical
            if (vert == "T") sf.LineAlignment = StringAlignment.Near;
            else if (vert == "B") sf.LineAlignment = StringAlignment.Far;
            else sf.LineAlignment = StringAlignment.Center;

            // Поддержка старой записи "Portrait"
            if (orientationCode.Contains("Portrait") || orientationCode == "Вертикально")
                sf.FormatFlags = StringFormatFlags.DirectionVertical;

            return sf;
        }

        private void picCanvas_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (Font seatFont = new Font("Arial", 8 * _scale, FontStyle.Bold))
            using (Pen pen = new Pen(Color.Silver, 1 * _scale))
            {
                // Отрисовка полигонов
                foreach (var zone in _zones)
                {
                    using (Font zoneFont = new Font("Arial", zone.FontSize * _scale, FontStyle.Bold))
                    {
                        if (zone.Type != "Текст" && !string.IsNullOrWhiteSpace(zone.PolygonString))
                        {
                            using (GraphicsPath path = ParsePath(zone.PolygonString, _scale))
                            {
                                if (path.PointCount > 0 && zone.Type != "Линия")
                                {
                                    g.FillPath(Brushes.White, path);
                                }

                                // Отрисовка Танцпола (с цветом категории)
                                if (zone.Type == "Танцпол")
                                {
                                    Color baseColor = zone.CategoryColor != Color.Empty ? zone.CategoryColor : Color.DeepSkyBlue;
                                    Brush b = zone.IsSelected ? Brushes.Gold : new SolidBrush(Color.FromArgb(40, baseColor));
                                    g.FillPath(b, path);
                                    g.DrawPath(new Pen(baseColor, 2 * _scale), path);
                                }
                                else if (zone.Type == "Линия") g.DrawPath(pen, path);
                            }
                        }
                    }
                }
                // Отрисовка текстов
                foreach (var zone in _zones)
                {
                    using (Font zoneFont = new Font("Arial", zone.FontSize * _scale, FontStyle.Bold))
                    {
                        if (!string.IsNullOrEmpty(zone.Name) && string.IsNullOrWhiteSpace(zone.PolygonString))
                        {
                            using (StringFormat sf = GetTextAlignment(zone.TextOrientation))
                            { // плюсуя Y - оно идёт вниз, плюсуя X - оно идёт влево
                                g.DrawString(zone.Name, zoneFont, Brushes.Black, (zone.X + 90) * _scale, (zone.Y + 42) * _scale, sf);
                            }
                        }
                    }
                }
                    // Отрисовка мест
                    float size = 30 * _scale;
                StringFormat textFormat = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

                foreach (var seat in _seats)
                {
                    float sx = seat.OriginalX * _scale;
                    float sy = seat.OriginalY * _scale;
                    seat.CurrentBounds = new Rectangle((int)sx, (int)sy, (int)size, (int)size);

                    if (seat.IsSelected)
                    {
                        g.FillEllipse(Brushes.Black, seat.CurrentBounds);
                        using (Brush textBrush = new SolidBrush(seat.Color))
                            g.DrawString(seat.SeatNumber, seatFont, textBrush, new RectangleF(sx, sy, size, size), textFormat);
                    }
                    else
                    {
                        using (Brush bgBrush = new SolidBrush(seat.Color))
                            g.FillEllipse(bgBrush, seat.CurrentBounds);
                    }
                    g.DrawEllipse(Pens.White, seat.CurrentBounds);
                }

                // Tooltips
                if (_hoveredZone != null && _hoveredZone.Type != "Линия" && _hoveredZone.Type != "Текст")
                {
                    DrawZoneTooltip(g, _hoveredZone);
                }
            }
        }

        private void DrawZoneTooltip(Graphics g, ZoneData zone)
        {
            // Показываем тултип, даже если мест 0, но это Зона для покупки (Танцпол)
            if (zone.FreeSeatsCount == 0 && zone.Type != "Танцпол") return;

            string priceText = zone.MinPrice == zone.MaxPrice
                ? $"{zone.MinPrice.ToString("0.##")} ₽"
                : $"от {zone.MinPrice.ToString("0.##")} ₽";

            string text = $"{zone.Name}\n{priceText}\nСвободно: {zone.FreeSeatsCount}";

            using (Font ttFont = new Font("Arial", 10, FontStyle.Regular))
            {
                SizeF size = g.MeasureString(text, ttFont);
                Point mousePos = picCanvas.PointToClient(Cursor.Position);

                RectangleF bgRect = new RectangleF(mousePos.X + 15, mousePos.Y + 15, size.Width + 20, size.Height + 20);

                g.FillRectangle(new SolidBrush(Color.FromArgb(220, 20, 20, 20)), bgRect);
                g.DrawRectangle(Pens.Gray, bgRect.X, bgRect.Y, bgRect.Width, bgRect.Height);
                g.DrawString(text, ttFont, Brushes.White, bgRect.X + 10, bgRect.Y + 10);
            }
        }

        private GraphicsPath ParsePath(string pathString, float scale)
        {
            GraphicsPath path = new GraphicsPath();
            if (string.IsNullOrWhiteSpace(pathString)) return path;
            try
            {
                string[] tokens = pathString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                PointF cur = new PointF(0, 0);
                int i = 0;
                while (i < tokens.Length)
                {
                    string cmd = tokens[i++];
                    if (cmd == "M")
                    {
                        cur = new PointF(float.Parse(tokens[i++]) * scale, float.Parse(tokens[i++]) * scale);
                        path.StartFigure();
                    }
                    else if (cmd == "L")
                    {
                        PointF next = new PointF(float.Parse(tokens[i++]) * scale, float.Parse(tokens[i++]) * scale);
                        path.AddLine(cur, next);
                        cur = next;
                    }
                    else if (cmd == "Z") path.CloseFigure();
                }
            }
            catch { }
            return path;
        }

        private void CenterCanvas()
        {
            if (pnlMap == null || picCanvas == null) return;
            int newLeft = picCanvas.Left; int newTop = picCanvas.Top;
            if (picCanvas.Width <= pnlMap.Width) newLeft = (pnlMap.Width - picCanvas.Width) / 2;
            else { if (newLeft > 0) newLeft = 0; if (newLeft < pnlMap.Width - picCanvas.Width) newLeft = pnlMap.Width - picCanvas.Width; }
            if (picCanvas.Height <= pnlMap.Height) newTop = (pnlMap.Height - picCanvas.Height) / 2;
            else { if (newTop > 0) newTop = 0; if (newTop < pnlMap.Height - picCanvas.Height) newTop = pnlMap.Height - picCanvas.Height; }
            picCanvas.Location = new Point(newLeft, newTop);
        }

        private void ApplyZoom(float delta)
        {
            _scale = Math.Max(0.4f, Math.Min(3.0f, _scale + delta));
            picCanvas.Width = (int)(_canvasOriginalWidth * _scale);
            picCanvas.Height = (int)(_canvasOriginalHeight * _scale);
            CenterCanvas();
            picCanvas.Invalidate();
        }

        private void PicCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) { _isDragging = true; _startMousePos = Control.MousePosition; _startCanvasPos = picCanvas.Location; }
        }

        private void PicCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                int dx = Control.MousePosition.X - _startMousePos.X; int dy = Control.MousePosition.Y - _startMousePos.Y;
                int newLeft = _startCanvasPos.X + dx; int newTop = _startCanvasPos.Y + dy;

                if (picCanvas.Width <= pnlMap.Width) newLeft = (pnlMap.Width - picCanvas.Width) / 2;
                else { if (newLeft > 0) newLeft = 0; if (newLeft < pnlMap.Width - picCanvas.Width) newLeft = pnlMap.Width - picCanvas.Width; }
                if (picCanvas.Height <= pnlMap.Height) newTop = (pnlMap.Height - picCanvas.Height) / 2;
                else { if (newTop > 0) newTop = 0; if (newTop < pnlMap.Height - picCanvas.Height) newTop = pnlMap.Height - picCanvas.Height; }

                picCanvas.Location = new Point(newLeft, newTop);
            }
            else
            {
                ZoneData currentHover = null;
                foreach (var zone in _zones)
                {
                    using (GraphicsPath path = ParsePath(zone.PolygonString, _scale))
                    {
                        if (path.IsVisible(e.Location)) { currentHover = zone; break; }
                    }
                }

                if (_hoveredZone != currentHover)
                {
                    _hoveredZone = currentHover;
                    picCanvas.Invalidate();
                }
            }
        }

        private void picCanvas_MouseClick(object sender, MouseEventArgs e)
        {
            bool changed = false;

            // Проверка клика по конкретным местам
            foreach (var seat in _seats)
            {
                if (seat.CurrentBounds.Contains(e.Location))
                {
                    int totalSelected = _seats.Count(s => s.IsSelected) + _zones.Count(z => z.IsSelected) + _unmappedTickets.Sum(t => t.SelectedCount);
                    if (!seat.IsSelected && totalSelected >= 6) { MessageBox.Show("Не более 6 билетов за один заказ."); return; }

                    seat.IsSelected = !seat.IsSelected;
                    changed = true;
                    break;
                }
            }

            // Проверка клика по целым зонам (Танцпол)
            if (!changed)
            {
                float originalX = e.Location.X / _scale;
                float originalY = e.Location.Y / _scale;
                PointF originalPoint = new PointF(originalX, originalY);

                foreach (var zone in _zones.Where(z => z.Type == "Танцпол" && z.FreeSeatsCount > 0))
                {
                    using (GraphicsPath path = ParsePath(zone.PolygonString, 1.0f)) 
                    {
                        if (path.IsVisible(originalPoint))
                        {
                            int totalSelected = _seats.Count(s => s.IsSelected) +
                                               _zones.Count(z => z.IsSelected) +
                                               _unmappedTickets.Sum(t => t.SelectedCount);

                            if (!zone.IsSelected && totalSelected >= 6)
                            {
                                MessageBox.Show("Не более 6 билетов в одни руки.");
                                return;
                            }

                            zone.IsSelected = !zone.IsSelected;
                            changed = true;

                            // Перерисовываем карту, чтобы отобразить выделение 
                            picCanvas.Invalidate();
                            break;
                        }
                    }
                }
            }

            if (changed)
            {
                UpdateBuyButton();
                picCanvas.Invalidate();
            }
        }

        private void UpdateBuyButton()
        {
            _cartPanel.Controls.Clear();
            decimal totalSum = 0;
            int count = 0;

            // 1. Добавляем выбранные места
            foreach (var s in _seats.Where(x => x.IsSelected))
            {
                count++; totalSum += s.Price;
                var zone = _zones.FirstOrDefault(z => z.ZoneId == s.ZoneId);
                AddTicketToCartPanel(s.Id, $"{zone?.Name ?? "Места"}\nРяд: {s.Row}, Место: {s.SeatNumber}", s.Price, () => { s.IsSelected = false; UpdateBuyButton(); picCanvas.Invalidate(); });
            }

            // 2. Добавляем выбранные зоны (Танцполы)
            foreach (var z in _zones.Where(x => x.IsSelected))
            {
                count++; totalSum += z.MinPrice;
                AddTicketToCartPanel(z.ZoneId, $"{z.Name}\nВходной билет", z.MinPrice, () => { z.IsSelected = false; UpdateBuyButton(); picCanvas.Invalidate(); });
            }

            // 3. Добавляем выбранные входные билеты (Без места)
            foreach (var tkt in _unmappedTickets.Where(x => x.SelectedCount > 0))
            {
                for (int i = 0; i < tkt.SelectedCount; i++)
                {
                    count++; totalSum += tkt.Price;
                    AddTicketToCartPanel($"UNM-{tkt.CategoryId}-{i}", $"{tkt.Name}\nВходной билет", tkt.Price, () => { tkt.SelectedCount--; RenderUnmappedPanel(); UpdateBuyButton(); });
                }
            }

            btnBuy.Enabled = count > 0;
            btnBuy.Text = count > 0 ? $"Купить: {count} билета (Сумма: {totalSum.ToString("0.##")} ₽)" : "Выберите места";
        }

        // Вспомогательный метод для генерации плашек в корзине
        private void AddTicketToCartPanel(string id, string text, decimal price, Action onRemove)
        {
            Panel pnlTicket = new Panel { Width = 200, Height = 65, BackColor = Color.FromArgb(50, 50, 50), Margin = new Padding(5) };
            Label lblInfo = new Label { Text = $"{text}\n{price.ToString("0.##")} ₽", AutoSize = true, Location = new Point(10, 8), ForeColor = Color.White, Font = new Font("Arial", 9) };
            Button btnRemove = new Button { Text = "✕", Size = new Size(25, 25), Location = new Point(170, 5), FlatStyle = FlatStyle.Flat, ForeColor = Color.IndianRed, BackColor = Color.Transparent, Cursor = Cursors.Hand, Font = new Font("Arial", 10, FontStyle.Bold) };
            btnRemove.FlatAppearance.BorderSize = 0;

            btnRemove.Click += (sender, e) => onRemove();

            pnlTicket.Controls.Add(lblInfo);
            pnlTicket.Controls.Add(btnRemove);
            _cartPanel.Controls.Add(pnlTicket);
        }
        private void btnBuy_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(UserSession.Login)) { if (new FormLogin().ShowDialog() != DialogResult.OK) return; }

            try
            {
                // Покупка мест
                foreach (var s in _seats.Where(x => x.IsSelected)) SaveOrderToDatabase(UserSession.Login, _eventId, s.Id, null, s.Price);

                // Покупка Танцполов
                foreach (var z in _zones.Where(x => x.IsSelected)) SaveOrderToDatabase(UserSession.Login, _eventId, null, z.ZoneId, z.MinPrice);

                // Покупка входных билетов
                foreach (var t in _unmappedTickets.Where(x => x.SelectedCount > 0))
                {
                    var mappedZone = _zones.FirstOrDefault(z => z.CategoryName == t.Name || z.Name == t.Name);

                    // Если зона НЕ нарисована, берём название категории (например, "Фан-зона") как Зона_ИД.
                    string zoneIdToSave = mappedZone != null ? mappedZone.ZoneId : t.Name;

                    for (int i = 0; i < t.SelectedCount; i++)
                    {
                        SaveOrderToDatabase(UserSession.Login, _eventId, null, zoneIdToSave, t.Price);
                    }
                }
                MessageBox.Show("Успешно!");
                this.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void SaveOrderToDatabase(string login, string eventId, string seatId, string zoneId, decimal price)
        {
            using (SqlConnection conn = new SqlConnection(DatabaseHelper.connectionString))
            {
                conn.Open();
                string query = "INSERT INTO Билеты (Заказ_ИД, Логин, Событие_ИД, Место_ИД, Зона_ИД, Сумма_Оплаты) VALUES (@id, @login, @ev, @seat, @zone, @p)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", "TKT-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper());
                    cmd.Parameters.AddWithValue("@login", login);
                    cmd.Parameters.AddWithValue("@ev", eventId);
                    cmd.Parameters.AddWithValue("@seat", (object)seatId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@zone", (object)zoneId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@p", price);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (pnlMap == null) return;
            btnZoomIn.Location = new Point(pnlMap.Width - 50, pnlMap.Height / 2 - 23);
            btnZoomOut.Location = new Point(pnlMap.Width - 50, pnlMap.Height / 2 + 23);
            CenterCanvas();
        }
    }
}