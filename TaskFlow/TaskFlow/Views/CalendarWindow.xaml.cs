using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using TaskFlow.Models;

namespace TaskFlow.Views
{
    public partial class CalendarWindow : Window
    {
        private readonly ObservableCollection<TaskItem> _allTasks;

        public ObservableCollection<TaskItem> SelectedTasks { get; set; }

        private DateTime CurrentMonth;
        private DateTime SelectedDate;
        private string _calendarMode = "Месяц";

        public CalendarWindow(ObservableCollection<TaskItem> tasks)
        {
            InitializeComponent();

            _allTasks = tasks;

            SelectedTasks = new ObservableCollection<TaskItem>();

            DataContext = this;

            CurrentMonth = new DateTime(
                DateTime.Today.Year,
                DateTime.Today.Month,
                1
            );

            SelectedDate = DateTime.Today;

            BuildCalendar();

            UpdateSelectedTasks();
        }

        private void CompletedFilter_Click(object sender, RoutedEventArgs e)
        {
            UpdateSelectedTasks();
        }

        // ==========================================
        // ПОСТРОЕНИЕ КАЛЕНДАРЯ
        // ==========================================

        private void BuildCalendar()
        {
            CalendarGrid.Children.Clear();

            CultureInfo culture = new CultureInfo("ru-RU");

            // Определяем количество ячеек и начало периода
            DateTime startDate;
            int cellCount;

            if (_calendarMode == "Месяц")
            {
                startDate = new DateTime(
                    CurrentMonth.Year,
                    CurrentMonth.Month,
                    1);

                int offset = ((int)startDate.DayOfWeek + 6) % 7;

                startDate = startDate.AddDays(-offset);

                cellCount = 42;

                CalendarGrid.Columns = 7;
                CalendarGrid.Rows = 6;

                WeekdayHeader.Visibility = Visibility.Visible;

                MonthTitle.Text = culture.TextInfo.ToTitleCase(
                    CurrentMonth.ToString("MMMM yyyy", culture));
            }
            else if (_calendarMode == "Неделя")
            {
                int offset = ((int)SelectedDate.DayOfWeek + 6) % 7;

                startDate = SelectedDate.Date.AddDays(-offset);

                cellCount = 7;

                CalendarGrid.Columns = 7;
                CalendarGrid.Rows = 1;

                WeekdayHeader.Visibility = Visibility.Visible;

                DateTime endDate = startDate.AddDays(6);

                MonthTitle.Text =
                    $"{startDate:dd.MM.yyyy} — {endDate:dd.MM.yyyy}";
            }
            else
            {
                startDate = SelectedDate.Date;

                cellCount = 1;

                CalendarGrid.Columns = 1;
                CalendarGrid.Rows = 1;

                WeekdayHeader.Visibility = Visibility.Collapsed;

                MonthTitle.Text = culture.TextInfo.ToTitleCase(
                    SelectedDate.ToString("dd MMMM yyyy", culture));
            }

            // Цвета приоритетов
            string[] priorities =
            {"Низкий", "Средний", "Высокий", "Критический"};

            Color[] priorityColors =
            {Color.FromRgb(34, 197, 94), Color.FromRgb(59, 130, 246), Color.FromRgb(249, 115, 22), Color.FromRgb(239, 68, 68)};

            // Создание ячеек календаря
            for (int i = 0; i < cellCount; i++)
            {
                DateTime date = startDate.AddDays(i);

                bool isCurrentMonth =
                    date.Month == CurrentMonth.Month &&
                    date.Year == CurrentMonth.Year;

                var tasksForDate = _allTasks
                    .Where(task =>
                        task.Deadline.HasValue &&
                        task.Deadline.Value.Date == date.Date)
                    .ToList();

                Button dayButton = new Button
                {
                    Margin = new Thickness(3),
                    Padding = new Thickness(4),
                    MinHeight = 50,
                    Tag = date,
                    IsEnabled = _calendarMode != "Месяц" || isCurrentMonth,
                    Cursor = Cursors.Hand,
                    BorderThickness = new Thickness(1),
                    BorderBrush = new SolidColorBrush(
                        Color.FromRgb(226, 232, 240))
                };

                // Оформление выбранной даты
                if (date.Date == SelectedDate.Date)
                {
                    dayButton.Background = new SolidColorBrush(
                        Color.FromRgb(224, 231, 255));

                    dayButton.BorderBrush = new SolidColorBrush(
                        Color.FromRgb(99, 102, 241));
                }
                else
                {
                    dayButton.Background = Brushes.White;
                }

                StackPanel content = new StackPanel
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                // Номер дня
                TextBlock dayText = new TextBlock
                {
                    Text = date.Day.ToString(),
                    FontSize = 16,
                    FontWeight = FontWeights.SemiBold,

                    Foreground = date.Date == DateTime.Today
                        ? new SolidColorBrush(Color.FromRgb(79, 70, 229))
                        : new SolidColorBrush(Color.FromRgb(30, 41, 59)),

                    HorizontalAlignment = HorizontalAlignment.Center
                };

                content.Children.Add(dayText);

                // Количество задач
                if (tasksForDate.Count > 0)
                {
                    TextBlock countText = new TextBlock
                    {
                        Text = $"Задач: {tasksForDate.Count}",
                        FontSize = 10,

                        Foreground = new SolidColorBrush(
                            Color.FromRgb(99, 102, 241)),

                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 3, 0, 2)
                    };

                    content.Children.Add(countText);

                    // Цветные индикаторы приоритетов
                    WrapPanel indicators = new WrapPanel
                    {
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    for (int j = 0; j < priorities.Length; j++)
                    {
                        bool hasPriority = tasksForDate.Any(task =>
                            string.Equals(
                                task.Priority?.ToString(),
                                priorities[j],
                                StringComparison.OrdinalIgnoreCase));

                        if (hasPriority)
                        {
                            Ellipse indicator = new Ellipse
                            {
                                Width = 7,
                                Height = 7,
                                Fill = new SolidColorBrush(priorityColors[j]),
                                Margin = new Thickness(2)
                            };

                            indicators.Children.Add(indicator);
                        }
                    }

                    content.Children.Add(indicators);
                }

                dayButton.Content = content;

                dayButton.Click += CalendarDay_Click;

                CalendarGrid.Children.Add(dayButton);
            }

            UpdateModeButtons();
        }



