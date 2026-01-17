namespace WindowsFormsApp1
{
    partial class UpdateCoefForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblFile;
        private System.Windows.Forms.TextBox txtFilePath;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label lblSetName;
        private System.Windows.Forms.TextBox txtSetName;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblSheetSelector;
        private System.Windows.Forms.ComboBox cmbSheetSelector;
        private System.Windows.Forms.DataGridView dgvSystems;
        private System.Windows.Forms.Label lblInfo;

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
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblFile = new System.Windows.Forms.Label();
            this.txtFilePath = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.lblSetName = new System.Windows.Forms.Label();
            this.txtSetName = new System.Windows.Forms.TextBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblSheetSelector = new System.Windows.Forms.Label();
            this.cmbSheetSelector = new System.Windows.Forms.ComboBox();
            this.dgvSystems = new System.Windows.Forms.DataGridView();
            this.lblInfo = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSystems)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(884, 40);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Обновление коэффициентов влияния";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFile
            // 
            this.lblFile.AutoSize = true;
            this.lblFile.Location = new System.Drawing.Point(12, 50);
            this.lblFile.Name = "lblFile";
            this.lblFile.Size = new System.Drawing.Size(81, 16);
            this.lblFile.TabIndex = 1;
            this.lblFile.Text = "Excel файл:";
            // 
            // txtFilePath
            // 
            this.txtFilePath.Location = new System.Drawing.Point(99, 47);
            this.txtFilePath.Name = "txtFilePath";
            this.txtFilePath.ReadOnly = true;
            this.txtFilePath.Size = new System.Drawing.Size(429, 22);
            this.txtFilePath.TabIndex = 2;
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(546, 46);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 25);
            this.btnBrowse.TabIndex = 3;
            this.btnBrowse.Text = "Обзор";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // lblSetName
            // 
            this.lblSetName.AutoSize = true;
            this.lblSetName.Location = new System.Drawing.Point(12, 90);
            this.lblSetName.Name = "lblSetName";
            this.lblSetName.Size = new System.Drawing.Size(238, 16);
            this.lblSetName.TabIndex = 7;
            this.lblSetName.Text = "Название набора коэффициентов:";
            this.lblSetName.Click += new System.EventHandler(this.lblSetName_Click);
            // 
            // txtSetName
            // 
            this.txtSetName.Location = new System.Drawing.Point(256, 87);
            this.txtSetName.Name = "txtSetName";
            this.txtSetName.Size = new System.Drawing.Size(272, 22);
            this.txtSetName.TabIndex = 8;
            // 
            // btnSave
            // 
            this.btnSave.Enabled = false;
            this.btnSave.Location = new System.Drawing.Point(628, 554);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(119, 30);
            this.btnSave.TabIndex = 14;
            this.btnSave.Text = "Сохранить";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(753, 554);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(119, 30);
            this.btnCancel.TabIndex = 15;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // lblSheetSelector
            // 
            this.lblSheetSelector.AutoSize = true;
            this.lblSheetSelector.Location = new System.Drawing.Point(16, 128);
            this.lblSheetSelector.Name = "lblSheetSelector";
            this.lblSheetSelector.Size = new System.Drawing.Size(108, 16);
            this.lblSheetSelector.TabIndex = 10;
            this.lblSheetSelector.Text = "Выберите лист:";
            this.lblSheetSelector.Visible = false;
            // 
            // cmbSheetSelector
            // 
            this.cmbSheetSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSheetSelector.FormattingEnabled = true;
            this.cmbSheetSelector.Location = new System.Drawing.Point(256, 125);
            this.cmbSheetSelector.Name = "cmbSheetSelector";
            this.cmbSheetSelector.Size = new System.Drawing.Size(272, 24);
            this.cmbSheetSelector.TabIndex = 11;
            this.cmbSheetSelector.Visible = false;
            this.cmbSheetSelector.SelectedIndexChanged += new System.EventHandler(this.CmbSheetSelector_SelectedIndexChanged);
            // 
            // dgvSystems
            // 
            this.dgvSystems.AllowUserToAddRows = false;
            this.dgvSystems.AllowUserToDeleteRows = false;
            this.dgvSystems.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvSystems.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSystems.Location = new System.Drawing.Point(12, 173);
            this.dgvSystems.Name = "dgvSystems";
            this.dgvSystems.ReadOnly = true;
            this.dgvSystems.RowHeadersVisible = false;
            this.dgvSystems.RowHeadersWidth = 51;
            this.dgvSystems.Size = new System.Drawing.Size(860, 349);
            this.dgvSystems.TabIndex = 12;
            // 
            // lblInfo
            // 
            this.lblInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblInfo.Location = new System.Drawing.Point(12, 525);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(860, 44);
            this.lblInfo.TabIndex = 13;
            this.lblInfo.Text = "Все найденные энергосистемы будут сохранены одновременно.";
            // 
            // UpdateCoefForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 598);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.dgvSystems);
            this.Controls.Add(this.cmbSheetSelector);
            this.Controls.Add(this.lblSheetSelector);
            this.Controls.Add(this.txtSetName);
            this.Controls.Add(this.lblSetName);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.txtFilePath);
            this.Controls.Add(this.lblFile);
            this.Controls.Add(this.lblTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "UpdateCoefForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Обновление коэффициентов влияния";
            ((System.ComponentModel.ISupportInitialize)(this.dgvSystems)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}