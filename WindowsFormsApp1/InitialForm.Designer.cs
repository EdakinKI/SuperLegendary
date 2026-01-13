namespace WindowsFormsApp1
{
    partial class InitialFormDb
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblCalculationType;
        private System.Windows.Forms.ComboBox cmbCalculationType;
        private System.Windows.Forms.Label lblSystem;
        private System.Windows.Forms.ComboBox cmbSystems;
        private System.Windows.Forms.Label lblParamSet;
        private System.Windows.Forms.ComboBox cmbParamSets;
        private System.Windows.Forms.Label lblT1;
        private System.Windows.Forms.TextBox txtT1;
        private System.Windows.Forms.Label lblT2;
        private System.Windows.Forms.TextBox txtT2;
        private System.Windows.Forms.Label lblP1;
        private System.Windows.Forms.TextBox txtP1;
        private System.Windows.Forms.Label lblE1;
        private System.Windows.Forms.TextBox txtE1;
        private System.Windows.Forms.Label lblForecastDate;
        private System.Windows.Forms.DateTimePicker dtpForecastDate;
        private System.Windows.Forms.Button btnCalculate;
        private System.Windows.Forms.GroupBox gbPowerRanges;
        private System.Windows.Forms.DataGridView dgvPowerRanges;
        private System.Windows.Forms.GroupBox gbEnergyRanges;
        private System.Windows.Forms.DataGridView dgvEnergyRanges;

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
            this.lblCalculationType = new System.Windows.Forms.Label();
            this.cmbCalculationType = new System.Windows.Forms.ComboBox();
            this.lblSystem = new System.Windows.Forms.Label();
            this.cmbSystems = new System.Windows.Forms.ComboBox();
            this.lblParamSet = new System.Windows.Forms.Label();
            this.cmbParamSets = new System.Windows.Forms.ComboBox();
            this.lblT1 = new System.Windows.Forms.Label();
            this.txtT1 = new System.Windows.Forms.TextBox();
            this.lblT2 = new System.Windows.Forms.Label();
            this.txtT2 = new System.Windows.Forms.TextBox();
            this.lblP1 = new System.Windows.Forms.Label();
            this.txtP1 = new System.Windows.Forms.TextBox();
            this.lblE1 = new System.Windows.Forms.Label();
            this.txtE1 = new System.Windows.Forms.TextBox();
            this.lblForecastDate = new System.Windows.Forms.Label();
            this.dtpForecastDate = new System.Windows.Forms.DateTimePicker();
            this.btnCalculate = new System.Windows.Forms.Button();
            this.gbPowerRanges = new System.Windows.Forms.GroupBox();
            this.dgvPowerRanges = new System.Windows.Forms.DataGridView();
            this.gbEnergyRanges = new System.Windows.Forms.GroupBox();
            this.dgvEnergyRanges = new System.Windows.Forms.DataGridView();
            this.gbPowerRanges.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPowerRanges)).BeginInit();
            this.gbEnergyRanges.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvEnergyRanges)).BeginInit();
            this.SuspendLayout();
            // 
            // lblCalculationType
            // 
            this.lblCalculationType.AutoSize = true;
            this.lblCalculationType.Location = new System.Drawing.Point(12, 20);
            this.lblCalculationType.Name = "lblCalculationType";
            this.lblCalculationType.Size = new System.Drawing.Size(92, 16);
            this.lblCalculationType.TabIndex = 0;
            this.lblCalculationType.Text = "Тип расчета:";
            // 
            // cmbCalculationType
            // 
            this.cmbCalculationType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCalculationType.FormattingEnabled = true;
            this.cmbCalculationType.Location = new System.Drawing.Point(103, 17);
            this.cmbCalculationType.Name = "cmbCalculationType";
            this.cmbCalculationType.Size = new System.Drawing.Size(250, 24);
            this.cmbCalculationType.TabIndex = 1;
            // 
            // lblSystem
            // 
            this.lblSystem.AutoSize = true;
            this.lblSystem.Location = new System.Drawing.Point(12, 50);
            this.lblSystem.Name = "lblSystem";
            this.lblSystem.Size = new System.Drawing.Size(112, 16);
            this.lblSystem.TabIndex = 2;
            this.lblSystem.Text = "Энергосистема:";
            // 
            // cmbSystems
            // 
            this.cmbSystems.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSystems.FormattingEnabled = true;
            this.cmbSystems.Location = new System.Drawing.Point(127, 47);
            this.cmbSystems.Name = "cmbSystems";
            this.cmbSystems.Size = new System.Drawing.Size(250, 24);
            this.cmbSystems.TabIndex = 3;
            // 
            // lblParamSet
            // 
            this.lblParamSet.AutoSize = true;
            this.lblParamSet.Location = new System.Drawing.Point(12, 80);
            this.lblParamSet.Name = "lblParamSet";
            this.lblParamSet.Size = new System.Drawing.Size(178, 16);
            this.lblParamSet.TabIndex = 4;
            this.lblParamSet.Text = "Таблица коэффициентов:";
            // 
            // cmbParamSets
            // 
            this.cmbParamSets.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbParamSets.FormattingEnabled = true;
            this.cmbParamSets.Location = new System.Drawing.Point(187, 77);
            this.cmbParamSets.Name = "cmbParamSets";
            this.cmbParamSets.Size = new System.Drawing.Size(250, 24);
            this.cmbParamSets.TabIndex = 5;
            // 
            // lblT1
            // 
            this.lblT1.AutoSize = true;
            this.lblT1.Location = new System.Drawing.Point(12, 110);
            this.lblT1.Name = "lblT1";
            this.lblT1.Size = new System.Drawing.Size(50, 16);
            this.lblT1.TabIndex = 6;
            this.lblT1.Text = "Т1 (°C):";
            // 
            // txtT1
            // 
            this.txtT1.Location = new System.Drawing.Point(94, 107);
            this.txtT1.Name = "txtT1";
            this.txtT1.Size = new System.Drawing.Size(100, 22);
            this.txtT1.TabIndex = 7;
            // 
            // lblT2
            // 
            this.lblT2.AutoSize = true;
            this.lblT2.Location = new System.Drawing.Point(200, 110);
            this.lblT2.Name = "lblT2";
            this.lblT2.Size = new System.Drawing.Size(50, 16);
            this.lblT2.TabIndex = 8;
            this.lblT2.Text = "Т2 (°C):";
            // 
            // txtT2
            // 
            this.txtT2.Location = new System.Drawing.Point(282, 107);
            this.txtT2.Name = "txtT2";
            this.txtT2.Size = new System.Drawing.Size(100, 22);
            this.txtT2.TabIndex = 9;
            // 
            // lblP1
            // 
            this.lblP1.AutoSize = true;
            this.lblP1.Location = new System.Drawing.Point(12, 140);
            this.lblP1.Name = "lblP1";
            this.lblP1.Size = new System.Drawing.Size(64, 16);
            this.lblP1.TabIndex = 10;
            this.lblP1.Text = "Р1 (МВт):";
            // 
            // txtP1
            // 
            this.txtP1.Location = new System.Drawing.Point(102, 137);
            this.txtP1.Name = "txtP1";
            this.txtP1.Size = new System.Drawing.Size(120, 22);
            this.txtP1.TabIndex = 11;
            // 
            // lblE1
            // 
            this.lblE1.AutoSize = true;
            this.lblE1.Location = new System.Drawing.Point(228, 140);
            this.lblE1.Name = "lblE1";
            this.lblE1.Size = new System.Drawing.Size(101, 16);
            this.lblE1.TabIndex = 12;
            this.lblE1.Text = "Е1 (млн кВт·ч):";
            // 
            // txtE1
            // 
            this.txtE1.Location = new System.Drawing.Point(365, 137);
            this.txtE1.Name = "txtE1";
            this.txtE1.Size = new System.Drawing.Size(120, 22);
            this.txtE1.TabIndex = 13;
            // 
            // lblForecastDate
            // 
            this.lblForecastDate.AutoSize = true;
            this.lblForecastDate.Location = new System.Drawing.Point(12, 170);
            this.lblForecastDate.Name = "lblForecastDate";
            this.lblForecastDate.Size = new System.Drawing.Size(107, 16);
            this.lblForecastDate.TabIndex = 14;
            this.lblForecastDate.Text = "Дата прогноза:";
            // 
            // dtpForecastDate
            // 
            this.dtpForecastDate.Location = new System.Drawing.Point(113, 167);
            this.dtpForecastDate.Name = "dtpForecastDate";
            this.dtpForecastDate.Size = new System.Drawing.Size(150, 22);
            this.dtpForecastDate.TabIndex = 15;
            // 
            // btnCalculate
            // 
            this.btnCalculate.Location = new System.Drawing.Point(300, 165);
            this.btnCalculate.Name = "btnCalculate";
            this.btnCalculate.Size = new System.Drawing.Size(120, 30);
            this.btnCalculate.TabIndex = 16;
            this.btnCalculate.Text = "Рассчитать";
            this.btnCalculate.UseVisualStyleBackColor = true;
            // 
            // gbPowerRanges
            // 
            this.gbPowerRanges.Controls.Add(this.dgvPowerRanges);
            this.gbPowerRanges.Location = new System.Drawing.Point(12, 210);
            this.gbPowerRanges.Name = "gbPowerRanges";
            this.gbPowerRanges.Size = new System.Drawing.Size(560, 200);
            this.gbPowerRanges.TabIndex = 17;
            this.gbPowerRanges.TabStop = false;
            this.gbPowerRanges.Text = "Диапазоны температур и коэффициенты (Мощность)";
            // 
            // dgvPowerRanges
            // 
            this.dgvPowerRanges.AllowUserToAddRows = false;
            this.dgvPowerRanges.AllowUserToDeleteRows = false;
            this.dgvPowerRanges.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPowerRanges.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvPowerRanges.Location = new System.Drawing.Point(3, 18);
            this.dgvPowerRanges.Name = "dgvPowerRanges";
            this.dgvPowerRanges.ReadOnly = true;
            this.dgvPowerRanges.RowHeadersVisible = false;
            this.dgvPowerRanges.RowHeadersWidth = 51;
            this.dgvPowerRanges.Size = new System.Drawing.Size(554, 179);
            this.dgvPowerRanges.TabIndex = 0;
            // 
            // gbEnergyRanges
            // 
            this.gbEnergyRanges.Controls.Add(this.dgvEnergyRanges);
            this.gbEnergyRanges.Location = new System.Drawing.Point(12, 420);
            this.gbEnergyRanges.Name = "gbEnergyRanges";
            this.gbEnergyRanges.Size = new System.Drawing.Size(560, 200);
            this.gbEnergyRanges.TabIndex = 18;
            this.gbEnergyRanges.TabStop = false;
            this.gbEnergyRanges.Text = "Диапазоны температур и коэффициенты (Электроэнергия)";
            // 
            // dgvEnergyRanges
            // 
            this.dgvEnergyRanges.AllowUserToAddRows = false;
            this.dgvEnergyRanges.AllowUserToDeleteRows = false;
            this.dgvEnergyRanges.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvEnergyRanges.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvEnergyRanges.Location = new System.Drawing.Point(3, 18);
            this.dgvEnergyRanges.Name = "dgvEnergyRanges";
            this.dgvEnergyRanges.ReadOnly = true;
            this.dgvEnergyRanges.RowHeadersVisible = false;
            this.dgvEnergyRanges.RowHeadersWidth = 51;
            this.dgvEnergyRanges.Size = new System.Drawing.Size(554, 179);
            this.dgvEnergyRanges.TabIndex = 0;
            // 
            // InitialFormDb
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 632);
            this.Controls.Add(this.gbEnergyRanges);
            this.Controls.Add(this.gbPowerRanges);
            this.Controls.Add(this.btnCalculate);
            this.Controls.Add(this.dtpForecastDate);
            this.Controls.Add(this.lblForecastDate);
            this.Controls.Add(this.txtE1);
            this.Controls.Add(this.lblE1);
            this.Controls.Add(this.txtP1);
            this.Controls.Add(this.lblP1);
            this.Controls.Add(this.txtT2);
            this.Controls.Add(this.lblT2);
            this.Controls.Add(this.txtT1);
            this.Controls.Add(this.lblT1);
            this.Controls.Add(this.cmbParamSets);
            this.Controls.Add(this.lblParamSet);
            this.Controls.Add(this.cmbSystems);
            this.Controls.Add(this.lblSystem);
            this.Controls.Add(this.cmbCalculationType);
            this.Controls.Add(this.lblCalculationType);
            this.Name = "InitialFormDb";
            this.Text = "Расчет температурных зависимостей";
            this.gbPowerRanges.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvPowerRanges)).EndInit();
            this.gbEnergyRanges.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvEnergyRanges)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}