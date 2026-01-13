namespace WindowsFormsApp1
{
    partial class UpdateCoefForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblFile;
        private System.Windows.Forms.TextBox txtFilePath;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label lblSheetType;
        private System.Windows.Forms.Label lblSystem;
        private System.Windows.Forms.ComboBox cmbSystems;
        private System.Windows.Forms.Label lblSetName;
        private System.Windows.Forms.TextBox txtSetName;
        private System.Windows.Forms.DataGridView dgvRanges;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;

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
            this.lblSheetType = new System.Windows.Forms.Label();
            this.lblSystem = new System.Windows.Forms.Label();
            this.cmbSystems = new System.Windows.Forms.ComboBox();
            this.lblSetName = new System.Windows.Forms.Label();
            this.txtSetName = new System.Windows.Forms.TextBox();
            this.dgvRanges = new System.Windows.Forms.DataGridView();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRanges)).BeginInit();
            this.SuspendLayout();

            // lblTitle
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(584, 40);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Обновление коэффициентов влияния";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // lblFile
            this.lblFile.AutoSize = true;
            this.lblFile.Location = new System.Drawing.Point(12, 50);
            this.lblFile.Name = "lblFile";
            this.lblFile.Size = new System.Drawing.Size(67, 16);
            this.lblFile.TabIndex = 1;
            this.lblFile.Text = "Excel файл:";

            // txtFilePath
            this.txtFilePath.Location = new System.Drawing.Point(85, 47);
            this.txtFilePath.Name = "txtFilePath";
            this.txtFilePath.Size = new System.Drawing.Size(400, 22);
            this.txtFilePath.TabIndex = 2;
            this.txtFilePath.ReadOnly = true;

            // btnBrowse
            this.btnBrowse.Location = new System.Drawing.Point(491, 46);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 25);
            this.btnBrowse.TabIndex = 3;
            this.btnBrowse.Text = "Обзор";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);

            // lblSheetType
            this.lblSheetType.AutoSize = true;
            this.lblSheetType.Location = new System.Drawing.Point(12, 80);
            this.lblSheetType.Name = "lblSheetType";
            this.lblSheetType.Size = new System.Drawing.Size(100, 16);
            this.lblSheetType.TabIndex = 4;
            this.lblSheetType.Text = "Тип листа: ---";

            // lblSystem
            this.lblSystem.AutoSize = true;
            this.lblSystem.Location = new System.Drawing.Point(12, 110);
            this.lblSystem.Name = "lblSystem";
            this.lblSystem.Size = new System.Drawing.Size(109, 16);
            this.lblSystem.TabIndex = 5;
            this.lblSystem.Text = "Энергосистема:";

            // cmbSystems
            this.cmbSystems.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSystems.FormattingEnabled = true;
            this.cmbSystems.Location = new System.Drawing.Point(127, 107);
            this.cmbSystems.Name = "cmbSystems";
            this.cmbSystems.Size = new System.Drawing.Size(200, 24);
            this.cmbSystems.TabIndex = 6;
            this.cmbSystems.SelectedIndexChanged += new System.EventHandler(this.cmbSystems_SelectedIndexChanged);

            // lblSetName
            this.lblSetName.AutoSize = true;
            this.lblSetName.Location = new System.Drawing.Point(12, 140);
            this.lblSetName.Name = "lblSetName";
            this.lblSetName.Size = new System.Drawing.Size(148, 16);
            this.lblSetName.TabIndex = 7;
            this.lblSetName.Text = "Название набора коэф.:";

            // txtSetName
            this.txtSetName.Location = new System.Drawing.Point(166, 137);
            this.txtSetName.Name = "txtSetName";
            this.txtSetName.Size = new System.Drawing.Size(200, 22);
            this.txtSetName.TabIndex = 8;

            // dgvRanges
            this.dgvRanges.AllowUserToAddRows = false;
            this.dgvRanges.AllowUserToDeleteRows = false;
            this.dgvRanges.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRanges.Location = new System.Drawing.Point(12, 170);
            this.dgvRanges.Name = "dgvRanges";
            this.dgvRanges.ReadOnly = true;
            this.dgvRanges.RowHeadersVisible = false;
            this.dgvRanges.RowHeadersWidth = 51;
            this.dgvRanges.Size = new System.Drawing.Size(560, 150);
            this.dgvRanges.TabIndex = 9;

            // btnSave
            this.btnSave.Enabled = false;
            this.btnSave.Location = new System.Drawing.Point(416, 330);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 30);
            this.btnSave.TabIndex = 10;
            this.btnSave.Text = "Сохранить";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);

            // btnCancel
            this.btnCancel.Location = new System.Drawing.Point(497, 330);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 30);
            this.btnCancel.TabIndex = 11;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);

            // UpdateCoefForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 370);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.dgvRanges);
            this.Controls.Add(this.txtSetName);
            this.Controls.Add(this.lblSetName);
            this.Controls.Add(this.cmbSystems);
            this.Controls.Add(this.lblSystem);
            this.Controls.Add(this.lblSheetType);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.txtFilePath);
            this.Controls.Add(this.lblFile);
            this.Controls.Add(this.lblTitle);
            this.Name = "UpdateCoefForm";
            this.Text = "Обновление коэффициентов";
            ((System.ComponentModel.ISupportInitialize)(this.dgvRanges)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}