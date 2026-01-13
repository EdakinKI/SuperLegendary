using System;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class ProcessingForm : Form
    {
        private Label lblStatus;
        private ProgressBar progressBar;
        private Label lblPercent;

        public ProcessingForm()
        {
            InitializeComponent(); // Этот метод определен в ProcessingForm.Designer.cs
            ConfigureForm();
        }

        private void ConfigureForm()
        {
            this.Text = "Обработка преобразованием Фурье";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Size = new System.Drawing.Size(450, 150);
            this.BackColor = Color.White;

            // Заголовок
            var lblTitle = new Label
            {
                Location = new Point(20, 15),
                Size = new Size(400, 25),
                Text = "Применение преобразования Фурье",
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.DarkBlue
            };

            // Статус
            lblStatus = new Label
            {
                Location = new Point(20, 45),
                Size = new Size(400, 20),
                Text = "Подготовка данных..."
            };

            // Прогресс бар
            progressBar = new ProgressBar
            {
                Location = new Point(20, 70),
                Size = new Size(350, 25),
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                Style = ProgressBarStyle.Continuous
            };

            // Проценты
            lblPercent = new Label
            {
                Location = new Point(375, 70),
                Size = new Size(45, 25),
                Text = "0%",
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };

            this.Controls.Add(lblTitle);
            this.Controls.Add(lblStatus);
            this.Controls.Add(progressBar);
            this.Controls.Add(lblPercent);
        }

        public void UpdateProgress(string message, int progress)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateProgress(message, progress)));
                return;
            }

            lblStatus.Text = message;
            progressBar.Value = Math.Max(0, Math.Min(progress, 100));
            lblPercent.Text = $"{progressBar.Value}%";

            // Принудительное обновление
            progressBar.Refresh();
            lblPercent.Refresh();
            Application.DoEvents();
        }
    }
}