using System;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class MainMenuForm : Form
    {
        public MainMenuForm()
        {
            InitializeComponent();
            ConfigureForm();
            AddUpdateCoefButton();
        }

        private void ConfigureForm()
        {
            this.Text = "Прогноз потребления электроэнергии";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Size = new System.Drawing.Size(400, 300); // Увеличили высоту
        }

        private void AddUpdateCoefButton()
        {
            // Кнопка для обновления коэффициентов
            var btnUpdateCoef = new Button
            {
                Text = "Обновить коэффициенты",
                Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular),
                Location = new Point(50, 220),
                Size = new Size(284, 40),
                TabIndex = 4,
                BackColor = Color.LightBlue
            };

            btnUpdateCoef.Click += BtnUpdateCoef_Click;
            this.Controls.Add(btnUpdateCoef);
        }

        private void btnTemperatureDependency_Click(object sender, EventArgs e)
        {
            // Открываем обновленную форму для расчета с БД
            InitialFormDb initialFormDb = new InitialFormDb();
            initialFormDb.Show();
            this.Hide(); // Скрываем главное меню
            initialFormDb.FormClosed += (s, args) => this.Show(); // Показываем меню при закрытии формы
        }

        private void btnConsumptionForecast_Click(object sender, EventArgs e)
        {
            // Открываем форму для расчета прогноза потребления
            InitialFormStatic initialFormStatic = new InitialFormStatic();
            initialFormStatic.Show();
            this.Hide(); // Скрываем главное меню
            initialFormStatic.FormClosed += (s, args) => this.Show(); // Показываем меню при закрытии формы
        }

        private void BtnUpdateCoef_Click(object sender, EventArgs e)
        {
            // Открываем форму для обновления коэффициентов
            UpdateCoefForm updateCoefForm = new UpdateCoefForm();
            updateCoefForm.ShowDialog(); // Модальное окно
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void lblTitle_Click(object sender, EventArgs e)
        {

        }
    }
}