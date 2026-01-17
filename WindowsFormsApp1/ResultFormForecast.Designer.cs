namespace WindowsFormsApp1
{
    partial class ResultForm
    {
        private System.ComponentModel.IContainer components = null;

        // Основной контейнер
        private System.Windows.Forms.TableLayoutPanel mainTable;
        private System.Windows.Forms.Panel scrollPanel;

        // Панель с исходными данными
        private System.Windows.Forms.GroupBox gbInput;
        private System.Windows.Forms.TableLayoutPanel inputTable;

        // Панель с результатами расчета
        private System.Windows.Forms.GroupBox gbResults;
        private System.Windows.Forms.TableLayoutPanel resultsTable;

        // Таблицы для мощности (сверху вниз)
        private System.Windows.Forms.GroupBox gbPowerSteps;
        private System.Windows.Forms.DataGridView dgvPowerSteps;
        private System.Windows.Forms.GroupBox gbPowerRanges;
        private System.Windows.Forms.DataGridView dgvPowerRangesGrid;

        // Таблицы для электроэнергии (сверху вниз)
        private System.Windows.Forms.GroupBox gbEnergySteps;
        private System.Windows.Forms.DataGridView dgvEnergySteps;
        private System.Windows.Forms.GroupBox gbEnergyRanges;
        private System.Windows.Forms.DataGridView dgvEnergyRangesGrid;

        // Кнопки
        private System.Windows.Forms.FlowLayoutPanel buttonPanel;
        private System.Windows.Forms.Button btnClose;

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
            this.scrollPanel = new System.Windows.Forms.Panel();
            this.gbEnergyRanges = new System.Windows.Forms.GroupBox();
            this.dgvEnergyRangesGrid = new System.Windows.Forms.DataGridView();
            this.gbEnergySteps = new System.Windows.Forms.GroupBox();
            this.dgvEnergySteps = new System.Windows.Forms.DataGridView();
            this.gbPowerRanges = new System.Windows.Forms.GroupBox();
            this.dgvPowerRangesGrid = new System.Windows.Forms.DataGridView();
            this.gbPowerSteps = new System.Windows.Forms.GroupBox();
            this.dgvPowerSteps = new System.Windows.Forms.DataGridView();
            this.gbResults = new System.Windows.Forms.GroupBox();
            this.resultsTable = new System.Windows.Forms.TableLayoutPanel();
            this.gbInput = new System.Windows.Forms.GroupBox();
            this.inputTable = new System.Windows.Forms.TableLayoutPanel();
            this.buttonPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.btnClose = new System.Windows.Forms.Button();

            // Настройка формы
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(1100, 700);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Text = "Результаты расчета";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

            // scrollPanel (панель с прокруткой)
            this.scrollPanel.AutoScroll = true;
            this.scrollPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scrollPanel.Location = new System.Drawing.Point(0, 0);
            this.scrollPanel.Name = "scrollPanel";
            this.scrollPanel.Size = new System.Drawing.Size(1100, 700);
            this.scrollPanel.TabIndex = 0;

            // gbEnergyRanges
            this.gbEnergyRanges.Font = new System.Drawing.Font("Arial", 9F);
            this.gbEnergyRanges.Location = new System.Drawing.Point(18, 628);
            this.gbEnergyRanges.Name = "gbEnergyRanges";
            this.gbEnergyRanges.Size = new System.Drawing.Size(1050, 150);
            this.gbEnergyRanges.TabIndex = 5;
            this.gbEnergyRanges.TabStop = false;
            this.gbEnergyRanges.Text = "Диапазоны температур и коэффициенты (Электроэнергия)";

            // dgvEnergyRangesGrid
            this.dgvEnergyRangesGrid.AllowUserToAddRows = false;
            this.dgvEnergyRangesGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvEnergyRangesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvEnergyRangesGrid.Location = new System.Drawing.Point(3, 19);
            this.dgvEnergyRangesGrid.Name = "dgvEnergyRangesGrid";
            this.dgvEnergyRangesGrid.ReadOnly = true;
            this.dgvEnergyRangesGrid.RowHeadersVisible = false;
            this.dgvEnergyRangesGrid.RowTemplate.Height = 25;
            this.dgvEnergyRangesGrid.Size = new System.Drawing.Size(1044, 128);
            this.dgvEnergyRangesGrid.TabIndex = 0;

            // gbEnergySteps
            this.gbEnergySteps.Font = new System.Drawing.Font("Arial", 9F);
            this.gbEnergySteps.Location = new System.Drawing.Point(18, 468);
            this.gbEnergySteps.Name = "gbEnergySteps";
            this.gbEnergySteps.Size = new System.Drawing.Size(1050, 150);
            this.gbEnergySteps.TabIndex = 4;
            this.gbEnergySteps.TabStop = false;
            this.gbEnergySteps.Text = "Шаги расчета (Электроэнергия)";

            // dgvEnergySteps
            this.dgvEnergySteps.AllowUserToAddRows = false;
            this.dgvEnergySteps.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvEnergySteps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvEnergySteps.Location = new System.Drawing.Point(3, 19);
            this.dgvEnergySteps.Name = "dgvEnergySteps";
            this.dgvEnergySteps.ReadOnly = true;
            this.dgvEnergySteps.RowHeadersVisible = false;
            this.dgvEnergySteps.RowTemplate.Height = 25;
            this.dgvEnergySteps.Size = new System.Drawing.Size(1044, 128);
            this.dgvEnergySteps.TabIndex = 0;

            // gbPowerRanges
            this.gbPowerRanges.Font = new System.Drawing.Font("Arial", 9F);
            this.gbPowerRanges.Location = new System.Drawing.Point(18, 308);
            this.gbPowerRanges.Name = "gbPowerRanges";
            this.gbPowerRanges.Size = new System.Drawing.Size(1050, 150);
            this.gbPowerRanges.TabIndex = 3;
            this.gbPowerRanges.TabStop = false;
            this.gbPowerRanges.Text = "Диапазоны температур и коэффициенты (Мощность)";

            // dgvPowerRangesGrid
            this.dgvPowerRangesGrid.AllowUserToAddRows = false;
            this.dgvPowerRangesGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPowerRangesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvPowerRangesGrid.Location = new System.Drawing.Point(3, 19);
            this.dgvPowerRangesGrid.Name = "dgvPowerRangesGrid";
            this.dgvPowerRangesGrid.ReadOnly = true;
            this.dgvPowerRangesGrid.RowHeadersVisible = false;
            this.dgvPowerRangesGrid.RowTemplate.Height = 25;
            this.dgvPowerRangesGrid.Size = new System.Drawing.Size(1044, 128);
            this.dgvPowerRangesGrid.TabIndex = 0;

            // gbPowerSteps
            this.gbPowerSteps.Font = new System.Drawing.Font("Arial", 9F);
            this.gbPowerSteps.Location = new System.Drawing.Point(18, 148);
            this.gbPowerSteps.Name = "gbPowerSteps";
            this.gbPowerSteps.Size = new System.Drawing.Size(1050, 150);
            this.gbPowerSteps.TabIndex = 2;
            this.gbPowerSteps.TabStop = false;
            this.gbPowerSteps.Text = "Шаги расчета (Мощность)";

            // dgvPowerSteps
            this.dgvPowerSteps.AllowUserToAddRows = false;
            this.dgvPowerSteps.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPowerSteps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvPowerSteps.Location = new System.Drawing.Point(3, 19);
            this.dgvPowerSteps.Name = "dgvPowerSteps";
            this.dgvPowerSteps.ReadOnly = true;
            this.dgvPowerSteps.RowHeadersVisible = false;
            this.dgvPowerSteps.RowTemplate.Height = 25;
            this.dgvPowerSteps.Size = new System.Drawing.Size(1044, 128);
            this.dgvPowerSteps.TabIndex = 0;

            // gbResults
            this.gbResults.Font = new System.Drawing.Font("Arial", 9F);
            this.gbResults.Location = new System.Drawing.Point(18, 748);
            this.gbResults.Name = "gbResults";
            this.gbResults.Size = new System.Drawing.Size(1050, 100);
            this.gbResults.TabIndex = 1;
            this.gbResults.TabStop = false;
            this.gbResults.Text = "Результаты расчета";

            // resultsTable
            this.resultsTable.ColumnCount = 4;
            this.resultsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 180F));
            this.resultsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 180F));
            this.resultsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 180F));
            this.resultsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.resultsTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resultsTable.Location = new System.Drawing.Point(3, 19);
            this.resultsTable.Name = "resultsTable";
            this.resultsTable.RowCount = 1;
            this.resultsTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.resultsTable.Size = new System.Drawing.Size(1044, 78);
            this.resultsTable.TabIndex = 0;

            // gbInput
            this.gbInput.Font = new System.Drawing.Font("Arial", 9F);
            this.gbInput.Location = new System.Drawing.Point(18, 18);
            this.gbInput.Name = "gbInput";
            this.gbInput.Size = new System.Drawing.Size(1050, 120);
            this.gbInput.TabIndex = 0;
            this.gbInput.TabStop = false;
            this.gbInput.Text = "Исходные данные";

            // inputTable
            this.inputTable.ColumnCount = 6;
            this.inputTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 180F));
            this.inputTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 180F));
            this.inputTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 180F));
            this.inputTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 180F));
            this.inputTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 180F));
            this.inputTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.inputTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputTable.Location = new System.Drawing.Point(3, 19);
            this.inputTable.Name = "inputTable";
            this.inputTable.RowCount = 4;
            this.inputTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.inputTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.inputTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.inputTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.inputTable.Size = new System.Drawing.Size(1044, 98);
            this.inputTable.TabIndex = 0;

            // buttonPanel
            this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.buttonPanel.Location = new System.Drawing.Point(0, 870);
            this.buttonPanel.Name = "buttonPanel";
            this.buttonPanel.Padding = new System.Windows.Forms.Padding(10);
            this.buttonPanel.Size = new System.Drawing.Size(1083, 60);
            this.buttonPanel.TabIndex = 6;

            // btnClose
            this.btnClose.Font = new System.Drawing.Font("Arial", 9F);
            this.btnClose.Location = new System.Drawing.Point(953, 13);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(120, 35);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "Закрыть";
            this.btnClose.UseVisualStyleBackColor = true;

            // Добавление контролов
            this.gbPowerSteps.Controls.Add(this.dgvPowerSteps);
            this.gbPowerRanges.Controls.Add(this.dgvPowerRangesGrid);
            this.gbEnergySteps.Controls.Add(this.dgvEnergySteps);
            this.gbEnergyRanges.Controls.Add(this.dgvEnergyRangesGrid);
            this.gbInput.Controls.Add(this.inputTable);
            this.gbResults.Controls.Add(this.resultsTable);

            this.buttonPanel.Controls.Add(this.btnClose);

            this.Controls.Add(this.scrollPanel);

            this.ResumeLayout(false);

            this.scrollPanel.Controls.Add(this.gbInput);
            this.scrollPanel.Controls.Add(this.gbResults);
            this.scrollPanel.Controls.Add(this.gbPowerSteps);
            this.scrollPanel.Controls.Add(this.gbPowerRanges);
            this.scrollPanel.Controls.Add(this.gbEnergySteps);
            this.scrollPanel.Controls.Add(this.gbEnergyRanges);
            this.scrollPanel.Controls.Add(this.buttonPanel);
        }
    }
}