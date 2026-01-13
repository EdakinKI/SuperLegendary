namespace WindowsFormsApp1
{
    partial class RegressionFormDB
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageChart;
        private System.Windows.Forms.TabPage tabPageCoefficients;
        private System.Windows.Forms.TabPage tabPageFormulas;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartRegression;
        private System.Windows.Forms.DataGridView dgvCoefficients;
        private System.Windows.Forms.RichTextBox rtbFormulas;

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
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageChart = new System.Windows.Forms.TabPage();
            this.chartRegression = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.tabPageCoefficients = new System.Windows.Forms.TabPage();
            this.dgvCoefficients = new System.Windows.Forms.DataGridView();
            this.tabPageFormulas = new System.Windows.Forms.TabPage();
            this.rtbFormulas = new System.Windows.Forms.RichTextBox();
            this.tabControl.SuspendLayout();
            this.tabPageChart.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartRegression)).BeginInit();
            this.tabPageCoefficients.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCoefficients)).BeginInit();
            this.tabPageFormulas.SuspendLayout();
            this.SuspendLayout();

            // tabControl
            this.tabControl.Controls.Add(this.tabPageChart);
            this.tabControl.Controls.Add(this.tabPageCoefficients);
            this.tabControl.Controls.Add(this.tabPageFormulas);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(1300, 800);
            this.tabControl.TabIndex = 0;

            // tabPageChart
            this.tabPageChart.Controls.Add(this.chartRegression);
            this.tabPageChart.Location = new System.Drawing.Point(4, 25);
            this.tabPageChart.Name = "tabPageChart";
            this.tabPageChart.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageChart.Size = new System.Drawing.Size(1292, 771);
            this.tabPageChart.TabIndex = 0;
            this.tabPageChart.Text = "Графики регрессий";
            this.tabPageChart.UseVisualStyleBackColor = true;

            // chartRegression
            this.chartRegression.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartRegression.Location = new System.Drawing.Point(3, 3);
            this.chartRegression.Name = "chartRegression";
            this.chartRegression.Size = new System.Drawing.Size(1286, 765);
            this.chartRegression.TabIndex = 0;
            this.chartRegression.Text = "chart1";

            // tabPageCoefficients
            this.tabPageCoefficients.Controls.Add(this.dgvCoefficients);
            this.tabPageCoefficients.Location = new System.Drawing.Point(4, 25);
            this.tabPageCoefficients.Name = "tabPageCoefficients";
            this.tabPageCoefficients.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageCoefficients.Size = new System.Drawing.Size(1292, 771);
            this.tabPageCoefficients.TabIndex = 1;
            this.tabPageCoefficients.Text = "Коэффициенты регрессий";
            this.tabPageCoefficients.UseVisualStyleBackColor = true;

            // dgvCoefficients
            this.dgvCoefficients.AllowUserToAddRows = false;
            this.dgvCoefficients.AllowUserToDeleteRows = false;
            this.dgvCoefficients.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCoefficients.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvCoefficients.Location = new System.Drawing.Point(3, 3);
            this.dgvCoefficients.Name = "dgvCoefficients";
            this.dgvCoefficients.ReadOnly = true;
            this.dgvCoefficients.RowHeadersVisible = false;
            this.dgvCoefficients.RowTemplate.Height = 24;
            this.dgvCoefficients.Size = new System.Drawing.Size(1286, 765);
            this.dgvCoefficients.TabIndex = 0;

            // tabPageFormulas
            this.tabPageFormulas.Controls.Add(this.rtbFormulas);
            this.tabPageFormulas.Location = new System.Drawing.Point(4, 25);
            this.tabPageFormulas.Name = "tabPageFormulas";
            this.tabPageFormulas.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageFormulas.Size = new System.Drawing.Size(1292, 771);
            this.tabPageFormulas.TabIndex = 2;
            this.tabPageFormulas.Text = "Формулы и объяснения";
            this.tabPageFormulas.UseVisualStyleBackColor = true;

            // rtbFormulas
            this.rtbFormulas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbFormulas.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.rtbFormulas.Location = new System.Drawing.Point(3, 3);
            this.rtbFormulas.Name = "rtbFormulas";
            this.rtbFormulas.ReadOnly = true;
            this.rtbFormulas.Size = new System.Drawing.Size(1286, 765);
            this.rtbFormulas.TabIndex = 0;
            this.rtbFormulas.Text = "";

            // RegressionForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1300, 800);
            this.Controls.Add(this.tabControl);
            this.MinimumSize = new System.Drawing.Size(1000, 700);
            this.Name = "RegressionForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Результаты аппроксимации данных";
            this.tabControl.ResumeLayout(false);
            this.tabPageChart.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chartRegression)).EndInit();
            this.tabPageCoefficients.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvCoefficients)).EndInit();
            this.tabPageFormulas.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}