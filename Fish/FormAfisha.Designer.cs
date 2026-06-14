namespace Fish
{
    partial class FormAfisha
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pnlFilters = new System.Windows.Forms.Panel();
            this.BtnMyTickets = new System.Windows.Forms.Button();
            this.BtnLogin = new System.Windows.Forms.Button();
            this.cmbSort = new System.Windows.Forms.ComboBox();
            this.cmbPersonality = new System.Windows.Forms.ComboBox();
            this.cmbVenue = new System.Windows.Forms.ComboBox();
            this.cmbTag = new System.Windows.Forms.ComboBox();
            this.cmbCity = new System.Windows.Forms.ComboBox();
            this.dtpDate = new System.Windows.Forms.DateTimePicker();
            this.flpEvents = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlRightDetails = new System.Windows.Forms.Panel();
            this.lblRightTitle = new System.Windows.Forms.Label();
            this.pnlFilters.SuspendLayout();
            this.pnlRightDetails.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlFilters
            // 
            this.pnlFilters.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlFilters.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlFilters.Controls.Add(this.BtnMyTickets);
            this.pnlFilters.Controls.Add(this.BtnLogin);
            this.pnlFilters.Controls.Add(this.cmbSort);
            this.pnlFilters.Controls.Add(this.cmbPersonality);
            this.pnlFilters.Controls.Add(this.cmbVenue);
            this.pnlFilters.Controls.Add(this.cmbTag);
            this.pnlFilters.Controls.Add(this.cmbCity);
            this.pnlFilters.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlFilters.Location = new System.Drawing.Point(0, 0);
            this.pnlFilters.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pnlFilters.Name = "pnlFilters";
            this.pnlFilters.Size = new System.Drawing.Size(1184, 99);
            this.pnlFilters.TabIndex = 0;
            // 
            // BtnMyTickets
            // 
            this.BtnMyTickets.Font = new System.Drawing.Font("Arial", 14F);
            this.BtnMyTickets.Location = new System.Drawing.Point(633, 16);
            this.BtnMyTickets.Name = "BtnMyTickets";
            this.BtnMyTickets.Size = new System.Drawing.Size(138, 29);
            this.BtnMyTickets.TabIndex = 5;
            this.BtnMyTickets.Text = "Мои билеты";
            this.BtnMyTickets.UseVisualStyleBackColor = true;
            this.BtnMyTickets.Visible = false;
            this.BtnMyTickets.Click += new System.EventHandler(this.BtnMyTickets_Click);
            // 
            // BtnLogin
            // 
            this.BtnLogin.Font = new System.Drawing.Font("Arial", 14F);
            this.BtnLogin.Location = new System.Drawing.Point(633, 14);
            this.BtnLogin.Name = "BtnLogin";
            this.BtnLogin.Size = new System.Drawing.Size(138, 29);
            this.BtnLogin.TabIndex = 5;
            this.BtnLogin.Text = "Логин/Регист.";
            this.BtnLogin.UseVisualStyleBackColor = true;
            this.BtnLogin.Visible = false;
            this.BtnLogin.Click += new System.EventHandler(this.BtnLogin_Click);
            // 
            // cmbSort
            // 
            this.cmbSort.Font = new System.Drawing.Font("Arial", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(254)));
            this.cmbSort.FormattingEnabled = true;
            this.cmbSort.Items.AddRange(new object[] {
            "Название (А-Я)",
            "Название (Я-А)",
            "Сначала поздние",
            "Сначала ранние "});
            this.cmbSort.Location = new System.Drawing.Point(427, 49);
            this.cmbSort.Name = "cmbSort";
            this.cmbSort.Size = new System.Drawing.Size(200, 30);
            this.cmbSort.TabIndex = 4;
            this.cmbSort.SelectedIndexChanged += new System.EventHandler(this.dox);
            // 
            // cmbPersonality
            // 
            this.cmbPersonality.Font = new System.Drawing.Font("Arial", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(254)));
            this.cmbPersonality.FormattingEnabled = true;
            this.cmbPersonality.Location = new System.Drawing.Point(221, 49);
            this.cmbPersonality.Name = "cmbPersonality";
            this.cmbPersonality.Size = new System.Drawing.Size(200, 30);
            this.cmbPersonality.TabIndex = 3;
            this.cmbPersonality.SelectedIndexChanged += new System.EventHandler(this.dox);
            // 
            // cmbVenue
            // 
            this.cmbVenue.Font = new System.Drawing.Font("Arial", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(254)));
            this.cmbVenue.FormattingEnabled = true;
            this.cmbVenue.Location = new System.Drawing.Point(14, 49);
            this.cmbVenue.Name = "cmbVenue";
            this.cmbVenue.Size = new System.Drawing.Size(200, 30);
            this.cmbVenue.TabIndex = 2;
            this.cmbVenue.SelectedIndexChanged += new System.EventHandler(this.dox);
            // 
            // cmbTag
            // 
            this.cmbTag.Font = new System.Drawing.Font("Arial", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(254)));
            this.cmbTag.FormattingEnabled = true;
            this.cmbTag.Location = new System.Drawing.Point(427, 14);
            this.cmbTag.Name = "cmbTag";
            this.cmbTag.Size = new System.Drawing.Size(200, 30);
            this.cmbTag.TabIndex = 1;
            this.cmbTag.SelectedIndexChanged += new System.EventHandler(this.dox);
            // 
            // cmbCity
            // 
            this.cmbCity.Font = new System.Drawing.Font("Arial", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(254)));
            this.cmbCity.FormattingEnabled = true;
            this.cmbCity.Location = new System.Drawing.Point(221, 14);
            this.cmbCity.Name = "cmbCity";
            this.cmbCity.Size = new System.Drawing.Size(200, 30);
            this.cmbCity.TabIndex = 0;
            this.cmbCity.SelectedIndexChanged += new System.EventHandler(this.dox);
            // 
            // dtpDate
            // 
            this.dtpDate.Font = new System.Drawing.Font("Arial", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(254)));
            this.dtpDate.Location = new System.Drawing.Point(15, 15);
            this.dtpDate.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.dtpDate.Name = "dtpDate";
            this.dtpDate.Size = new System.Drawing.Size(200, 29);
            this.dtpDate.TabIndex = 1;
            // 
            // flpEvents
            // 
            this.flpEvents.AutoScroll = true;
            this.flpEvents.BackColor = System.Drawing.SystemColors.ControlLight;
            this.flpEvents.Location = new System.Drawing.Point(0, 105);
            this.flpEvents.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.flpEvents.Name = "flpEvents";
            this.flpEvents.Padding = new System.Windows.Forms.Padding(10);
            this.flpEvents.Size = new System.Drawing.Size(825, 460);
            this.flpEvents.TabIndex = 2;
            // 
            // pnlRightDetails
            // 
            this.pnlRightDetails.BackColor = System.Drawing.Color.White;
            this.pnlRightDetails.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlRightDetails.Controls.Add(this.lblRightTitle);
            this.pnlRightDetails.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlRightDetails.Location = new System.Drawing.Point(834, 99);
            this.pnlRightDetails.Name = "pnlRightDetails";
            this.pnlRightDetails.Size = new System.Drawing.Size(350, 466);
            this.pnlRightDetails.TabIndex = 0;
            this.pnlRightDetails.Visible = false;
            // 
            // lblRightTitle
            // 
            this.lblRightTitle.AutoSize = true;
            this.lblRightTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblRightTitle.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(254)));
            this.lblRightTitle.Location = new System.Drawing.Point(0, 0);
            this.lblRightTitle.Name = "lblRightTitle";
            this.lblRightTitle.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.lblRightTitle.Size = new System.Drawing.Size(157, 19);
            this.lblRightTitle.TabIndex = 0;
            this.lblRightTitle.Text = "Выберите сеанс:";
            this.lblRightTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // FormAfisha
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1184, 565);
            this.Controls.Add(this.pnlRightDetails);
            this.Controls.Add(this.flpEvents);
            this.Controls.Add(this.dtpDate);
            this.Controls.Add(this.pnlFilters);
            this.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(254)));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1200, 600);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(1200, 600);
            this.Name = "FormAfisha";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Афиша мероприятий";
            this.DoubleClick += new System.EventHandler(this.FormAfisha_DoubleClick);
            this.pnlFilters.ResumeLayout(false);
            this.pnlRightDetails.ResumeLayout(false);
            this.pnlRightDetails.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlFilters;
        private System.Windows.Forms.DateTimePicker dtpDate;
        private System.Windows.Forms.FlowLayoutPanel flpEvents;
        private System.Windows.Forms.Panel pnlRightDetails;
        private System.Windows.Forms.Label lblRightTitle;
        private System.Windows.Forms.ComboBox cmbSort;
        private System.Windows.Forms.ComboBox cmbPersonality;
        private System.Windows.Forms.ComboBox cmbVenue;
        private System.Windows.Forms.ComboBox cmbTag;
        private System.Windows.Forms.ComboBox cmbCity;
        private System.Windows.Forms.Button BtnLogin;
        private System.Windows.Forms.Button BtnMyTickets;
    }
}