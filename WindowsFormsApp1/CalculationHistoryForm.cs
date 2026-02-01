using ClassLibrary1.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WindowsFormsApp1.Services;

namespace WindowsFormsApp1
{
    public partial class CalculationHistoryForm : Form
    {
        private DatabaseService _databaseService;
        private Dictionary<Panel, bool> _expandedPanels = new Dictionary<Panel, bool>();

        public CalculationHistoryForm()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
        }

        private void CalculationHistoryForm_Load(object sender, EventArgs e)
        {
            LoadHistoryData();
        }

        private void LoadHistoryData()
        {
            try
            {
                // Загружаем данные из базы
                var historyItems = _databaseService.GetHistoryData();
                DisplayHistory(historyItems);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayHistory(List<HistoryItem> historyItems)
        {
            panelHistory.Controls.Clear();
            _expandedPanels.Clear();

            int yPosition = 10;

            foreach (var item in historyItems)
            {
                // Создаем панель для одного элемента истории
                var itemPanel = CreateHistoryItemPanel(item, yPosition);
                panelHistory.Controls.Add(itemPanel);

                yPosition += itemPanel.Height + 10;
            }
        }

        private Panel CreateHistoryItemPanel(HistoryItem item, int yPosition)
        {
            // Определяем высоту панели в зависимости от типа
            int panelHeight = 50; // базовая высота

            if (item.IsStaticAnalysis)
                panelHeight = 70; // больше места для статических расчетов
            else if (item.TypeName.Contains("и электроэнергии"))
                panelHeight = 60; // средняя для комбинированных

            // Основная панель элемента
            var mainPanel = new Panel
            {
                Location = new Point(10, yPosition),
                Width = panelHistory.Width - 40,
                Height = panelHeight,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = GetItemColor(item),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Tag = item
            };

            // Кнопка раскрытия
            var expandButton = new Button
            {
                Text = "▼",
                Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Bold),
                Size = new Size(30, 30),
                Location = new Point(mainPanel.Width - 40, 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Tag = mainPanel
            };
            expandButton.Click += ExpandButton_Click;
            mainPanel.Controls.Add(expandButton);

            // Метки с основной информацией
            var typeLabel = new Label
            {
                Text = item.TypeName,
                Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };
            mainPanel.Controls.Add(typeLabel);

            var dateLabel = new Label
            {
                Text = $"Дата расчета: {item.CalculationDate:dd.MM.yyyy HH:mm}",
                Font = new Font("Microsoft Sans Serif", 9F),
                Location = new Point(10, 30),
                AutoSize = true
            };
            mainPanel.Controls.Add(dateLabel);

            // Для статических расчетов показываем количество периодов
            if (item.IsStaticAnalysis && item.PeriodDetails != null)
            {
                var periodsLabel = new Label
                {
                    Text = $"Периодов: {item.PeriodDetails.Count}",
                    Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Italic),
                    Location = new Point(10, 50),
                    AutoSize = true
                };
                mainPanel.Controls.Add(periodsLabel);
            }

            // Панель с деталями (скрыта по умолчанию)
            var detailsPanel = CreateDetailsPanel(item, mainPanel.Width);
            mainPanel.Controls.Add(detailsPanel);

            _expandedPanels.Add(mainPanel, false);

            return mainPanel;
        }

        private Color GetItemColor(HistoryItem item)
        {
            if (item.IsStaticAnalysis)
                return Color.Lavender;
            else if (item.TypeName.Contains("мощности и электроэнергии"))
                return Color.LightCyan;
            else if (item.TypeName.Contains("мощности"))
                return Color.LightYellow;
            else if (item.TypeName.Contains("электроэнергии"))
                return Color.LightGreen;
            else
                return SystemColors.ControlLight;
        }

        private Panel CreateDetailsPanel(HistoryItem item, int width)
        {
            var detailsPanel = new Panel
            {
                Location = new Point(10, item.IsStaticAnalysis ? 75 : 55),
                Width = width - 20,
                BackColor = SystemColors.Control,
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false
            };

            int yPos = 10;

            // Отображаем данные в зависимости от типа расчета
            if (item.TargetDate.HasValue && item.TargetDate.Value != default)
            {
                AddDetailLabel(detailsPanel, $"Дата, к которой выполняется расчет: {item.TargetDate.Value:dd.MM.yyyy}", ref yPos);
            }

            if (item.TOriginal.HasValue)
            {
                AddDetailLabel(detailsPanel, $"Исходная температура: {item.TOriginal.Value:F1}°C", ref yPos);
            }

            if (item.TResult.HasValue)
            {
                AddDetailLabel(detailsPanel, $"Температура, к которой приводится: {item.TResult.Value:F1}°C", ref yPos);
            }

            // Для прогнозов по мощности
            if (item.TypeName.Contains("мощности") && !item.TypeName.Contains("электроэнергии"))
            {
                if (item.POriginal.HasValue)
                {
                    AddDetailLabel(detailsPanel, $"Исходная мощность: {item.POriginal.Value:F2} МВт", ref yPos);
                }
                if (item.PResult.HasValue)
                {
                    AddDetailLabel(detailsPanel, $"Мощность, к которой приводится: {item.PResult.Value:F2} МВт", ref yPos);
                }
            }

            // Для прогнозов по электроэнергии
            if (item.TypeName.Contains("электроэнергии") && !item.TypeName.Contains("мощности"))
            {
                if (item.EOriginal.HasValue)
                {
                    AddDetailLabel(detailsPanel, $"Исходная электроэнергия: {item.EOriginal.Value:F2} млн кВт·ч", ref yPos);
                }
                if (item.EResult.HasValue)
                {
                    AddDetailLabel(detailsPanel, $"Электроэнергия, к которой приводится: {item.EResult.Value:F2} млн кВт·ч", ref yPos);
                }
            }

            // Для комбинированных прогнозов
            if (item.TypeName.Contains("мощности и электроэнергии"))
            {
                if (item.POriginal.HasValue)
                {
                    AddDetailLabel(detailsPanel, $"Исходная мощность: {item.POriginal.Value:F2} МВт", ref yPos);
                }
                if (item.PResult.HasValue)
                {
                    AddDetailLabel(detailsPanel, $"Мощность, к которой приводится: {item.PResult.Value:F2} МВт", ref yPos);
                }
                if (item.EOriginal.HasValue)
                {
                    AddDetailLabel(detailsPanel, $"Исходная электроэнергия: {item.EOriginal.Value:F2} млн кВт·ч", ref yPos);
                }
                if (item.EResult.HasValue)
                {
                    AddDetailLabel(detailsPanel, $"Электроэнергия, к которой приводится: {item.EResult.Value:F2} млн кВт·ч", ref yPos);
                }
            }

            // Для статических расчетов показываем периоды с регрессиями
            if (item.IsStaticAnalysis && item.PeriodDetails != null && item.PeriodDetails.Count > 0)
            {
                AddDetailLabel(detailsPanel, "Результаты регрессий по периодам:", ref yPos);
                yPos += 5; // Отступ

                foreach (var period in item.PeriodDetails)
                {
                    AddDetailLabel(detailsPanel, $"• Период: {period.PeriodName} ({period.PeriodYear})", ref yPos);

                    if (period.KLinear.HasValue)
                        AddDetailLabel(detailsPanel, $"  Коэффициент наклона (k): {period.KLinear.Value:F4}", ref yPos);

                    if (period.BLinear.HasValue)
                        AddDetailLabel(detailsPanel, $"  Свободный член (b): {period.BLinear.Value:F2}", ref yPos);

                    if (period.LExponential.HasValue)
                        AddDetailLabel(detailsPanel, $"  Интенсивность (L): {period.LExponential.Value:F4}", ref yPos);

                    yPos += 5; // Отступ между периодами
                }
            }

            detailsPanel.Height = yPos + 10;
            return detailsPanel;
        }

        private void AddDetailLabel(Panel panel, string text, ref int yPos)
        {
            var label = new Label
            {
                Text = text,
                Font = new Font("Microsoft Sans Serif", 9F),
                Location = new Point(10, yPos),
                AutoSize = true,
                MaximumSize = new Size(panel.Width - 20, 0)
            };
            panel.Controls.Add(label);
            yPos += label.Height + 5;
        }

        private void ExpandButton_Click(object sender, EventArgs e)
        {
            var button = sender as Button;
            var mainPanel = button.Tag as Panel;

            if (mainPanel != null)
            {
                bool isExpanded = _expandedPanels[mainPanel];
                isExpanded = !isExpanded;

                // Находим панель деталей (она всегда последняя в Controls)
                var detailsPanel = mainPanel.Controls[mainPanel.Controls.Count - 1] as Panel;

                if (detailsPanel != null)
                {
                    button.Text = isExpanded ? "▲" : "▼";
                    detailsPanel.Visible = isExpanded;
                    mainPanel.Height = isExpanded ? 55 + detailsPanel.Height + 10 : 50;

                    // Обновляем положение всех последующих элементов
                    UpdateItemsPosition(mainPanel);

                    _expandedPanels[mainPanel] = isExpanded;
                }
            }
        }

        private void UpdateItemsPosition(Panel changedPanel)
        {
            int startIndex = panelHistory.Controls.IndexOf(changedPanel) + 1;
            int yPosition = changedPanel.Location.Y + changedPanel.Height + 10;

            for (int i = startIndex; i < panelHistory.Controls.Count; i++)
            {
                var panel = panelHistory.Controls[i] as Panel;
                if (panel != null)
                {
                    panel.Location = new Point(panel.Location.X, yPosition);
                    yPosition += panel.Height + 10;
                }
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadHistoryData();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}