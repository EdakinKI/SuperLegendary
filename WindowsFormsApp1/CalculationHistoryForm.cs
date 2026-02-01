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
        private List<HistoryItem> _allHistoryItems = new List<HistoryItem>();

        // Фильтры
        private string _selectedType = "Все типы";
        private DateTime? _dateFrom = null;
        private DateTime? _dateTo = null;

        public CalculationHistoryForm()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            InitializeFilters();
        }

        private void InitializeFilters()
        {
            // Устанавливаем даты по умолчанию (последние 30 дней)
            dtpFilterDateFrom.Value = DateTime.Now.AddDays(-30);
            dtpFilterDateTo.Value = DateTime.Now;

            // Выбираем "Все типы" по умолчанию
            cmbFilterType.SelectedIndex = 0;

            // Активируем/деактивируем DateTimePicker в зависимости от чекбокса
            UpdateDateFiltersState();
        }

        private void CalculationHistoryForm_Load(object sender, EventArgs e)
        {
            LoadAllHistoryData();
        }

        private void LoadAllHistoryData()
        {
            try
            {
                // Загружаем все данные из базы
                _allHistoryItems = _databaseService.GetHistoryData();

                // Применяем текущие фильтры
                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyFilters()
        {
            try
            {
                // Начинаем со всех данных
                var filteredItems = _allHistoryItems.AsEnumerable();

                // Фильтр по типу
                if (_selectedType != "Все типы")
                {
                    filteredItems = filteredItems.Where(item => item.TypeName == _selectedType);
                }

                // Фильтр по дате (если не выбрано "Все даты")
                if (_dateFrom.HasValue)
                {
                    filteredItems = filteredItems.Where(item => item.CalculationDate >= _dateFrom.Value);
                }

                if (_dateTo.HasValue)
                {
                    // Добавляем 1 день чтобы включить весь день
                    var dateToWithTime = _dateTo.Value.AddDays(1).AddSeconds(-1);
                    filteredItems = filteredItems.Where(item => item.CalculationDate <= dateToWithTime);
                }

                // Отображаем отфильтрованные данные
                DisplayHistory(filteredItems.ToList());

                // Обновляем заголовок
                UpdateTitle();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка применения фильтров: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateTitle()
        {
            int totalCount = _allHistoryItems.Count;
            int filteredCount = panelHistory.Controls.Count; // количество отображенных элементов

            if (totalCount == filteredCount)
            {
                lblTitle.Text = $"История расчетов ({totalCount})";
            }
            else
            {
                lblTitle.Text = $"История расчетов ({filteredCount} из {totalCount})";
            }
        }

        private void DisplayHistory(List<HistoryItem> historyItems)
        {
            panelHistory.Controls.Clear();
            _expandedPanels.Clear();

            if (!historyItems.Any())
            {
                // Показываем сообщение, если нет данных
                var noDataLabel = new Label
                {
                    Text = "Нет данных, соответствующих выбранным фильтрам",
                    Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Italic),
                    ForeColor = Color.Gray,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill
                };
                panelHistory.Controls.Add(noDataLabel);
                return;
            }

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
                panelHeight = 70;
            else if (item.TypeName.Contains("и электроэнергии"))
                panelHeight = 60;

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
            detailsPanel.Location = new Point(10, mainPanel.Height - 10);
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
                    mainPanel.Height = isExpanded ? mainPanel.Height + detailsPanel.Height + 10 : mainPanel.Height - detailsPanel.Height;

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

        // Обработчики фильтров
        private void btnApplyFilters_Click(object sender, EventArgs e)
        {
            // Сохраняем выбранные фильтры
            _selectedType = cmbFilterType.SelectedItem?.ToString() ?? "Все типы";

            if (chkShowAll.Checked)
            {
                _dateFrom = null;
                _dateTo = null;
            }
            else
            {
                _dateFrom = dtpFilterDateFrom.Value.Date;
                _dateTo = dtpFilterDateTo.Value.Date;

                // Проверяем, что дата "от" не больше даты "до"
                if (_dateFrom > _dateTo)
                {
                    MessageBox.Show("Дата 'От' не может быть больше даты 'До'", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // Применяем фильтры
            ApplyFilters();
        }

        private void btnClearFilters_Click(object sender, EventArgs e)
        {
            // Сбрасываем все фильтры
            cmbFilterType.SelectedIndex = 0;
            chkShowAll.Checked = true;
            dtpFilterDateFrom.Value = DateTime.Now.AddDays(-30);
            dtpFilterDateTo.Value = DateTime.Now;

            // Обновляем состояние
            UpdateDateFiltersState();

            // Применяем фильтры (все данные)
            _selectedType = "Все типы";
            _dateFrom = null;
            _dateTo = null;
            ApplyFilters();
        }

        private void chkShowAll_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDateFiltersState();
        }

        private void UpdateDateFiltersState()
        {
            bool enableDates = !chkShowAll.Checked;
            dtpFilterDateFrom.Enabled = enableDates;
            dtpFilterDateTo.Enabled = enableDates;
            lblFilterDateFrom.Enabled = enableDates;
            lblFilterDateTo.Enabled = enableDates;
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            // Перезагружаем все данные из базы
            LoadAllHistoryData();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}