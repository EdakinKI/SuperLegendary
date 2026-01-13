using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class DataCheckForm : Form
    {
        private List<HourlyData> matchedData;

        public DataCheckForm(List<HourlyData> data)
        {
            matchedData = data ?? new List<HourlyData>();
            InitializeComponent();
            ConfigureForm();
            LoadData();
        }

        private void ConfigureForm()
        {
            this.Text = "Проверка данных";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Size = new System.Drawing.Size(600, 400);
        }

        private void LoadData()
        {
            dgvCheck.Rows.Clear();
            dgvCheck.Columns.Clear();

            // Настраиваем колонки
            dgvCheck.Columns.Add("DateTime", "Дата и время");
            dgvCheck.Columns.Add("Temperature", "Температура, °C");
            dgvCheck.Columns.Add("Power", "Мощность, МВт");

            dgvCheck.Columns["DateTime"].Width = 200;
            dgvCheck.Columns["Temperature"].Width = 150;
            dgvCheck.Columns["Power"].Width = 150;

            if (matchedData.Count == 0)
            {
                dgvCheck.Rows.Add("Нет данных");
                return;
            }

            // Показываем только 20 записей
            int showCount = Math.Min(20, matchedData.Count);

            for (int i = 0; i < showCount; i++)
            {
                var data = matchedData[i];
                dgvCheck.Rows.Add(
                    data.DateTime.ToString("dd.MM.yyyy HH:mm"),
                    data.Value.ToString("F1"),
                    data.PowerValue?.ToString("F2") ?? "N/A"
                );
            }

            lblStats.Text = $"Всего записей: {matchedData.Count}\n" +
                           $"Первые {showCount} записей";
        }
    }
}