using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using ClassLibrary1; // Добавьте ссылку на ClassLibrary1

namespace WindowsFormsApp1
{
    public partial class ProcessingFormWithCancel : Form, IProgressReporter
    {
        private Label lblStatus;
        private ProgressBar progressBar;
        private Label lblPercent;
        private Button btnCancel;
        private bool _isCancelled = false;
        private bool _isProcessing = false;

        public bool IsCancelled => _isCancelled;

        public ProcessingFormWithCancel()
        {
            InitializeComponent();
            ConfigureForm();
        }

        private void ConfigureForm()
        {
            this.Text = "Обработка данных";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Size = new System.Drawing.Size(500, 180);
            this.BackColor = Color.White;

            // Запрещаем закрытие формы крестиком
            this.ControlBox = false;

            // Заголовок
            var lblTitle = new Label
            {
                Location = new Point(20, 15),
                Size = new Size(460, 25),
                Text = "Обработка данных",
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.DarkBlue
            };

            // Статус
            lblStatus = new Label
            {
                Location = new Point(20, 45),
                Size = new Size(460, 20),
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
                Size = new Size(75, 25),
                Text = "0%",
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };

            // Кнопка отмены
            btnCancel = new Button
            {
                Location = new Point(200, 105),
                Size = new Size(100, 30),
                Text = "Прекратить",
                BackColor = Color.LightCoral,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            btnCancel.Click += (s, e) => CancelProcessing();

            this.Controls.Add(lblTitle);
            this.Controls.Add(lblStatus);
            this.Controls.Add(progressBar);
            this.Controls.Add(lblPercent);
            this.Controls.Add(btnCancel);
        }

        // Реализация интерфейса IProgressReporter
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

        private void CancelProcessing()
        {
            _isCancelled = true;
            btnCancel.Enabled = false;
            btnCancel.Text = "Отменяется...";
            lblStatus.Text = "Отмена операции...";
        }

        public void SetProcessing(bool isProcessing)
        {
            _isProcessing = isProcessing;
            if (!isProcessing)
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() => this.Close()));
                }
                else
                {
                    this.Close();
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_isProcessing && !_isCancelled)
            {
                e.Cancel = true; // Запрещаем закрытие, если идет обработка
            }
            base.OnFormClosing(e);
        }
    }
}