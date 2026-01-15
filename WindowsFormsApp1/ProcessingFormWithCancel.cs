using System;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class ProcessingFormWithCancel : Form
    {
        public event EventHandler CancelRequested;

        private Label lblStatus;
        private ProgressBar progressBar;
        private Label lblPercent;
        private Button btnCancel;
        private bool cancellationRequested = false;

        public ProcessingFormWithCancel()
        {
            InitializeForm();
        }

        private void InitializeForm()
        {
            this.Text = "Обработка данных";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Size = new Size(500, 180);
            this.BackColor = Color.White;

            // Заголовок
            var lblTitle = new Label
            {
                Location = new Point(20, 15),
                Size = new Size(450, 25),
                Text = "Обработка данных",
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.DarkBlue
            };

            // Статус
            lblStatus = new Label
            {
                Location = new Point(20, 45),
                Size = new Size(450, 20),
                Text = "Подготовка данных..."
            };

            // Прогресс бар
            progressBar = new ProgressBar
            {
                Location = new Point(20, 70),
                Size = new Size(380, 25),
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                Style = ProgressBarStyle.Continuous
            };

            // Проценты
            lblPercent = new Label
            {
                Location = new Point(405, 70),
                Size = new Size(45, 25),
                Text = "0%",
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };

            // Кнопка отмены
            btnCancel = new Button
            {
                Location = new Point(200, 105),
                Size = new Size(100, 30),
                Text = "Прервать",
                BackColor = Color.LightCoral,
                Enabled = true
            };

            btnCancel.Click += BtnCancel_Click;

            this.Controls.Add(lblTitle);
            this.Controls.Add(lblStatus);
            this.Controls.Add(progressBar);
            this.Controls.Add(lblPercent);
            this.Controls.Add(btnCancel);
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (!cancellationRequested)
            {
                cancellationRequested = true;
                btnCancel.Enabled = false;
                btnCancel.Text = "Прерывание...";
                CancelRequested?.Invoke(this, EventArgs.Empty);
            }
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

            btnCancel.Enabled = !cancellationRequested;

            if (progress >= 100)
            {
                btnCancel.Visible = false;
                lblStatus.Text = "Обработка завершена!";
            }

            progressBar.Refresh();
            lblPercent.Refresh();
            Application.DoEvents();
        }

        public void RequestCancel()
        {
            BtnCancel_Click(null, EventArgs.Empty);
        }

        public bool IsCancellationRequested => cancellationRequested;
    }
}