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
            // Основная панель элемента
            var mainPanel = new Panel
            {
                Location = new Point(10, yPosition),
                Width = panelHistory.Width - 40,
                Height = item.IsStaticAnalysis ? 80 : 50, // Больше места для статических расчетов
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = item.IsStaticAnalysis ? Color.Lavender : SystemColors.ControlLight,
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

            // Для статических расчетов показываем сводку
            if (item.IsStaticAnalysis)
            {
                var summaryLabel = new Label
                {
                    Text = item.CalculationName ?? "Статический анализ",
                    Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Italic),
                    Location = new Point(10, 50),
                    AutoSize = true,
                    MaximumSize = new Size(mainPanel.Width - 50, 0)
                };
                mainPanel.Controls.Add(summaryLabel);
            }

            // Панель с деталями (скрыта по умолчанию)
            var detailsPanel = CreateDetailsPanel(item, mainPanel.Width);
            mainPanel.Controls.Add(detailsPanel);

            _expandedPanels.Add(mainPanel, false);

            return mainPanel;
        }

        private Panel CreateDetailsPanel(HistoryItem item, int width)
        {
            var detailsPanel = new Panel
            {
                Location = new Point(10, item.IsStaticAnalysis ? 85 : 55),
                Width = width - 20,
                BackColor = SystemColors.Control,
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false
            };

            int yPos = 10;

            // Для статических расчетов показываем сводку регрессий
            if (item.IsStaticAnalysis && !string.IsNullOrEmpty(item.RegressionSummary))
            {
                AddDetailLabel(detailsPanel, "Результаты регрессий:", ref yPos);

                // Добавляем пустую строку для отступа
                yPos += 5;

                // Разбиваем сводку по строкам
                var regressionLines = item.RegressionSummary.Split('\n');
                foreach (var line in regressionLines)
                {
                    AddDetailLabel(detailsPanel, $"• {line}", ref yPos);
                }

                yPos += 10; // Дополнительный отступ
            }

            if (item.TargetDate.HasValue && item.TargetDate.Value != default)
            {
                AddDetailLabel(detailsPanel, $"Дата, к которой выполняется расчет: {item.TargetDate.Value:dd.MM.yyyy}", ref yPos);
            }

            // Остальные детали без изменений...
            // [остальной код остается прежним]

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