        // ==========================================
        // ВЫБОР ДАТЫ
        // ==========================================


        private void CalendarDay_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button &&
                button.Tag is DateTime selectedDate)
            {
                SelectedDate = selectedDate;

                CurrentMonth = new DateTime(
                    selectedDate.Year,
                    selectedDate.Month,
                    1);

                BuildCalendar();

                UpdateSelectedTasks();
            }
        }


        // ==========================================
        // ОБНОВЛЕНИЕ СПИСКА ЗАДАЧ
        // ==========================================


        private void UpdateSelectedTasks()
        {
            // Очищаем предыдущий список
            SelectedTasks.Clear();

            // Обновляем заголовок
            SelectedDateText.Text =
                $"Задачи на {SelectedDate:dd.MM.yyyy}";

            // Определяем, нужно ли показывать выполненные задачи
            bool showCompleted = ShowCompletedCheckBox.IsChecked == true;

            // Получаем задачи выбранной даты
            var tasksForDate = _allTasks
                .Where(task =>
                    task.Deadline.HasValue &&
                    task.Deadline.Value.Date == SelectedDate.Date &&
                    (showCompleted || !task.IsCompleted))
                .ToList();

            // Добавляем задачи в отображаемый список
            foreach (var task in tasksForDate)
            {
                SelectedTasks.Add(task);
            }

            // Если задач нет, показываем соответствующее сообщение
            if (tasksForDate.Count == 0)
            {
                if (showCompleted)
                {
                    SelectedDateText.Text += " — задач нет";
                }
                else
                {
                    SelectedDateText.Text += " — активных задач нет";
                }
            }
        }


        // ==========================================
        // ПЕРЕКЛЮЧЕНИЕ МЕСЯЦЕВ
        // ==========================================


        private void PreviousMonth_Click(object sender, RoutedEventArgs e)
        {
            if (_calendarMode == "Месяц")
            {
                CurrentMonth = CurrentMonth.AddMonths(-1);

                SelectedDate = CurrentMonth;
            }
            else if (_calendarMode == "Неделя")
            {
                SelectedDate = SelectedDate.AddDays(-7);
            }
            else
            {
                SelectedDate = SelectedDate.AddDays(-1);
            }

            CurrentMonth = new DateTime(
                SelectedDate.Year,
                SelectedDate.Month,
                1);

            BuildCalendar();
            UpdateSelectedTasks();
        }

        private void NextMonth_Click(object sender, RoutedEventArgs e)
        {
            if (_calendarMode == "Месяц")
            {
                CurrentMonth = CurrentMonth.AddMonths(1);

                SelectedDate = CurrentMonth;
            }
            else if (_calendarMode == "Неделя")
            {
                SelectedDate = SelectedDate.AddDays(7);
            }
            else
            {
                SelectedDate = SelectedDate.AddDays(1);
            }

            CurrentMonth = new DateTime(SelectedDate.Year, SelectedDate.Month, 1);

            BuildCalendar();
            UpdateSelectedTasks();
        }

        private void Today_Click(object sender, RoutedEventArgs e)
        {
            // Получаем сегодняшнюю дату
            DateTime today = DateTime.Today;

            // Переключаем календарь на текущий месяц
            CurrentMonth = new DateTime(today.Year, today.Month, 1);

            // Выбираем сегодняшний день
            SelectedDate = today;

            // Перестраиваем календарь
            BuildCalendar();

            // Обновляем список задач
            UpdateSelectedTasks();
        }
        private void MonthMode_Click(object sender, RoutedEventArgs e)
        {
            _calendarMode = "Месяц";
            BuildCalendar();
            UpdateSelectedTasks();
        }

        private void WeekMode_Click(object sender, RoutedEventArgs e)
        {
            _calendarMode = "Неделя";
            BuildCalendar();
            UpdateSelectedTasks();
        }

        private void DayMode_Click(object sender, RoutedEventArgs e)
        {
            _calendarMode = "День";
            BuildCalendar();
            UpdateSelectedTasks();
        }

        private void UpdateModeButtons()
        {
            // Сбрасываем оформление всех кнопок
            MonthModeButton.Background = Brushes.White;
            MonthModeButton.Foreground =
                new SolidColorBrush(Color.FromRgb(51, 65, 85));

            WeekModeButton.Background = Brushes.White;
            WeekModeButton.Foreground =
                new SolidColorBrush(Color.FromRgb(51, 65, 85));

            DayModeButton.Background = Brushes.White;
            DayModeButton.Foreground =
                new SolidColorBrush(Color.FromRgb(51, 65, 85));

            // Выделяем активный режим
            Button activeButton = _calendarMode switch
            {
                "Месяц" => MonthModeButton,
                "Неделя" => WeekModeButton,
                "День" => DayModeButton,
                _ => MonthModeButton
            };

            activeButton.Background =
                new SolidColorBrush(Color.FromRgb(79, 70, 229));

            activeButton.Foreground = Brushes.White;
        }
    }
}
