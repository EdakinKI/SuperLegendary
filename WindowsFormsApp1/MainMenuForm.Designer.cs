namespace WindowsFormsApp1
{
    partial class MainMenuForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Button btnForecast;
        private System.Windows.Forms.Button btnStatic;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnUpdateCoef;

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
            this.btnForecast = new System.Windows.Forms.Button();
            this.btnStatic = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.lblTitle = new System.Windows.Forms.Label();
            this.btnUpdateCoef = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnForecast
            // 
            this.btnForecast.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnForecast.Location = new System.Drawing.Point(50, 70);
            this.btnForecast.Name = "btnForecast";
            this.btnForecast.Size = new System.Drawing.Size(284, 40);
            this.btnForecast.TabIndex = 1;
            this.btnForecast.Text = "Расчет прогноза";
            this.btnForecast.UseVisualStyleBackColor = true;
            this.btnForecast.Click += new System.EventHandler(this.btnTemperatureDependency_Click);
            // 
            // btnStatic
            // 
            this.btnStatic.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnStatic.Location = new System.Drawing.Point(50, 120);
            this.btnStatic.Name = "btnStatic";
            this.btnStatic.Size = new System.Drawing.Size(284, 40);
            this.btnStatic.TabIndex = 2;
            this.btnStatic.Text = "Расчет зависимостей";
            this.btnStatic.UseVisualStyleBackColor = true;
            this.btnStatic.Click += new System.EventHandler(this.btnConsumptionForecast_Click);
            // 
            // btnExit
            // 
            this.btnExit.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnExit.Location = new System.Drawing.Point(50, 248);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(284, 40);
            this.btnExit.TabIndex = 3;
            this.btnExit.Text = "Выход";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // lblTitle
            // 
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(384, 50);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Выберите тип расчета";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblTitle.Click += new System.EventHandler(this.lblTitle_Click);
            // 
            // btnUpdateCoef
            // 
            this.btnUpdateCoef.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnUpdateCoef.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnUpdateCoef.Location = new System.Drawing.Point(50, 174);
            this.btnUpdateCoef.Name = "btnUpdateCoef";
            this.btnUpdateCoef.Size = new System.Drawing.Size(284, 40);
            this.btnUpdateCoef.TabIndex = 4;
            this.btnUpdateCoef.Text = "Обновить коэффициенты";
            this.btnUpdateCoef.UseVisualStyleBackColor = false;
            this.btnUpdateCoef.Click += new System.EventHandler(this.BtnUpdateCoef_Click);
            // 
            // MainMenuForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 310);
            this.Controls.Add(this.btnUpdateCoef);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnStatic);
            this.Controls.Add(this.btnForecast);
            this.Controls.Add(this.lblTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainMenuForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Прогноз потребления электроэнергии";
            this.ResumeLayout(false);

        }
    }
}