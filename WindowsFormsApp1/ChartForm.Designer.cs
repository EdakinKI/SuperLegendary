namespace WindowsFormsApp1
{
    partial class ChartForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartDependency;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblStats;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.Button btnApproximate;
        private System.Windows.Forms.Button btnShowRegression;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.Button btnShowFormulas;

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
            this.panelMain = new System.Windows.Forms.Panel();
            this.btnShowFormulas = new System.Windows.Forms.Button();
            this.lblInfo = new System.Windows.Forms.Label();
            this.btnShowRegression = new System.Windows.Forms.Button();
            this.btnApproximate = new System.Windows.Forms.Button();
            this.lblStats = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.chartDependency = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.lblTitle = new System.Windows.Forms.Label();
            this.panelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartDependency)).BeginInit();
            this.SuspendLayout();

            // panelMain
            this.panelMain.Controls.Add(this.btnShowFormulas);
            this.panelMain.Controls.Add(this.lblInfo);
            this.panelMain.Controls.Add(this.btnShowRegression);
            this.panelMain.Controls.Add(this.btnApproximate);
            this.panelMain.Controls.Add(this.lblStats);
            this.panelMain.Controls.Add(this.btnClose);
            this.panelMain.Controls.Add(this.chartDependency);
            this.panelMain.Controls.Add(this.lblTitle);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 0);
            this.panelMain.Name = "panelMain";
            this.panelMain.Padding = new System.Windows.Forms.Padding(10);
            this.panelMain.Size = new System.Drawing.Size(1200, 800);
            this.panelMain.TabIndex = 0;

            // lblTitle
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblTitle.Location = new System.Drawing.Point(10, 10);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(1180, 50);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "График зависимости мощности от температуры";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // chartDependency
            this.chartDependency.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chartDependency.Location = new System.Drawing.Point(10, 70);
            this.chartDependency.Name = "chartDependency";
            this.chartDependency.Size = new System.Drawing.Size(1180, 450);
            this.chartDependency.TabIndex = 1;
            this.chartDependency.Text = "chart1";

            // btnClose
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(1070, 720);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(120, 35);
            this.btnClose.TabIndex = 3;
            this.btnClose.Text = "Закрыть";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);

            // lblStats
            this.lblStats.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStats.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblStats.Location = new System.Drawing.Point(10, 530);
            this.lblStats.Name = "lblStats";
            this.lblStats.Size = new System.Drawing.Size(850, 80);
            this.lblStats.TabIndex = 4;
            this.lblStats.Text = "Статистика";

            // btnApproximate
            this.btnApproximate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnApproximate.BackColor = System.Drawing.Color.LightGreen;
            this.btnApproximate.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnApproximate.Location = new System.Drawing.Point(10, 620);
            this.btnApproximate.Name = "btnApproximate";
            this.btnApproximate.Size = new System.Drawing.Size(180, 35);
            this.btnApproximate.TabIndex = 5;
            this.btnApproximate.Text = "Аппроксимировать данные";
            this.btnApproximate.UseVisualStyleBackColor = false;
            this.btnApproximate.Click += new System.EventHandler(this.BtnApproximate_Click);

            // btnShowRegression
            this.btnShowRegression.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnShowRegression.BackColor = System.Drawing.Color.LightBlue;
            this.btnShowRegression.Enabled = false;
            this.btnShowRegression.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnShowRegression.Location = new System.Drawing.Point(200, 620);
            this.btnShowRegression.Name = "btnShowRegression";
            this.btnShowRegression.Size = new System.Drawing.Size(150, 35);
            this.btnShowRegression.TabIndex = 6;
            this.btnShowRegression.Text = "Показать регрессии";
            this.btnShowRegression.UseVisualStyleBackColor = false;
            this.btnShowRegression.Click += new System.EventHandler(this.BtnShowRegression_Click);

            // lblInfo
            this.lblInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblInfo.ForeColor = System.Drawing.Color.DarkBlue;
            this.lblInfo.Location = new System.Drawing.Point(10, 660);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(600, 50);
            this.lblInfo.TabIndex = 7;
            this.lblInfo.Text = "Сначала загрузите данные, затем нажмите \'Аппроксимировать\'";

            // btnShowFormulas
            this.btnShowFormulas.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnShowFormulas.BackColor = System.Drawing.Color.LightGoldenrodYellow;
            this.btnShowFormulas.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnShowFormulas.Location = new System.Drawing.Point(360, 620);
            this.btnShowFormulas.Name = "btnShowFormulas";
            this.btnShowFormulas.Size = new System.Drawing.Size(150, 35);
            this.btnShowFormulas.TabIndex = 8;
            this.btnShowFormulas.Text = "Показать формулы";
            this.btnShowFormulas.UseVisualStyleBackColor = false;
            this.btnShowFormulas.Click += new System.EventHandler(this.BtnShowFormulas_Click);

            // ChartForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 800);
            this.Controls.Add(this.panelMain);
            this.MinimumSize = new System.Drawing.Size(1000, 700);
            this.Name = "ChartForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "График зависимости мощности от температуры";
            this.panelMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chartDependency)).EndInit();
            this.ResumeLayout(false);
        }
    }
}