namespace WindowsFormsApp1
{
    partial class CalculationHistoryForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel panelHistory;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel panelFilters;
        private System.Windows.Forms.Label lblFilterType;
        private System.Windows.Forms.ComboBox cmbFilterType;
        private System.Windows.Forms.Label lblFilterDateFrom;
        private System.Windows.Forms.DateTimePicker dtpFilterDateFrom;
        private System.Windows.Forms.Label lblFilterDateTo;
        private System.Windows.Forms.DateTimePicker dtpFilterDateTo;
        private System.Windows.Forms.Button btnApplyFilters;
        private System.Windows.Forms.Button btnClearFilters;
        private System.Windows.Forms.CheckBox chkShowAll;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.panelHistory = new System.Windows.Forms.Panel();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.panelHeader = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.panelFilters = new System.Windows.Forms.Panel();
            this.chkShowAll = new System.Windows.Forms.CheckBox();
            this.btnClearFilters = new System.Windows.Forms.Button();
            this.btnApplyFilters = new System.Windows.Forms.Button();
            this.dtpFilterDateTo = new System.Windows.Forms.DateTimePicker();
            this.lblFilterDateTo = new System.Windows.Forms.Label();
            this.dtpFilterDateFrom = new System.Windows.Forms.DateTimePicker();
            this.lblFilterDateFrom = new System.Windows.Forms.Label();
            this.cmbFilterType = new System.Windows.Forms.ComboBox();
            this.lblFilterType = new System.Windows.Forms.Label();
            this.panelHeader.SuspendLayout();
            this.panelFilters.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelHistory
            // 
            this.panelHistory.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelHistory.AutoScroll = true;
            this.panelHistory.BackColor = System.Drawing.SystemColors.Control;
            this.panelHistory.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelHistory.Location = new System.Drawing.Point(12, 130);
            this.panelHistory.Name = "panelHistory";
            this.panelHistory.Size = new System.Drawing.Size(1100, 322);
            this.panelHistory.TabIndex = 0;
            // 
            // btnRefresh
            // 
            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefresh.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.btnRefresh.Location = new System.Drawing.Point(912, 458);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(200, 35);
            this.btnRefresh.TabIndex = 1;
            this.btnRefresh.Text = "Обновить данные";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.btnClose.Location = new System.Drawing.Point(12, 458);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(200, 35);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "Закрыть";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // panelHeader
            // 
            this.panelHeader.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panelHeader.Controls.Add(this.lblTitle);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(0, 0);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new System.Drawing.Size(1124, 50);
            this.panelHeader.TabIndex = 3;
            // 
            // lblTitle
            // 
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(1124, 50);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "История расчетов";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panelFilters
            // 
            this.panelFilters.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelFilters.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.panelFilters.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelFilters.Controls.Add(this.chkShowAll);
            this.panelFilters.Controls.Add(this.btnClearFilters);
            this.panelFilters.Controls.Add(this.btnApplyFilters);
            this.panelFilters.Controls.Add(this.dtpFilterDateTo);
            this.panelFilters.Controls.Add(this.lblFilterDateTo);
            this.panelFilters.Controls.Add(this.dtpFilterDateFrom);
            this.panelFilters.Controls.Add(this.lblFilterDateFrom);
            this.panelFilters.Controls.Add(this.cmbFilterType);
            this.panelFilters.Controls.Add(this.lblFilterType);
            this.panelFilters.Location = new System.Drawing.Point(12, 56);
            this.panelFilters.Name = "panelFilters";
            this.panelFilters.Size = new System.Drawing.Size(1100, 68);
            this.panelFilters.TabIndex = 4;
            // 
            // chkShowAll
            // 
            this.chkShowAll.AutoSize = true;
            this.chkShowAll.Checked = true;
            this.chkShowAll.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowAll.Location = new System.Drawing.Point(717, 31);
            this.chkShowAll.Name = "chkShowAll";
            this.chkShowAll.Size = new System.Drawing.Size(88, 20);
            this.chkShowAll.TabIndex = 8;
            this.chkShowAll.Text = "Все даты";
            this.chkShowAll.UseVisualStyleBackColor = true;
            this.chkShowAll.CheckedChanged += new System.EventHandler(this.chkShowAll_CheckedChanged);
            // 
            // btnClearFilters
            // 
            this.btnClearFilters.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnClearFilters.Location = new System.Drawing.Point(995, 25);
            this.btnClearFilters.Name = "btnClearFilters";
            this.btnClearFilters.Size = new System.Drawing.Size(96, 30);
            this.btnClearFilters.TabIndex = 7;
            this.btnClearFilters.Text = "Сбросить";
            this.btnClearFilters.UseVisualStyleBackColor = true;
            this.btnClearFilters.Click += new System.EventHandler(this.btnClearFilters_Click);
            // 
            // btnApplyFilters
            // 
            this.btnApplyFilters.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnApplyFilters.Location = new System.Drawing.Point(881, 25);
            this.btnApplyFilters.Name = "btnApplyFilters";
            this.btnApplyFilters.Size = new System.Drawing.Size(108, 30);
            this.btnApplyFilters.TabIndex = 6;
            this.btnApplyFilters.Text = "Применить";
            this.btnApplyFilters.UseVisualStyleBackColor = true;
            this.btnApplyFilters.Click += new System.EventHandler(this.btnApplyFilters_Click);
            // 
            // dtpFilterDateTo
            // 
            this.dtpFilterDateTo.CustomFormat = "dd.MM.yyyy";
            this.dtpFilterDateTo.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpFilterDateTo.Location = new System.Drawing.Point(578, 29);
            this.dtpFilterDateTo.Name = "dtpFilterDateTo";
            this.dtpFilterDateTo.Size = new System.Drawing.Size(120, 22);
            this.dtpFilterDateTo.TabIndex = 5;
            // 
            // lblFilterDateTo
            // 
            this.lblFilterDateTo.AutoSize = true;
            this.lblFilterDateTo.Location = new System.Drawing.Point(510, 31);
            this.lblFilterDateTo.Name = "lblFilterDateTo";
            this.lblFilterDateTo.Size = new System.Drawing.Size(62, 16);
            this.lblFilterDateTo.TabIndex = 4;
            this.lblFilterDateTo.Text = "До даты:";
            // 
            // dtpFilterDateFrom
            // 
            this.dtpFilterDateFrom.CustomFormat = "dd.MM.yyyy";
            this.dtpFilterDateFrom.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpFilterDateFrom.Location = new System.Drawing.Point(371, 28);
            this.dtpFilterDateFrom.Name = "dtpFilterDateFrom";
            this.dtpFilterDateFrom.Size = new System.Drawing.Size(120, 22);
            this.dtpFilterDateFrom.TabIndex = 3;
            // 
            // lblFilterDateFrom
            // 
            this.lblFilterDateFrom.AutoSize = true;
            this.lblFilterDateFrom.Location = new System.Drawing.Point(306, 31);
            this.lblFilterDateFrom.Name = "lblFilterDateFrom";
            this.lblFilterDateFrom.Size = new System.Drawing.Size(62, 16);
            this.lblFilterDateFrom.TabIndex = 2;
            this.lblFilterDateFrom.Text = "От даты:";
            // 
            // cmbFilterType
            // 
            this.cmbFilterType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFilterType.FormattingEnabled = true;
            this.cmbFilterType.Items.AddRange(new object[] {
            "Все типы",
            "Прогноз потребления по мощности",
            "Прогноз потребления по электроэнергии",
            "Прогноз потребления по мощности и электроэнергии",
            "Расчет статических зависимостей"});
            this.cmbFilterType.Location = new System.Drawing.Point(16, 25);
            this.cmbFilterType.Name = "cmbFilterType";
            this.cmbFilterType.Size = new System.Drawing.Size(271, 24);
            this.cmbFilterType.TabIndex = 1;
            // 
            // lblFilterType
            // 
            this.lblFilterType.AutoSize = true;
            this.lblFilterType.Location = new System.Drawing.Point(9, 5);
            this.lblFilterType.Name = "lblFilterType";
            this.lblFilterType.Size = new System.Drawing.Size(92, 16);
            this.lblFilterType.TabIndex = 0;
            this.lblFilterType.Text = "Тип расчета:";
            // 
            // CalculationHistoryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1124, 503);
            this.Controls.Add(this.panelFilters);
            this.Controls.Add(this.panelHeader);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.panelHistory);
            this.MaximumSize = new System.Drawing.Size(1142, 550);
            this.MinimumSize = new System.Drawing.Size(800, 550);
            this.Name = "CalculationHistoryForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "История расчетов";
            this.Load += new System.EventHandler(this.CalculationHistoryForm_Load);
            this.panelHeader.ResumeLayout(false);
            this.panelFilters.ResumeLayout(false);
            this.panelFilters.PerformLayout();
            this.ResumeLayout(false);

        }
    }
}