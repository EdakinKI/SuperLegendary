namespace WindowsFormsApp1
{
    partial class InitialFormStatic
    {
        private System.ComponentModel.IContainer components = null;

        // Измените названия контролов для ясности
        private System.Windows.Forms.Label lblCutoffRatio;
        private System.Windows.Forms.NumericUpDown numCutoffRatio;
        private System.Windows.Forms.Label lblFourierPeriodType;
        private System.Windows.Forms.ComboBox cmbFourierPeriodType;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageInput;
        private System.Windows.Forms.TabPage tabPageAnalysis;
        private System.Windows.Forms.Label lblPowerFile;
        private System.Windows.Forms.TextBox txtPowerFile;
        private System.Windows.Forms.Button btnBrowsePower;
        private System.Windows.Forms.Label lblTemperatureFile;
        private System.Windows.Forms.TextBox txtTemperatureFile;
        private System.Windows.Forms.Button btnBrowseTemperature;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.DataGridView dgvPowerPreview;
        private System.Windows.Forms.DataGridView dgvAnalysis;
        private System.Windows.Forms.Button btnAnalyze;
        private System.Windows.Forms.Label lblPowerPreview;
        private System.Windows.Forms.Label lblAnalysisResults;

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

            this.lblCutoffRatio = new System.Windows.Forms.Label();
            this.numCutoffRatio = new System.Windows.Forms.NumericUpDown();
            this.lblFourierPeriodType = new System.Windows.Forms.Label();
            this.cmbFourierPeriodType = new System.Windows.Forms.ComboBox();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageInput = new System.Windows.Forms.TabPage();
            this.lblAnalysisResults = new System.Windows.Forms.Label();
            this.dgvAnalysis = new System.Windows.Forms.DataGridView();
            this.tabPageAnalysis = new System.Windows.Forms.TabPage();
            this.lblPowerPreview = new System.Windows.Forms.Label();
            this.dgvPowerPreview = new System.Windows.Forms.DataGridView();
            this.btnAnalyze = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnBrowseTemperature = new System.Windows.Forms.Button();
            this.txtTemperatureFile = new System.Windows.Forms.TextBox();
            this.lblTemperatureFile = new System.Windows.Forms.Label();
            this.btnBrowsePower = new System.Windows.Forms.Button();
            this.txtPowerFile = new System.Windows.Forms.TextBox();
            this.lblPowerFile = new System.Windows.Forms.Label();
            this.tabControl.SuspendLayout();
            this.tabPageInput.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAnalysis)).BeginInit();
            this.tabPageAnalysis.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPowerPreview)).BeginInit();
            this.SuspendLayout();


            // Изначально отключаем всё, кроме выбора файла мощности
            lblTemperatureFile.Enabled = false;
            txtTemperatureFile.Enabled = false;
            btnBrowseTemperature.Enabled = false;
            btnAnalyze.Enabled = false;
            lblPowerPreview.Text = "Сначала загрузите файл мощности";

            // tabControl
            this.tabControl.Controls.Add(this.tabPageInput);
            this.tabControl.Controls.Add(this.tabPageAnalysis);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(1000, 700);
            this.tabControl.TabIndex = 0;

            // tabPageInput
            this.tabPageInput.Controls.Add(this.lblPowerPreview);
            this.tabPageInput.Controls.Add(this.dgvPowerPreview);
            this.tabPageInput.Controls.Add(this.btnAnalyze);
            this.tabPageInput.Controls.Add(this.lblStatus);
            this.tabPageInput.Controls.Add(this.btnBrowseTemperature);
            this.tabPageInput.Controls.Add(this.txtTemperatureFile);
            this.tabPageInput.Controls.Add(this.lblTemperatureFile);
            this.tabPageInput.Controls.Add(this.btnBrowsePower);
            this.tabPageInput.Controls.Add(this.txtPowerFile);
            this.tabPageInput.Controls.Add(this.lblPowerFile);
            this.tabPageInput.Location = new System.Drawing.Point(4, 25);
            this.tabPageInput.Name = "tabPageInput";
            this.tabPageInput.Padding = new System.Windows.Forms.Padding(20);
            this.tabPageInput.Size = new System.Drawing.Size(992, 671);
            this.tabPageInput.TabIndex = 0;
            this.tabPageInput.Text = "Загрузка данных";
            this.tabPageInput.UseVisualStyleBackColor = true;

            // lblCutoffRatio
            this.lblCutoffRatio.AutoSize = true;
            this.lblCutoffRatio.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblCutoffRatio.Location = new System.Drawing.Point(20, 180);
            this.lblCutoffRatio.Name = "lblCutoffRatio";
            this.lblCutoffRatio.Size = new System.Drawing.Size(250, 20);
            this.lblCutoffRatio.TabIndex = 11;
            this.lblCutoffRatio.Text = "Процент оставляемых НЧ (%):";

            // numCutoffRatio
            this.numCutoffRatio.DecimalPlaces = 1;
            this.numCutoffRatio.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numCutoffRatio.Location = new System.Drawing.Point(280, 180);
            this.numCutoffRatio.Maximum = new decimal(new int[] {
             100,
             0,
             0,
             0});
            this.numCutoffRatio.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numCutoffRatio.Name = "numCutoffRatio";
            this.numCutoffRatio.Size = new System.Drawing.Size(120, 22);
            this.numCutoffRatio.TabIndex = 12;
            this.numCutoffRatio.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numCutoffRatio.ValueChanged += new System.EventHandler(this.NumCutoffRatio_ValueChanged);

            // lblPeriodType
            this.lblFourierPeriodType.AutoSize = true;
            this.lblFourierPeriodType.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblFourierPeriodType.Location = new System.Drawing.Point(420, 180);
            this.lblFourierPeriodType.Name = "lblPeriodType";
            this.lblFourierPeriodType.Size = new System.Drawing.Size(180, 20);
            this.lblFourierPeriodType.TabIndex = 13;
            this.lblFourierPeriodType.Text = "Тип периода анализа:";

            // cmbPeriodType
            this.cmbFourierPeriodType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFourierPeriodType.FormattingEnabled = true;
            this.cmbFourierPeriodType.Items.AddRange(new object[] {
            "Осенне-зимние периоды (Сен-Май)",
            "Ежемесячные периоды",
            "Еженедельные периоды"});
            this.cmbFourierPeriodType.Location = new System.Drawing.Point(420, 210);
            this.cmbFourierPeriodType.Name = "cmbPeriodType";
            this.cmbFourierPeriodType.Size = new System.Drawing.Size(280, 24);
            this.cmbFourierPeriodType.TabIndex = 14;
            this.cmbFourierPeriodType.SelectedIndexChanged += new System.EventHandler(this.CmbFourierPeriodType_SelectedIndexChanged);

            // lblPowerFile
            this.lblPowerFile.AutoSize = true;
            this.lblPowerFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblPowerFile.Location = new System.Drawing.Point(20, 30);
            this.lblPowerFile.Name = "lblPowerFile";
            this.lblPowerFile.Size = new System.Drawing.Size(220, 20);
            this.lblPowerFile.TabIndex = 0;
            this.lblPowerFile.Text = "Файл с данными мощности:";

            // txtPowerFile
            this.txtPowerFile.Location = new System.Drawing.Point(20, 60);
            this.txtPowerFile.Name = "txtPowerFile";
            this.txtPowerFile.ReadOnly = true;
            this.txtPowerFile.Size = new System.Drawing.Size(500, 22);
            this.txtPowerFile.TabIndex = 1;

            // btnBrowsePower
            this.btnBrowsePower.Location = new System.Drawing.Point(540, 58);
            this.btnBrowsePower.Name = "btnBrowsePower";
            this.btnBrowsePower.Size = new System.Drawing.Size(100, 30);
            this.btnBrowsePower.TabIndex = 2;
            this.btnBrowsePower.Text = "Обзор...";
            this.btnBrowsePower.UseVisualStyleBackColor = true;

            // lblTemperatureFile
            this.lblTemperatureFile.AutoSize = true;
            this.lblTemperatureFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblTemperatureFile.Location = new System.Drawing.Point(20, 110);
            this.lblTemperatureFile.Name = "lblTemperatureFile";
            this.lblTemperatureFile.Size = new System.Drawing.Size(200, 20);
            this.lblTemperatureFile.TabIndex = 3;
            this.lblTemperatureFile.Text = "Файл с температурой:";

            // txtTemperatureFile
            this.txtTemperatureFile.Location = new System.Drawing.Point(20, 140);
            this.txtTemperatureFile.Name = "txtTemperatureFile";
            this.txtTemperatureFile.ReadOnly = true;
            this.txtTemperatureFile.Size = new System.Drawing.Size(500, 22);
            this.txtTemperatureFile.TabIndex = 4;

            // btnBrowseTemperature
            this.btnBrowseTemperature.Location = new System.Drawing.Point(540, 138);
            this.btnBrowseTemperature.Name = "btnBrowseTemperature";
            this.btnBrowseTemperature.Size = new System.Drawing.Size(100, 30);
            this.btnBrowseTemperature.TabIndex = 5;
            this.btnBrowseTemperature.Text = "Обзор...";
            this.btnBrowseTemperature.UseVisualStyleBackColor = true;

            // lblStatus
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblStatus.Location = new System.Drawing.Point(20, 300);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(126, 18);
            this.lblStatus.TabIndex = 6;
            this.lblStatus.Text = "Статус: Ожидание";

            // btnAnalyze
            this.btnAnalyze.Enabled = false;
            this.btnAnalyze.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnAnalyze.Location = new System.Drawing.Point(20, 250);
            this.btnAnalyze.Name = "btnAnalyze";
            this.btnAnalyze.Size = new System.Drawing.Size(680, 40);
            this.btnAnalyze.TabIndex = 8;
            this.btnAnalyze.Text = "Построение графика";
            this.btnAnalyze.UseVisualStyleBackColor = true;

            // dgvPowerPreview
            this.dgvPowerPreview.AllowUserToAddRows = false;
            this.dgvPowerPreview.AllowUserToDeleteRows = false;
            this.dgvPowerPreview.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPowerPreview.Location = new System.Drawing.Point(20, 340);
            this.dgvPowerPreview.Name = "dgvPowerPreview";
            this.dgvPowerPreview.ReadOnly = true;
            this.dgvPowerPreview.RowHeadersVisible = false;
            this.dgvPowerPreview.RowTemplate.Height = 25;
            this.dgvPowerPreview.Size = new System.Drawing.Size(950, 350);
            this.dgvPowerPreview.TabIndex = 9;

            // lblPowerPreview
            this.lblPowerPreview.AutoSize = true;
            this.lblPowerPreview.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblPowerPreview.Location = new System.Drawing.Point(20, 250);
            this.lblPowerPreview.Name = "lblPowerPreview";
            this.lblPowerPreview.Size = new System.Drawing.Size(221, 20);
            this.lblPowerPreview.TabIndex = 10;
            this.lblPowerPreview.Text = "Предпросмотр данных (24ч):";

            // tabPageAnalysis
            this.tabPageAnalysis.Controls.Add(this.lblAnalysisResults);
            this.tabPageAnalysis.Controls.Add(this.dgvAnalysis);
            this.tabPageAnalysis.Location = new System.Drawing.Point(4, 25);
            this.tabPageAnalysis.Name = "tabPageAnalysis";
            this.tabPageAnalysis.Padding = new System.Windows.Forms.Padding(20);
            this.tabPageAnalysis.Size = new System.Drawing.Size(992, 671);
            this.tabPageAnalysis.TabIndex = 1;
            this.tabPageAnalysis.Text = "Анализ зависимости";
            this.tabPageAnalysis.UseVisualStyleBackColor = true;

            // dgvAnalysis
            this.dgvAnalysis.AllowUserToAddRows = false;
            this.dgvAnalysis.AllowUserToDeleteRows = false;
            this.dgvAnalysis.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAnalysis.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvAnalysis.Location = new System.Drawing.Point(20, 50);
            this.dgvAnalysis.Name = "dgvAnalysis";
            this.dgvAnalysis.ReadOnly = true;
            this.dgvAnalysis.RowHeadersVisible = false;
            this.dgvAnalysis.RowTemplate.Height = 25;
            this.dgvAnalysis.Size = new System.Drawing.Size(952, 601);
            this.dgvAnalysis.TabIndex = 0;

            // lblAnalysisResults
            this.lblAnalysisResults.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblAnalysisResults.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblAnalysisResults.Location = new System.Drawing.Point(20, 20);
            this.lblAnalysisResults.Name = "lblAnalysisResults";
            this.lblAnalysisResults.Size = new System.Drawing.Size(952, 30);
            this.lblAnalysisResults.TabIndex = 1;
            this.lblAnalysisResults.Text = "Зависимость мощности от температуры";
            this.lblAnalysisResults.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // InitialFormStatic
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 700);
            this.Controls.Add(this.tabControl);
            this.Name = "InitialFormStatic";
            this.Text = "Статический анализ зависимостей";
            this.tabControl.ResumeLayout(false);
            this.tabPageInput.ResumeLayout(false);
            this.tabPageInput.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAnalysis)).EndInit();
            this.tabPageAnalysis.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvPowerPreview)).EndInit();
            this.ResumeLayout(false);

            // Добавляем контролы на форму
            this.tabPageInput.Controls.Add(this.lblCutoffRatio);
            this.tabPageInput.Controls.Add(this.numCutoffRatio);
            this.tabPageInput.Controls.Add(this.lblFourierPeriodType);
            this.tabPageInput.Controls.Add(this.cmbFourierPeriodType);
        }
    }
}