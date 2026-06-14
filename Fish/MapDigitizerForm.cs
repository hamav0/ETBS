using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Fish
{
    // --- МОДЕЛИ ДАННЫХ ---
    public class PlacedZone
    {
        public string PlanId, ZoneId, Name, Type, Orientation;
        public int FontSize;
        public Point Center;
        public List<Point> Points = new List<Point>();
        public int? CategoryId;
        public int CapacityLimit;
    }

    public class PlacedSeat
    {
        public string PlanId, ZoneId, Row, Num;
        public Point Location;
        public int? CategoryId;
    }

    public class EventCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Color Color { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; } 
    }

    // Класс для выпадающего списка тегов из БД
    public class DbTagItem
    {
        public string Id { get; set; }
        public string Display { get; set; }
    }

    public partial class MapDigitizerForm : Form
    {
        private Panel pnlLeft, pnlMap;
        private PictureBox picCanvas;
        private TabControl tabMode;
        private Point? _measurePoint = null;

        // Вкладка 1: Планировка
        private TextBox txtPlanId, txtZoneId, txtZoneName, txtSeatRow, txtSeatNum;
        private NumericUpDown numFontSize;
        private ComboBox cmbTextOrientation;
        private RadioButton rbZones, rbSeats, rbText, rbDanse;
        private CheckBox chkOrtho, chkAutoStep;
        private Button btnFinishZone;
        private NumericUpDown numMaxStep;

        // Вкладка 2: Событие и Цены
        private TextBox txtEventId, txtInfoId;
        private NumericUpDown numVenueId;
        private DateTimePicker dtpEventDate;
        private FlowLayoutPanel pnlCategories;
        private TextBox txtCatName, txtCatDesc;
        private NumericUpDown numCatPrice, numZoneCapacity;
        private NumericUpDown numRowFrom, numRowTo, numSeatFrom, numSeatTo; // Для массового назначения
        private Button btnCatColor, btnAddCategory, btnRemoveCategory, btnAssignRange;
        private List<EventCategory> _categories = new List<EventCategory>();
        private int _activeCategoryId = -1;
        private Color _currentPickedColor = Color.DeepSkyBlue;
        private Stack<Action> _undoEventStack = new Stack<Action>();

        // Вкладка 3: Генерация БД
        private ComboBox cmbLinkType, cmbLinkTagDb;
        private TextBox txtLinkMainId;

        // Общее
        private RichTextBox rtbSQL;
        private CheckBox chkGenVenue, chkGenInfo, chkGenEvent, chkGenLayout, chkGenPrices;
        private Button btnGenerateMasterSQL;
        private NumericUpDown numBlueprintScale;
        private List<PlacedZone> _zones = new List<PlacedZone>();
        private List<PlacedSeat> _seats = new List<PlacedSeat>();
        private List<Point> _currentPolygon = new List<Point>();
        private Image _originalImage;
        private float _scale = 1.0f;
        private float _blueprintScale = 1.0f;
        private Point _mousePosReal;

        private const int SEAT_SIZE = 30;

        public MapDigitizerForm()
        {
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "Дигитайзер v4.2 (Мастер Генерации и Авто-Сетка)";
            this.Size = new Size(1600, 950);
            this.StartPosition = FormStartPosition.CenterScreen;

            Font defaultFont = new Font("Arial", 9, FontStyle.Regular);
            Font boldFont = new Font("Arial", 9, FontStyle.Bold);

            pnlLeft = new Panel { Dock = DockStyle.Left, Width = 400, BackColor = Color.FromArgb(240, 240, 245), BorderStyle = BorderStyle.FixedSingle };
            this.Controls.Add(pnlLeft);

            pnlMap = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.DarkGray };
            this.Controls.Add(pnlMap);
            pnlMap.BringToFront();

            picCanvas = new PictureBox { SizeMode = PictureBoxSizeMode.StretchImage, Location = new Point(0, 0) };
            picCanvas.MouseClick += PicCanvas_MouseClick;
            picCanvas.MouseMove += PicCanvas_MouseMove;
            picCanvas.Paint += PicCanvas_Paint;
            pnlMap.Controls.Add(picCanvas);

            pnlMap.MouseWheel += (s, e) =>
            {
                if (Control.ModifierKeys == Keys.Control && _originalImage != null)
                {
                    ApplyZoom(e.Delta > 0 ? 0.1f : -0.1f);
                    ((HandledMouseEventArgs)e).Handled = true;
                }
            };

            tabMode = new TabControl { Top = 10, Left = 10, Width = 380, Height = 480, Font = defaultFont };
            TabPage tabLayout = new TabPage("1. Планировка");
            TabPage tabEvent = new TabPage("2. Цены");
            TabPage tabDataGen = new TabPage("3. Справочники/Связи");
            tabMode.TabPages.Add(tabLayout);
            tabMode.TabPages.Add(tabEvent);
            tabMode.TabPages.Add(tabDataGen);
            pnlLeft.Controls.Add(tabMode);

            // ==========================================
            // Вкладка 1: ПЛАНИРОВКА
            // ==========================================
            tabLayout.Controls.Add(new Label { Text = "Планировка_ИД:", Top = 10, Left = 10, AutoSize = true, Font = defaultFont });
            txtPlanId = new TextBox { Text = "CROP-ARENA-01", Top = 25, Left = 10, Width = 350, Font = defaultFont };
            tabLayout.Controls.Add(txtPlanId);

            GroupBox gbMode = new GroupBox { Text = "Инструменты", Top = 60, Left = 10, Width = 350, Height = 100, Font = boldFont };
            rbZones = new RadioButton { Text = "Зоны (Полигоны)", Top = 20, Left = 10, Checked = true, AutoSize = true, Font = defaultFont };
            rbSeats = new RadioButton { Text = "Места (Точки)", Top = 45, Left = 10, AutoSize = true, Font = defaultFont };
            rbText = new RadioButton { Text = "Текст (Статика)", Top = 70, Left = 10, AutoSize = true, Font = defaultFont };
            rbDanse = new RadioButton { Text = "Зона без мест (Танцполы)", Top = 70, Left = 180, AutoSize = true, Font = defaultFont };
            rbText.CheckedChanged += rbText_CheckedChanged;
            chkOrtho = new CheckBox { Text = "Симметрия X/Y", Top = 20, Left = 180, AutoSize = true, Checked = true, Font = defaultFont };
            gbMode.Controls.Add(rbZones); gbMode.Controls.Add(rbSeats); gbMode.Controls.Add(rbText); gbMode.Controls.Add(rbDanse); gbMode.Controls.Add(chkOrtho);
            tabLayout.Controls.Add(gbMode);

            GroupBox gbZones = new GroupBox { Text = "Параметры Зоны / Текста", Top = 170, Left = 10, Width = 350, Height = 130, Font = boldFont };
            gbZones.Controls.Add(new Label { Text = "ИД Зоны:", Top = 20, Left = 10, AutoSize = true, Font = defaultFont });
            txtZoneId = new TextBox { Text = "Z-BALCONY", Top = 35, Left = 10, Width = 140, Font = defaultFont };
            gbZones.Controls.Add(txtZoneId);
            gbZones.Controls.Add(new Label { Text = "Название:", Top = 20, Left = 160, AutoSize = true, Font = defaultFont });
            txtZoneName = new TextBox { Text = "Балкон", Top = 35, Left = 160, Width = 180, Font = defaultFont };
            gbZones.Controls.Add(txtZoneName);
            gbZones.Controls.Add(new Label { Text = "Шрифт:", Top = 65, Left = 10, AutoSize = true, Font = defaultFont });
            numFontSize = new NumericUpDown { Top = 80, Left = 10, Width = 70, Minimum = 5, Maximum = 72, Value = 12, Font = defaultFont };
            gbZones.Controls.Add(numFontSize);
            gbZones.Controls.Add(new Label { Text = "Ориентация:", Top = 65, Left = 100, AutoSize = true, Font = defaultFont });
            cmbTextOrientation = new ComboBox { Top = 80, Left = 100, Width = 70, DropDownStyle = ComboBoxStyle.DropDownList, Font = defaultFont };
            cmbTextOrientation.Items.AddRange(new string[] { "L-T", "C-T", "R-T", "L-C", "C-C", "R-C", "L-B", "C-B", "R-B" });
            cmbTextOrientation.SelectedIndex = 0;
            cmbTextOrientation.SelectedIndexChanged += cmbTextOrientation_SelectedIndexChanged;
            gbZones.Controls.Add(cmbTextOrientation);
            btnFinishZone = new Button { Text = "💾 Сохранить полигон", Top = 80, Left = 180, Width = 160, Height = 25, BackColor = Color.LightGoldenrodYellow, FlatStyle = FlatStyle.Flat, Font = defaultFont };
            btnFinishZone.Click += BtnFinishZone_Click;
            gbZones.Controls.Add(btnFinishZone);
            tabLayout.Controls.Add(gbZones);

            GroupBox gbSeats = new GroupBox { Text = "Параметры мест", Top = 310, Left = 10, Width = 350, Height = 80, Font = boldFont };
            gbSeats.Controls.Add(new Label { Text = "Ряд:", Top = 20, Left = 10, AutoSize = true, Font = defaultFont });
            txtSeatRow = new TextBox { Text = "1", Top = 40, Left = 10, Width = 60, Font = defaultFont };
            gbSeats.Controls.Add(txtSeatRow);
            gbSeats.Controls.Add(new Label { Text = "Место (+1):", Top = 20, Left = 80, AutoSize = true, Font = defaultFont });
            txtSeatNum = new TextBox { Text = "1", Top = 40, Left = 80, Width = 70, Font = defaultFont };
            gbSeats.Controls.Add(txtSeatNum);

            gbSeats.Controls.Add(new Label { Text = "Макс. шаг:", Top = 20, Left = 160, AutoSize = true, Font = defaultFont });
            numMaxStep = new NumericUpDown { Top = 40, Left = 160, Width = 60, Minimum = 1, Maximum = 1000, Value = 50, DecimalPlaces = 2, Increment = 0.05m, Font = defaultFont };
            numMaxStep.ValueChanged += (s, e) => picCanvas.Invalidate();
            numMaxStep.MouseWheel += numMaxStep_MouseWheel;
            gbSeats.Controls.Add(numMaxStep);

            chkAutoStep = new CheckBox { Text = "Автовычисление", Top = 40, Left = 230, AutoSize = true, Font = defaultFont };
            gbSeats.Controls.Add(chkAutoStep);
            tabLayout.Controls.Add(gbSeats);

            Button btnUndo = new Button { Text = "↩ Отменить последнюю точку/зону", Top = 400, Left = 10, Width = 350, Height = 30, BackColor = Color.MistyRose, FlatStyle = FlatStyle.Flat, Font = defaultFont };
            btnUndo.Click += BtnUndo_Click;
            tabLayout.Controls.Add(btnUndo);


            // ==========================================
            // Вкладка 2: ЦЕНЫ И НАЗНАЧЕНИЕ
            // ==========================================
            Panel pnlEventScroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            tabEvent.Controls.Add(pnlEventScroll);

            GroupBox gbCat = new GroupBox { Text = "Управление Категориями Цен", Top = 10, Left = 5, Width = 345, Height = 175, Font = boldFont };
            gbCat.Controls.Add(new Label { Text = "Название:", Top = 20, Left = 10, AutoSize = true, Font = defaultFont });
            txtCatName = new TextBox { Text = "VIP", Top = 35, Left = 10, Width = 100, Font = defaultFont };
            gbCat.Controls.Add(txtCatName);

            gbCat.Controls.Add(new Label { Text = "Цена (₽):", Top = 20, Left = 120, AutoSize = true, Font = defaultFont });
            numCatPrice = new NumericUpDown { Top = 35, Left = 120, Width = 80, Minimum = 0, Maximum = 1000000, Value = 5000, Font = defaultFont };
            gbCat.Controls.Add(numCatPrice);

            gbCat.Controls.Add(new Label { Text = "Описание:", Top = 60, Left = 10, AutoSize = true, Font = defaultFont });
            txtCatDesc = new TextBox { Text = "Описание...", Top = 75, Left = 10, Width = 190, Font = defaultFont };
            gbCat.Controls.Add(txtCatDesc);

            btnCatColor = new Button { Top = 34, Left = 210, Width = 30, Height = 23, BackColor = _currentPickedColor, FlatStyle = FlatStyle.Flat };
            btnCatColor.Click += (s, e) => { using (ColorDialog cd = new ColorDialog()) { if (cd.ShowDialog() == DialogResult.OK) { _currentPickedColor = cd.Color; btnCatColor.BackColor = _currentPickedColor; } } };
            gbCat.Controls.Add(btnCatColor);

            btnAddCategory = new Button { Text = "Добавить", Top = 34, Left = 245, Width = 90, Height = 23, BackColor = Color.LightGreen, FlatStyle = FlatStyle.Flat, Font = defaultFont };
            btnAddCategory.Click += BtnAddCategory_Click;
            gbCat.Controls.Add(btnAddCategory);

            btnRemoveCategory = new Button { Text = "Удалить посл.", Top = 74, Left = 210, Width = 125, Height = 23, BackColor = Color.LightCoral, FlatStyle = FlatStyle.Flat, Font = defaultFont };
            btnRemoveCategory.Click += BtnRemoveCategory_Click;
            gbCat.Controls.Add(btnRemoveCategory);

            pnlCategories = new FlowLayoutPanel { Top = 105, Left = 10, Width = 325, Height = 60, AutoScroll = true, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            gbCat.Controls.Add(pnlCategories);
            pnlEventScroll.Controls.Add(gbCat);

            GroupBox gbAssignRange = new GroupBox { Text = "Массовое назначение мест", Top = 195, Left = 5, Width = 345, Height = 110, Font = boldFont };
            gbAssignRange.Controls.Add(new Label { Text = "Ряды с:", Top = 20, Left = 10, AutoSize = true, Font = defaultFont });
            numRowFrom = new NumericUpDown { Top = 35, Left = 10, Width = 50, Minimum = 1, Value = 1, Font = defaultFont };
            gbAssignRange.Controls.Add(numRowFrom);
            gbAssignRange.Controls.Add(new Label { Text = "по:", Top = 20, Left = 65, AutoSize = true, Font = defaultFont });
            numRowTo = new NumericUpDown { Top = 35, Left = 65, Width = 50, Minimum = 1, Value = 10, Font = defaultFont };
            gbAssignRange.Controls.Add(numRowTo);

            gbAssignRange.Controls.Add(new Label { Text = "Места с:", Top = 20, Left = 130, AutoSize = true, Font = defaultFont });
            numSeatFrom = new NumericUpDown { Top = 35, Left = 130, Width = 50, Minimum = 1, Value = 1, Font = defaultFont };
            gbAssignRange.Controls.Add(numSeatFrom);
            gbAssignRange.Controls.Add(new Label { Text = "по:", Top = 20, Left = 185, AutoSize = true, Font = defaultFont });
            numSeatTo = new NumericUpDown { Top = 35, Left = 185, Width = 50, Minimum = 1, Value = 50, Font = defaultFont };
            gbAssignRange.Controls.Add(numSeatTo);

            btnAssignRange = new Button { Text = "Назначить выбранную категорию", Top = 65, Left = 10, Width = 320, Height = 35, BackColor = Color.LightSkyBlue, FlatStyle = FlatStyle.Flat, Font = defaultFont };
            btnAssignRange.Click += BtnAssignRange_Click;
            gbAssignRange.Controls.Add(btnAssignRange);
            pnlEventScroll.Controls.Add(gbAssignRange);

            GroupBox gbAssign = new GroupBox { Text = "Лимиты Зон (Танцпол)", Top = 315, Left = 5, Width = 345, Height = 65, Font = boldFont };
            gbAssign.Controls.Add(new Label { Text = "Лимит (для клика по зоне):", Top = 25, Left = 10, AutoSize = true, Font = defaultFont });
            numZoneCapacity = new NumericUpDown { Top = 23, Left = 200, Width = 100, Minimum = 1, Maximum = 10000, Value = 300, Font = defaultFont };
            gbAssign.Controls.Add(numZoneCapacity);
            pnlEventScroll.Controls.Add(gbAssign);

            Button btnUndoEvent = new Button { Text = "↩ Отменить последнюю раскраску", Top = 390, Left = 5, Width = 345, Height = 30, BackColor = Color.MistyRose, FlatStyle = FlatStyle.Flat, Font = defaultFont };
            btnUndoEvent.Click += BtnUndo_Click;
            pnlEventScroll.Controls.Add(btnUndoEvent);


            // ==========================================
            // Вкладка 3: СПРАВОЧНИКИ И СВЯЗИ
            // ==========================================
            Panel pnlDataScroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            tabDataGen.Controls.Add(pnlDataScroll);

            GroupBox gbDict = new GroupBox { Text = "Создание справочника (Теги)", Top = 10, Left = 5, Width = 345, Height = 180, Font = boldFont };
            ComboBox cmbDictType = new ComboBox { Top = 20, Left = 10, Width = 320, DropDownStyle = ComboBoxStyle.DropDownList, Font = defaultFont };
            cmbDictType.Items.AddRange(new string[] { "Жанры", "Актеры", "Режиссеры", "Страны", "Дистрибьюторы", "Форматы", "Тип событий", "Личности" });

            gbDict.Controls.Add(new Label { Text = "Название/Имя:", Top = 50, Left = 10, AutoSize = true, Font = defaultFont });
            TextBox txtDictName = new TextBox { Top = 65, Left = 10, Width = 320, Font = defaultFont };

            Label lblDictId = new Label { Text = "ИД (Личность):", Top = 95, Left = 10, AutoSize = true, Font = defaultFont, Visible = false };
            TextBox txtDictId = new TextBox { Top = 110, Left = 10, Width = 120, Font = defaultFont, Visible = false };
            Label lblDictRole = new Label { Text = "Роль:", Top = 95, Left = 140, AutoSize = true, Font = defaultFont, Visible = false };
            TextBox txtDictRole = new TextBox { Top = 110, Left = 140, Width = 190, Font = defaultFont, Visible = false };

            cmbDictType.SelectedIndexChanged += (s, e) => {
                bool isPerson = cmbDictType.SelectedItem.ToString() == "Личности";
                lblDictId.Visible = txtDictId.Visible = isPerson;
                lblDictRole.Visible = txtDictRole.Visible = isPerson;
            };
            cmbDictType.SelectedIndex = 0;

            Button btnGenDict = new Button { Text = "Сгенерировать SQL Тега (с возвратом ID)", Top = 140, Left = 10, Width = 320, Height = 30, BackColor = Color.LightYellow, FlatStyle = FlatStyle.Flat, Font = defaultFont };
            btnGenDict.Click += (s, e) => {
                string t = cmbDictType.SelectedItem.ToString(); string v = txtDictName.Text; string sql = "";
                switch (t)
                {
                    case "Жанры": sql = $"INSERT INTO Жанры (Название) VALUES (N'{v}');\nSELECT SCOPE_IDENTITY();"; break;
                    case "Актеры": sql = $"INSERT INTO Актеры (Полное_Имя) VALUES (N'{v}');\nSELECT SCOPE_IDENTITY();"; break;
                    case "Режиссеры": sql = $"INSERT INTO Режиссеры (Полное_Имя) VALUES (N'{v}');\nSELECT SCOPE_IDENTITY();"; break;
                    case "Страны": sql = $"INSERT INTO Страны (Название) VALUES (N'{v}');\nSELECT SCOPE_IDENTITY();"; break;
                    case "Дистрибьюторы": sql = $"INSERT INTO Дистрибьюторы (Название) VALUES (N'{v}');\nSELECT SCOPE_IDENTITY();"; break;
                    case "Форматы": sql = $"INSERT INTO Форматы (Название) VALUES (N'{v}');\nSELECT SCOPE_IDENTITY();"; break;
                    case "Тип событий": sql = $"INSERT INTO Тип (Название) VALUES (N'{v}');\nSELECT SCOPE_IDENTITY();"; break;
                    case "Личности": sql = $"INSERT INTO Личности (Личность_ИД, Имя, Роль) VALUES (N'{txtDictId.Text}', N'{v}', N'{txtDictRole.Text}');"; break;
                }
                rtbSQL.Text = sql;
            };
            gbDict.Controls.Add(cmbDictType); gbDict.Controls.Add(txtDictName); gbDict.Controls.Add(lblDictId); gbDict.Controls.Add(txtDictId); gbDict.Controls.Add(lblDictRole); gbDict.Controls.Add(txtDictRole); gbDict.Controls.Add(btnGenDict);
            pnlDataScroll.Controls.Add(gbDict);

            GroupBox gbLinks = new GroupBox { Text = "Связи (Привязка тегов из БД)", Top = 200, Left = 5, Width = 345, Height = 150, Font = boldFont };
            cmbLinkType = new ComboBox { Top = 20, Left = 10, Width = 320, DropDownStyle = ComboBoxStyle.DropDownList, Font = defaultFont };
            cmbLinkType.Items.AddRange(new string[] { "Релиз - Жанр", "Релиз - Актер", "Релиз - Режиссер", "Релиз - Страна", "Релиз - Дистрибьютор", "Релиз - Формат", "Событие - Тип", "Событие - Личность" });

            gbLinks.Controls.Add(new Label { Text = "ИД Релиза/События:", Top = 50, Left = 10, AutoSize = true, Font = defaultFont });
            txtLinkMainId = new TextBox { Top = 65, Left = 10, Width = 140, Font = defaultFont };
            gbLinks.Controls.Add(txtLinkMainId);

            gbLinks.Controls.Add(new Label { Text = "Тег из БД:", Top = 50, Left = 160, AutoSize = true, Font = defaultFont });
            cmbLinkTagDb = new ComboBox { Top = 65, Left = 160, Width = 170, DropDownStyle = ComboBoxStyle.DropDownList, Font = defaultFont };
            gbLinks.Controls.Add(cmbLinkTagDb);

            cmbLinkType.SelectedIndexChanged += (s, e) => LoadTagsFromDb(cmbLinkType.SelectedItem.ToString());
            cmbLinkType.SelectedIndex = 0;

            Button btnRefreshDb = new Button { Text = "Обновить список из БД", Top = 100, Left = 10, Width = 160, Height = 35, FlatStyle = FlatStyle.Flat, Font = defaultFont };
            btnRefreshDb.Click += (s, e) => LoadTagsFromDb(cmbLinkType.SelectedItem.ToString());
            gbLinks.Controls.Add(btnRefreshDb);

            Button btnGenLink = new Button { Text = "SQL Связи", Top = 100, Left = 180, Width = 150, Height = 35, BackColor = Color.LightYellow, FlatStyle = FlatStyle.Flat, Font = defaultFont };
            btnGenLink.Click += (s, e) => {
                if (cmbLinkTagDb.SelectedValue == null) { MessageBox.Show("Тег не выбран!"); return; }
                string linkType = cmbLinkType.SelectedItem.ToString();
                string mainId = txtLinkMainId.Text;
                string tagId = cmbLinkTagDb.SelectedValue.ToString();
                string sql = "";
                switch (linkType)
                {
                    case "Релиз - Жанр": sql = $"INSERT INTO Релиз_Жанры (Информация_ИД, Жанр_ИД) VALUES ('{mainId}', {tagId});"; break;
                    case "Релиз - Актер": sql = $"INSERT INTO Релиз_Актеры (Информация_ИД, Актер_ИД) VALUES ('{mainId}', {tagId});"; break;
                    case "Релиз - Режиссер": sql = $"INSERT INTO Релиз_Режиссеры (Информация_ИД, Режиссер_ИД) VALUES ('{mainId}', {tagId});"; break;
                    case "Релиз - Страна": sql = $"INSERT INTO Релиз_Страны (Информация_ИД, Страна_ИД) VALUES ('{mainId}', {tagId});"; break;
                    case "Релиз - Дистрибьютор": sql = $"INSERT INTO Релиз_Дистрибьюторы (Информация_ИД, Дистрибьютор_ИД) VALUES ('{mainId}', {tagId});"; break;
                    case "Релиз - Формат": sql = $"INSERT INTO Релиз_Форматы (Информация_ИД, Формат_ИД) VALUES ('{mainId}', {tagId});"; break;
                    case "Событие - Тип": sql = $"INSERT INTO Событие_Типы (Событие_ИД, Тип_ИД) VALUES ('{mainId}', {tagId});"; break;
                    case "Событие - Личность": sql = $"INSERT INTO Событие_Личность (Событие_ИД, Личность_ИД) VALUES ('{mainId}', N'{tagId}');"; break;
                }
                rtbSQL.Text = sql;
            };
            gbLinks.Controls.Add(btnGenLink);
            pnlDataScroll.Controls.Add(gbLinks);


            // ==========================================
            // НИЖНЯЯ ПАНЕЛЬ - МАСТЕР ГЕНЕРАЦИИ (ПО ПРИМЕРУ "Нарисовал ДВА")
            // ==========================================
            int bottomTop = 500;
            pnlLeft.Controls.Add(new Label { Text = "Базовые данные (для генерации):", Top = bottomTop, Left = 10, AutoSize = true, Font = defaultFont });
            txtEventId = new TextBox { Text = "EV-01", Top = bottomTop + 15, Left = 10, Width = 110, Font = defaultFont };
            txtInfoId = new TextBox { Text = "INFO-01", Top = bottomTop + 15, Left = 125, Width = 110, Font = defaultFont };
            numVenueId = new NumericUpDown { Top = bottomTop + 15, Left = 240, Width = 50, Minimum = 1, Value = 1, Font = defaultFont };
            dtpEventDate = new DateTimePicker { Top = bottomTop + 15, Left = 295, Width = 95, Format = DateTimePickerFormat.Short, Font = defaultFont };
            pnlLeft.Controls.Add(txtEventId); pnlLeft.Controls.Add(txtInfoId); pnlLeft.Controls.Add(numVenueId); pnlLeft.Controls.Add(dtpEventDate);

            GroupBox gbMaster = new GroupBox { Text = "Модули единого SQL скрипта", Top = bottomTop + 45, Left = 10, Width = 380, Height = 95, Font = boldFont };
            chkGenVenue = new CheckBox { Text = "Площадки", Checked = false, Top = 20, Left = 10, AutoSize = true, Font = defaultFont };
            chkGenInfo = new CheckBox { Text = "Информация", Checked = false, Top = 20, Left = 120, AutoSize = true, Font = defaultFont };
            chkGenEvent = new CheckBox { Text = "События", Checked = true, Top = 20, Left = 240, AutoSize = true, Font = defaultFont };
            chkGenLayout = new CheckBox { Text = "Планировка", Checked = true, Top = 45, Left = 10, AutoSize = true, Font = defaultFont };
            chkGenPrices = new CheckBox { Text = "Цены и Привязки", Checked = true, Top = 45, Left = 120, AutoSize = true, Font = defaultFont };
            gbMaster.Controls.Add(chkGenVenue); gbMaster.Controls.Add(chkGenInfo); gbMaster.Controls.Add(chkGenEvent); gbMaster.Controls.Add(chkGenLayout); gbMaster.Controls.Add(chkGenPrices);

            btnGenerateMasterSQL = new Button { Text = "⚡ СГЕНЕРИРОВАТЬ ПОЛНЫЙ СКРИПТ", Top = 65, Left = 10, Width = 360, Height = 25, BackColor = Color.LightGreen, FlatStyle = FlatStyle.Flat, Font = boldFont };
            btnGenerateMasterSQL.Click += BtnGenerateMasterSQL_Click;
            gbMaster.Controls.Add(btnGenerateMasterSQL);
            pnlLeft.Controls.Add(gbMaster);

            // Кнопки Зума и Чертежа
            Button btnZoomOut = new Button { Text = "➖ Зум", Top = bottomTop + 145, Left = 10, Width = 60, Height = 25, Font = defaultFont };
            Button btnZoomIn = new Button { Text = "➕ Зум", Top = bottomTop + 145, Left = 75, Width = 60, Height = 25, Font = defaultFont };
            btnZoomOut.Click += (s, e) => ApplyZoom(-0.2f);
            btnZoomIn.Click += (s, e) => ApplyZoom(0.2f);
            pnlLeft.Controls.Add(btnZoomOut); pnlLeft.Controls.Add(btnZoomIn);

            pnlLeft.Controls.Add(new Label { Text = "Масштаб чертежа:", Top = bottomTop + 150, Left = 145, AutoSize = true, Font = defaultFont });
            numBlueprintScale = new NumericUpDown { Top = bottomTop + 148, Left = 270, Width = 60, Minimum = 0.1M, Maximum = 10.0M, Increment = 0.1M, Value = 1.0M, DecimalPlaces = 1, Font = defaultFont };
            numBlueprintScale.ValueChanged += (s, e) => { _blueprintScale = (float)numBlueprintScale.Value; picCanvas.Invalidate(); };
            pnlLeft.Controls.Add(numBlueprintScale);

            rtbSQL = new RichTextBox { Top = bottomTop + 180, Left = 10, Width = 380, Height = this.ClientSize.Height - bottomTop - 260, Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left, Font = new Font("Courier New", 9) };
            pnlLeft.Controls.Add(rtbSQL);

            Button btnLoadImage = new Button { Text = "📂 Загрузить чертеж", Top = this.ClientSize.Height - 75, Left = 10, Width = 380, Height = 35, BackColor = Color.Gainsboro, FlatStyle = FlatStyle.Flat, Anchor = AnchorStyles.Bottom | AnchorStyles.Left, Font = defaultFont };
            btnLoadImage.Click += BtnLoadImage_Click;
            pnlLeft.Controls.Add(btnLoadImage);

            typeof(Panel).InvokeMember("DoubleBuffered", System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, pnlMap, new object[] { true });
        }

        private void rbText_CheckedChanged(object sender, EventArgs e)
        {
            if (rbText.Checked)
            {
                cmbTextOrientation.SelectedIndex = 0;
            }
        }
        private void cmbTextOrientation_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (rbText.Checked && cmbTextOrientation.SelectedIndex != 0)
            {
                cmbTextOrientation.SelectedIndex = 0; // это необходимо поскольку при построении текста только L-T работает корректно
            }
        }
        // ==========================================
        // ЛОГИКА МАССОВОГО НАЗНАЧЕНИЯ И ОТМЕНЫ
        // ==========================================
        private void BtnAssignRange_Click(object sender, EventArgs e)
        {
            if (_activeCategoryId == -1) { MessageBox.Show("Сначала создайте и выберите категорию цены!"); return; }
            int rF = (int)numRowFrom.Value; int rT = (int)numRowTo.Value;
            int sF = (int)numSeatFrom.Value; int sT = (int)numSeatTo.Value;

            _undoEventStack.Push(() => { }); // Пустышка-разделитель для стека (можно усложнить, но для примера хватит)
            int assignedCount = 0;

            foreach (var seat in _seats)
            {
                if (int.TryParse(seat.Row, out int r) && int.TryParse(seat.Num, out int s))
                {
                    if (r >= rF && r <= rT && s >= sF && s <= sT)
                    {
                        int? prevCat = seat.CategoryId;
                        _undoEventStack.Push(() => seat.CategoryId = prevCat);
                        seat.CategoryId = _activeCategoryId;
                        assignedCount++;
                    }
                }
            }
            picCanvas.Invalidate();
            MessageBox.Show($"Назначено мест: {assignedCount}", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnAddCategory_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCatName.Text)) return;
            int newId = _categories.Count + 1;
            var cat = new EventCategory { Id = newId, Name = txtCatName.Text, Price = numCatPrice.Value, Color = _currentPickedColor, Description = txtCatDesc.Text };
            _categories.Add(cat);
            RenderCategories();
        }

        private void BtnRemoveCategory_Click(object sender, EventArgs e)
        {
            if (_categories.Count > 0)
            {
                var catToRemove = _categories.Last();
                _categories.Remove(catToRemove);

                // Очищаем места от этой категории
                foreach (var s in _seats.Where(x => x.CategoryId == catToRemove.Id)) s.CategoryId = null;
                foreach (var z in _zones.Where(x => x.CategoryId == catToRemove.Id)) z.CategoryId = null;

                RenderCategories();
                picCanvas.Invalidate();
            }
        }

        private void RenderCategories()
        {
            pnlCategories.Controls.Clear();
            foreach (var cat in _categories)
            {
                RadioButton rb = new RadioButton { Text = $"{cat.Name} ({cat.Price}₽)", AutoSize = true, ForeColor = cat.Color, Font = new Font("Arial", 8, FontStyle.Bold) };
                rb.CheckedChanged += (s, ev) => { if (rb.Checked) _activeCategoryId = cat.Id; };
                pnlCategories.Controls.Add(rb);
            }
            if (_categories.Count > 0) { ((RadioButton)pnlCategories.Controls[_categories.Count - 1]).Checked = true; }
            else { _activeCategoryId = -1; }
        }

        // ==========================================
        // БАЗА ДАННЫХ И ПОДГРУЗКА ТЕГОВ
        // ==========================================
        private void LoadTagsFromDb(string linkType)
        {
            cmbLinkTagDb.DataSource = null;
            List<DbTagItem> tags = new List<DbTagItem>();
            string query = "";

            switch (linkType)
            {
                case "Релиз - Жанр": query = "SELECT Жанр_ИД AS Id, Название AS Name FROM Жанры"; break;
                case "Релиз - Актер": query = "SELECT Актер_ИД AS Id, Полное_Имя AS Name FROM Актеры"; break;
                case "Релиз - Режиссер": query = "SELECT Режиссер_ИД AS Id, Полное_Имя AS Name FROM Режиссеры"; break;
                case "Релиз - Страна": query = "SELECT Страна_ИД AS Id, Название AS Name FROM Страны"; break;
                case "Релиз - Дистрибьютор": query = "SELECT Дистрибьютор_ИД AS Id, Название AS Name FROM Дистрибьюторы"; break;
                case "Релиз - Формат": query = "SELECT Формат_ИД AS Id, Название AS Name FROM Форматы"; break;
                case "Событие - Тип": query = "SELECT Тип_ИД AS Id, Название AS Name FROM Тип"; break;
                case "Событие - Личность": query = "SELECT Личность_ИД AS Id, Имя + ' (' + Роль + ')' AS Name FROM Личности"; break;
            }

            if (!string.IsNullOrEmpty(query))
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(DatabaseHelper.connectionString))
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            while (r.Read()) tags.Add(new DbTagItem { Id = r["Id"].ToString(), Display = $"[{r["Id"]}] {r["Name"]}" });
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            if (tags.Count > 0)
            {
                cmbLinkTagDb.DisplayMember = "Display";
                cmbLinkTagDb.ValueMember = "Id";
                cmbLinkTagDb.DataSource = tags;
            }
            else
            {
                cmbLinkTagDb.Items.Clear();
                cmbLinkTagDb.Items.Add("Нет данных в БД");
                cmbLinkTagDb.SelectedIndex = 0;
            }
        }


        // ==========================================
        // МАСТЕР ЕДИНОГО СКРИПТА
        // ==========================================
        private void BtnGenerateMasterSQL_Click(object sender, EventArgs e)
        {
            GetCanvasOffsets(out int shiftX, out int shiftY);
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("USE База_Мероприятий;");
            sb.AppendLine("GO");
            sb.AppendLine("SET NOCOUNT ON;");
            sb.AppendLine();
            sb.AppendLine("-- ОЧИСТКА СТАРЫХ ДАННЫХ ПЛАНИРОВКИ --");
            sb.AppendLine("-- ЗАКОММЕНТИРУЙТЕ ВО ИЗБЕЖАНИЕ СЛУЧАЙНЫХ УДАЛЕНИЙ");
            sb.AppendLine("ALTER TABLE Событие_Места_Категории NOCHECK CONSTRAINT ALL;");
            sb.AppendLine("ALTER TABLE Событие_Лимиты_Зон NOCHECK CONSTRAINT ALL;");
            sb.AppendLine("ALTER TABLE Планировки_Мест NOCHECK CONSTRAINT ALL;");
            sb.AppendLine("ALTER TABLE Планировки_Зон NOCHECK CONSTRAINT ALL;");
            sb.AppendLine("ALTER TABLE События NOCHECK CONSTRAINT ALL;");
            sb.AppendLine("ALTER TABLE Событие_Категории NOCHECK CONSTRAINT ALL;");
            sb.AppendLine("ALTER TABLE Ценовая_Политика NOCHECK CONSTRAINT ALL;");
            sb.AppendLine("ALTER TABLE Билеты NOCHECK CONSTRAINT ALL;");
            sb.AppendLine("DELETE FROM Событие_Места_Категории WHERE Планировка_ИД = '" + txtPlanId.Text + "';");
            sb.AppendLine("DELETE FROM Событие_Лимиты_Зон WHERE Планировка_ИД = '" + txtPlanId.Text + "';");
            sb.AppendLine("DELETE FROM Планировки_Мест WHERE Планировка_ИД = '" + txtPlanId.Text + "';");
            sb.AppendLine("DELETE FROM Планировки_Зон WHERE Планировка_ИД = '" + txtPlanId.Text + "';");
            sb.AppendLine();

            sb.AppendLine("BEGIN TRANSACTION;");
            sb.AppendLine("BEGIN TRY;");
            sb.AppendLine();

            sb.AppendLine($"    DECLARE @PlanID NVARCHAR(50) = '{txtPlanId.Text}';");
            sb.AppendLine($"    DECLARE @CurrentEvID NVARCHAR(50) = '{txtEventId.Text}';");
            sb.AppendLine($"    DECLARE @CurrentInfoID NVARCHAR(50) = '{txtInfoId.Text}';");
            sb.AppendLine($"    DECLARE @ПлощадкаИД INT = {numVenueId.Value};");
            sb.AppendLine($"    DECLARE @EventTime DATETIME = '{dtpEventDate.Value.ToString("yyyy-MM-dd HH:mm:00")}';");
            sb.AppendLine();

            if (chkGenVenue.Checked)
            {
                sb.AppendLine("    -- ПЛОЩАДКА --");
                sb.AppendLine("    IF NOT EXISTS (SELECT 1 FROM Площадки WHERE Площадка_ИД = @ПлощадкаИД)");
                sb.AppendLine($"    INSERT INTO Площадки (Название, Адрес, Планировка_ИД) VALUES (N'Новая Арена', N'Адрес', @PlanID);");
            }
            if (chkGenInfo.Checked)
            {
                sb.AppendLine("    -- ИНФОРМАЦИЯ --");
                sb.AppendLine("    IF NOT EXISTS (SELECT 1 FROM Информация WHERE Информация_ИД = @CurrentInfoID)");
                sb.AppendLine($"    INSERT INTO Информация (Информация_ИД, Название) VALUES (@CurrentInfoID, N'Новое Событие');");
            }
            if (chkGenEvent.Checked)
            {
                sb.AppendLine("    -- СОБЫТИЕ --");
                sb.AppendLine("    IF NOT EXISTS (SELECT 1 FROM События WHERE Событие_ИД = @CurrentEvID)");
                sb.AppendLine($"    INSERT INTO События (Событие_ИД, Информация_ИД, Дата_Время, Площадка_ИД, Номер_Зала, Планировка_ИД) VALUES (@CurrentEvID, @CurrentInfoID, @EventTime, @ПлощадкаИД, 0, @PlanID);");
            }

            if (chkGenLayout.Checked)
            {
                sb.AppendLine("\n    -- ГЕОМЕТРИЯ ЗОН И ТЕКСТА --");
                if (_zones.Count > 0)
                {
                    sb.AppendLine("    INSERT INTO Планировки_Зон (Планировка_ИД, Зона_ИД, Название_Зоны, Тип_Зоны, Коорд_X, Коорд_Y, Координаты_Полигона, Ориентация_Текста, Размер_Текста) VALUES");
                    for (int i = 0; i < _zones.Count; i++)
                    {
                        var z = _zones[i];
                        int cx = z.Type == "Текст" ? z.Center.X + shiftX : (int)(z.Points.Sum(p => p.X + shiftX) / Math.Max(1, z.Points.Count));
                        int cy = z.Type == "Текст" ? z.Center.Y + shiftY : (int)(z.Points.Sum(p => p.Y + shiftY) / Math.Max(1, z.Points.Count));
                        string polyString = z.Type == "Текст" ? "" : "M " + string.Join(" L ", z.Points.Select(p => $"{p.X + shiftX} {p.Y + shiftY}")) + " Z";
                        sb.Append($"    (@PlanID, '{z.ZoneId}', '{z.Name}', '{z.Type}', {cx}, {cy}, '{polyString}', '{z.Orientation}', {z.FontSize})");
                        sb.AppendLine(i == _zones.Count - 1 ? ";" : ",");
                    }
                }

                sb.AppendLine("\n    -- ГЕОМЕТРИЯ МЕСТ --");
                if (_seats.Count > 0)
                {
                    sb.AppendLine("    INSERT INTO Планировки_Мест (Планировка_ИД, Зона_ИД, Место_ИД, Ряд, Номер, Коорд_X, Коорд_Y) VALUES");
                    for (int i = 0; i < _seats.Count; i++)
                    {
                        var s = _seats[i];
                        if (i > 0 && i % 900 == 0) { sb.Length -= 3; sb.AppendLine(";\n    INSERT INTO Планировки_Мест (Планировка_ИД, Зона_ИД, Место_ИД, Ряд, Номер, Коорд_X, Коорд_Y) VALUES"); }
                        sb.Append($"    (@PlanID, '{s.ZoneId}', '{s.ZoneId}-R{s.Row}-S{s.Num}', '{s.Row}', '{s.Num}', {s.Location.X + shiftX}, {s.Location.Y + shiftY})");
                        sb.AppendLine((i == _seats.Count - 1 || (i + 1) % 900 == 0) ? ";" : ",");
                    }
                }
            }

            if (chkGenPrices.Checked && _categories.Count > 0)
            {
                sb.AppendLine("\n    -- КАТЕГОРИИ И ЦЕНЫ --");
                sb.AppendLine("    INSERT INTO Событие_Категории (Событие_ИД, Категория_ИД, Название, Цвет, Описание) VALUES");
                for (int i = 0; i < _categories.Count; i++)
                {
                    var c = _categories[i];
                    string hex = "#" + c.Color.R.ToString("X2") + c.Color.G.ToString("X2") + c.Color.B.ToString("X2");
                    sb.Append($"    (@CurrentEvID, {c.Id}, N'{c.Name}', '{hex}', N'{c.Description}')");
                    sb.AppendLine(i == _categories.Count - 1 ? ";" : ",");
                }

                sb.AppendLine("    INSERT INTO Ценовая_Политика (Событие_ИД, Категория_ИД, Цена, Актуальность) VALUES");
                for (int i = 0; i < _categories.Count; i++)
                {
                    sb.Append($"    (@CurrentEvID, {_categories[i].Id}, {_categories[i].Price.ToString(System.Globalization.CultureInfo.InvariantCulture)}, @EventTime)");
                    sb.AppendLine(i == _categories.Count - 1 ? ";" : ",");
                }

                var zonedLimits = _zones.Where(x => x.CategoryId.HasValue).ToList();
                if (zonedLimits.Count > 0)
                {
                    sb.AppendLine("\n    -- ПРИВЯЗКА ЛИМИТОВ ЗОН --");
                    sb.AppendLine("    INSERT INTO Событие_Лимиты_Зон (Событие_ИД, Планировка_ИД, Зона_ИД, Макс_Мест) VALUES");
                    for (int i = 0; i < zonedLimits.Count; i++)
                    {
                        sb.Append($"    (@CurrentEvID, @PlanID, '{zonedLimits[i].ZoneId}', {zonedLimits[i].CapacityLimit})");
                        sb.AppendLine(i == zonedLimits.Count - 1 ? ";" : ",");
                    }
                }

                var seatedCats = _seats.Where(x => x.CategoryId.HasValue).ToList();
                if (seatedCats.Count > 0)
                {
                    sb.AppendLine("\n    -- ПРИВЯЗКА МЕСТ К КАТЕГОРИЯМ --");
                    sb.AppendLine("    INSERT INTO Событие_Места_Категории (Событие_ИД, Планировка_ИД, Место_ИД, Категория_ИД) VALUES");
                    for (int i = 0; i < seatedCats.Count; i++)
                    {
                        var s = seatedCats[i];
                        if (i > 0 && i % 900 == 0) { sb.Length -= 3; sb.AppendLine(";\n    INSERT INTO Событие_Места_Категории (Событие_ИД, Планировка_ИД, Место_ИД, Категория_ИД) VALUES"); }
                        sb.Append($"    (@CurrentEvID, @PlanID, '{s.ZoneId}-R{s.Row}-S{s.Num}', {s.CategoryId.Value})");
                        sb.AppendLine((i == seatedCats.Count - 1 || (i + 1) % 900 == 0) ? ";" : ",");
                    }
                }
            }

            sb.AppendLine("\nCOMMIT TRANSACTION;");
            sb.AppendLine("END TRY");
            sb.AppendLine("BEGIN CATCH");
            sb.AppendLine("    ROLLBACK TRANSACTION;");
            sb.AppendLine("    PRINT 'Ошибка: ' + ERROR_MESSAGE();");
            sb.AppendLine("END CATCH");
            sb.AppendLine();
            sb.AppendLine("ALTER TABLE Событие_Места_Категории CHECK CONSTRAINT ALL;"); 
            sb.AppendLine("ALTER TABLE Событие_Лимиты_Зон CHECK CONSTRAINT ALL;");
            sb.AppendLine("ALTER TABLE Планировки_Мест CHECK CONSTRAINT ALL;");
            sb.AppendLine("ALTER TABLE Планировки_Зон  CHECK CONSTRAINT ALL;");
            sb.AppendLine("ALTER TABLE События  CHECK CONSTRAINT ALL;");
            sb.AppendLine("ALTER TABLE Событие_Категории CHECK CONSTRAINT ALL;");
            sb.AppendLine("ALTER TABLE Ценовая_Политика CHECK CONSTRAINT ALL;");
            sb.AppendLine("ALTER TABLE Билеты  CHECK CONSTRAINT ALL;");

            rtbSQL.Text = sb.ToString();
        }

        // ==========================================
        // БАЗОВЫЕ ФУНКЦИИ КАНВАСА (МЫШЬ, ОТРИСОВКА)
        // ==========================================
        private void numMaxStep_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e is HandledMouseEventArgs handledArgs)
            {
                handledArgs.Handled = true;
                if (e.Delta > 0 && numMaxStep.Value + numMaxStep.Increment <= numMaxStep.Maximum) numMaxStep.Value += numMaxStep.Increment;
                else if (e.Delta < 0 && numMaxStep.Value - numMaxStep.Increment >= numMaxStep.Minimum) numMaxStep.Value -= numMaxStep.Increment;
            }
        }

        private void ApplyZoom(float delta)
        {
            if (_originalImage == null) return;
            _scale = Math.Max(0.5f, Math.Min(4.0f, _scale + delta));
            picCanvas.Size = new Size((int)(_originalImage.Width * _scale), (int)(_originalImage.Height * _scale));
            picCanvas.Invalidate();
        }

        private void BtnLoadImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog { Filter = "Images|*.jpg;*.jpeg;*.png" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    _originalImage = Image.FromFile(ofd.FileName);
                    _scale = 1.0f;
                    picCanvas.Size = _originalImage.Size;
                    _zones.Clear(); _seats.Clear(); _currentPolygon.Clear(); rtbSQL.Clear(); _undoEventStack.Clear();
                }
            }
        }

        private void PicCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_originalImage == null) return;
            double step = (double)numMaxStep.Value;
            if (step < 1) step = 1;
            int snapX = (int)Math.Round((int)Math.Round((double)e.X / _scale / step) * step);
            int snapY = (int)Math.Round((int)Math.Round((double)e.Y / _scale / step) * step);

            if (tabMode.SelectedTab.Text.Contains("Планировка") && chkOrtho.Checked && !rbText.Checked)
            {
                Point last = rbZones.Checked && _currentPolygon.Count > 0 ? _currentPolygon.Last() : rbSeats.Checked && _seats.Count > 0 ? _seats.Last().Location : Point.Empty;
                if (!last.IsEmpty) { if (Math.Abs(snapX - last.X) > Math.Abs(snapY - last.Y)) snapY = last.Y; else snapX = last.X; }
            }
            _mousePosReal = new Point(snapX, snapY);
            picCanvas.Invalidate();
        }

        private void PicCanvas_MouseClick(object sender, MouseEventArgs e)
        {
            if (_originalImage == null || e.Button != MouseButtons.Left) return;

            if (tabMode.SelectedTab.Text.Contains("Планировка"))
            {
                if (chkAutoStep.Checked) // РАБОТАЕТ С ДЕФЕКТОМ, я пробовал использовать (значение - SEAT_SIZE) и тут и там, не помогло, но с ними ситуация получше, чем без
                {
                    Point rawPos = new Point((int)(e.X / _scale), (int)(e.Y / _scale)); // Чистые координаты

                    if (_measurePoint == null)
                    {
                        _measurePoint = rawPos; // Клик 1: запоминаем стартовую точку
                    }
                    else
                    {
                        // Клик 2: вычисляем расстояние
                        double dist = Math.Sqrt(Math.Pow(rawPos.X - _measurePoint.Value.X, 2) + Math.Pow(rawPos.Y - _measurePoint.Value.Y, 2));

                        if (dist >= (double)numMaxStep.Minimum && dist <= (double)numMaxStep.Maximum)
                            numMaxStep.Value = (decimal)Math.Round(dist, 2);

                        _measurePoint = null; // Сбрасываем линейку
                        chkAutoStep.Checked = false; // Отключаем галочку
                    }
                    picCanvas.Invalidate();
                    return; // Прерываем выполнение метода, чтобы не поставить место на чертеж
                }
                if (rbZones.Checked) _currentPolygon.Add(_mousePosReal);
                else if (rbSeats.Checked)
                {
                    _seats.Add(new PlacedSeat { PlanId = txtPlanId.Text, ZoneId = txtZoneId.Text, Row = txtSeatRow.Text, Num = txtSeatNum.Text, Location = _mousePosReal });
                    if (int.TryParse(txtSeatNum.Text, out int n)) txtSeatNum.Text = (n + 1).ToString();
                }
                else if (rbText.Checked) _zones.Add(new PlacedZone { PlanId = txtPlanId.Text, ZoneId = "TXT-" + Guid.NewGuid().ToString().Substring(0, 6), Name = txtZoneName.Text, Type = "Текст", FontSize = (int)numFontSize.Value, Orientation = cmbTextOrientation.SelectedItem.ToString(), Center = _mousePosReal });
            }
            else if (tabMode.SelectedTab.Text.Contains("Цены"))
            {
                if (_activeCategoryId == -1) { MessageBox.Show("Сначала создайте и выберите категорию цены!"); return; }

                double selectionRadius = 25.0;
                bool found = false;

                foreach (var s in _seats)
                {
                    double dist = Math.Sqrt(Math.Pow(s.Location.X - _mousePosReal.X, 2) + Math.Pow(s.Location.Y - _mousePosReal.Y, 2));
                    if (dist <= selectionRadius)
                    {
                        int? prevCat = s.CategoryId;
                        _undoEventStack.Push(() => s.CategoryId = prevCat);
                        s.CategoryId = _activeCategoryId;
                        found = true;
                    }
                }

                if (!found)
                {
                    foreach (var z in _zones.Where(x => x.Type == "Зона"))
                    {
                        using (GraphicsPath path = new GraphicsPath())
                        {
                            path.AddPolygon(z.Points.ToArray());
                            double dist = Math.Sqrt(Math.Pow(z.Center.X - _mousePosReal.X, 2) + Math.Pow(z.Center.Y - _mousePosReal.Y, 2));
                            if (path.IsVisible(_mousePosReal) || dist <= selectionRadius)
                            {
                                int? prevCat = z.CategoryId;
                                int prevLimit = z.CapacityLimit;
                                _undoEventStack.Push(() => { z.CategoryId = prevCat; z.CapacityLimit = prevLimit; });
                                z.CategoryId = _activeCategoryId;
                                z.CapacityLimit = (int)numZoneCapacity.Value;
                            }
                        }
                    }
                }
            }
            picCanvas.Invalidate();
        }

        private void BtnUndo_Click(object sender, EventArgs e)
        {
            if (tabMode.SelectedTab.Text.Contains("Планировка"))
            {
                if (rbZones.Checked) { if (_currentPolygon.Count > 0) _currentPolygon.RemoveAt(_currentPolygon.Count - 1); else if (_zones.Count > 0) _zones.RemoveAt(_zones.Count - 1); }
                else if (rbSeats.Checked) { if (_seats.Count > 0) { _seats.RemoveAt(_seats.Count - 1); if (int.TryParse(txtSeatNum.Text, out int n) && n > 1) txtSeatNum.Text = (n - 1).ToString(); } }
                else if (rbText.Checked) { var lastText = _zones.LastOrDefault(z => z.Type == "Текст"); if (lastText != null) _zones.Remove(lastText); }
            }
            else { if (_undoEventStack.Count > 0) _undoEventStack.Pop().Invoke(); }
            picCanvas.Invalidate();
        }

        private void BtnFinishZone_Click(object sender, EventArgs e)
        {
            if (_currentPolygon.Count < 3) return;
            if (rbZones.Checked) _zones.Add(new PlacedZone { PlanId = txtPlanId.Text, ZoneId = txtZoneId.Text, Name = txtZoneName.Text, Type = "Зона", FontSize = (int)numFontSize.Value, Orientation = cmbTextOrientation.SelectedItem.ToString(), Points = new List<Point>(_currentPolygon) });
            else if (rbDanse.Checked) _zones.Add(new PlacedZone { PlanId = txtPlanId.Text, ZoneId = txtZoneId.Text, Name = txtZoneName.Text, Type = "Танцопол", FontSize = (int)numFontSize.Value, Orientation = cmbTextOrientation.SelectedItem.ToString(), Points = new List<Point>(_currentPolygon) });
            _currentPolygon.Clear();
            picCanvas.Invalidate();
        }

        private void GetCanvasOffsets(out int shiftX, out int shiftY)
        {
            int minX = int.MaxValue, minY = int.MaxValue;
            foreach (var s in _seats) { minX = Math.Min(minX, s.Location.X); minY = Math.Min(minY, s.Location.Y); }
            foreach (var z in _zones)
            {
                if (z.Type == "Текст") { minX = Math.Min(minX, z.Center.X); minY = Math.Min(minY, z.Center.Y); }
                foreach (var p in z.Points) { minX = Math.Min(minX, p.X); minY = Math.Min(minY, p.Y); }
            }
            if (minX == int.MaxValue) { minX = 0; minY = 0; }
            shiftX = 100 - minX;
            shiftY = 100 - minY;
        }

        private void PicCanvas_Paint(object sender, PaintEventArgs e)
        {
            if (_originalImage != null) e.Graphics.DrawImage(_originalImage, 0, 0, _originalImage.Width * _blueprintScale, _originalImage.Height * _blueprintScale);
            else return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            // Отрисовка красной линии "Линейки" при автовычислении шага
            if (chkAutoStep.Checked && _measurePoint != null && tabMode.SelectedTab.Text.Contains("Планировка"))
            {
                Point localMouse = picCanvas.PointToClient(Cursor.Position);
                PointF start = new PointF(_measurePoint.Value.X * _scale, _measurePoint.Value.Y * _scale);
                e.Graphics.FillEllipse(Brushes.Red, start.X - 5, start.Y - 5, 10, 10);
                e.Graphics.DrawLine(new Pen(Color.Red, 2) { DashStyle = DashStyle.Dash }, start, localMouse);
            }
            foreach (var z in _zones)
            {
                if (z.Type == "Зона")
                {
                    PointF[] pts = z.Points.Select(p => new PointF(p.X * _scale, p.Y * _scale)).ToArray();
                    Color fillColor = Color.FromArgb(80, Color.LightGreen);
                    if (z.CategoryId.HasValue)
                    {
                        var cat = _categories.FirstOrDefault(c => c.Id == z.CategoryId.Value);
                        if (cat != null) fillColor = Color.FromArgb(100, cat.Color);
                    }
                    e.Graphics.FillPolygon(new SolidBrush(fillColor), pts);
                    e.Graphics.DrawPolygon(Pens.Green, pts);
                }
                else if (z.Type == "Текст")
                {
                    using (Font f = new Font("Arial", z.FontSize * _scale, FontStyle.Bold))
                    {
                        StringFormat fmt = new StringFormat();
                        if (z.Orientation.EndsWith("T")) fmt.LineAlignment = StringAlignment.Near; else if (z.Orientation.EndsWith("B")) fmt.LineAlignment = StringAlignment.Far; else fmt.LineAlignment = StringAlignment.Center;
                        if (z.Orientation.StartsWith("L")) fmt.Alignment = StringAlignment.Near; else if (z.Orientation.StartsWith("R")) fmt.Alignment = StringAlignment.Far; else fmt.Alignment = StringAlignment.Center;
                        e.Graphics.DrawString(z.Name, f, Brushes.Black, z.Center.X * _scale, z.Center.Y * _scale, fmt);
                    }
                }
            }

            if (_currentPolygon.Count > 0)
            {
                var scaledPts = _currentPolygon.Select(p => new PointF(p.X * _scale, p.Y * _scale)).ToArray();
                if (scaledPts.Length > 1) e.Graphics.DrawLines(new Pen(Color.Red, 2), scaledPts);
                foreach (var pt in scaledPts) e.Graphics.FillEllipse(Brushes.Yellow, pt.X - 4, pt.Y - 4, 8, 8);
                if (rbZones.Checked && tabMode.SelectedTab.Text.Contains("Планировка")) e.Graphics.DrawLine(new Pen(Color.Orange, 1) { DashStyle = DashStyle.Dash }, scaledPts.Last(), new PointF(_mousePosReal.X * _scale, _mousePosReal.Y * _scale));
            }

            float scaledSize = SEAT_SIZE * _scale;
            using (Font seatFont = new Font("Arial", Math.Max(7 * _scale, 1f), FontStyle.Regular))
            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                foreach (var s in _seats)
                {
                    float sx = s.Location.X * _scale;
                    float sy = s.Location.Y * _scale;
                    Color seatColor = Color.Cyan;
                    if (s.CategoryId.HasValue)
                    {
                        var cat = _categories.FirstOrDefault(c => c.Id == s.CategoryId.Value);
                        if (cat != null) seatColor = cat.Color;
                    }
                    e.Graphics.FillEllipse(new SolidBrush(seatColor), sx, sy, scaledSize, scaledSize);
                    e.Graphics.DrawEllipse(Pens.Black, sx, sy, scaledSize, scaledSize);

                    // ОТОБРАЖАЕМ РЯД И МЕСТО
                    RectangleF textRect = new RectangleF(sx, sy, scaledSize, scaledSize);
                    e.Graphics.DrawString($"{s.Row}-{s.Num}", seatFont, Brushes.Black, textRect, sf);
                }
            }

            if (tabMode.SelectedTab.Text.Contains("Планировка"))
            {
                if (rbSeats.Checked) e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(128, Color.Black)), _mousePosReal.X * _scale, _mousePosReal.Y * _scale, scaledSize, scaledSize);
                else if (rbText.Checked) { using (Font f = new Font("Arial", (float)numFontSize.Value * _scale, FontStyle.Bold)) e.Graphics.DrawString(txtZoneName.Text, f, Brushes.Gray, _mousePosReal.X * _scale, _mousePosReal.Y * _scale); }
            }
            else
            {
                if (_activeCategoryId != -1)
                {
                    var cat = _categories.FirstOrDefault(c => c.Id == _activeCategoryId);
                    if (cat != null)
                    {
                        float diameter = 50f;
                        float radius = 25f;
                        e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(150, cat.Color)), (_mousePosReal.X - radius) * _scale, (_mousePosReal.Y - radius) * _scale, diameter * _scale, diameter * _scale);
                    }
                }
            }
        }
    }
}