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

        // ==========================================
        // ПОСТРОЕНИЕ КАЛЕНДАРЯ
        // ==========================================


        private void BuildCalendar()
        {
            CalendarGrid.Children.Clear();

            CultureInfo culture = new CultureInfo("ru-RU");

            MonthTitle.Text = culture.TextInfo.ToTitleCase(
                CurrentMonth.ToString("MMMM yyyy", culture));

            DateTime firstDay = CurrentMonth;

            int startOffset = ((int)firstDay.DayOfWeek + 6) % 7;

            // Названия приоритетов и соответствующие цвета
            string[] priorities =
            {
        "Низкий",
        "Средний",
        "Высокий",
        "Критический"
    };

            Color[] priorityColors =
            {
        Color.FromRgb(34, 197, 94),   // Зеленый
        Color.FromRgb(59, 130, 246),  // Синий
        Color.FromRgb(249, 115, 22),  // Оранжевый
        Color.FromRgb(239, 68, 68)    // Красный
    };

            // Создаем 42 ячейки календаря
            for (int i = 0; i < 42; i++)
            {
                int dayNumber = i - startOffset + 1;

                DateTime date = firstDay.AddDays(dayNumber - 1);

                bool isCurrentMonth = date.Month == CurrentMonth.Month;

                // Получаем задачи на конкретную дату
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
                    IsEnabled = isCurrentMonth,
                    Cursor = Cursors.Hand,
                    BorderThickness = new Thickness(1),
                    BorderBrush = new SolidColorBrush(
                        Color.FromRgb(226, 232, 240))
                };

                // Оформление ячеек
                if (!isCurrentMonth)
                {
                    dayButton.Background = Brushes.Transparent;
                    dayButton.BorderThickness = new Thickness(0);
                }
                else if (date.Date == SelectedDate.Date)
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

                // Контейнер содержимого ячейки
                StackPanel content = new StackPanel
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                // Номер дня
                TextBlock dayText = new TextBlock
                {
                    Text = isCurrentMonth ? date.Day.ToString() : "",
                    FontSize = 16,
                    FontWeight = FontWeights.SemiBold,

                    Foreground = date.Date == DateTime.Today
                        ? new SolidColorBrush(Color.FromRgb(79, 70, 229))
                        : new SolidColorBrush(Color.FromRgb(30, 41, 59)),

                    HorizontalAlignment = HorizontalAlignment.Center
                };

                content.Children.Add(dayText);

                // Количество задач
                if (isCurrentMonth && tasksForDate.Count > 0)
                {
                    TextBlock countText = new TextBlock
                    {
                        Text = tasksForDate.Count == 1
                            ? "1 задача"
                            : $"{tasksForDate.Count} задач",

                        FontSize = 10,

                        Foreground = new SolidColorBrush(
                            Color.FromRgb(99, 102, 241)),

                        HorizontalAlignment = HorizontalAlignment.Center,

                        Margin = new Thickness(0, 3, 0, 2)
                    };

                    content.Children.Add(countText);

                    // Контейнер цветных индикаторов
                    WrapPanel indicators = new WrapPanel
                    {
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    // Проверяем наличие задач каждого приоритета
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
        }


        // ==========================================
        // ВЫБОР ДАТЫ
        // ==========================================

        private void CalendarDay_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is Button button &&
                button.Tag is DateTime selectedDate)
            {
                SelectedDate = selectedDate;

                BuildCalendar();

                UpdateSelectedTasks();
            }
        }

        // ==========================================
        // ОБНОВЛЕНИЕ СПИСКА ЗАДАЧ
        // ==========================================

        private void UpdateSelectedTasks()
        {
            SelectedTasks.Clear();

            SelectedDateText.Text =
                $"Задачи на {SelectedDate:dd.MM.yyyy}";

            var tasksForDate = _allTasks
                .Where(task =>
                    task.Deadline.HasValue &&
                    task.Deadline.Value.Date == SelectedDate.Date
                )
                .ToList();

            foreach (var task in tasksForDate)
            {
                SelectedTasks.Add(task);
            }

            if (tasksForDate.Count == 0)
            {
                SelectedDateText.Text += " — задач нет";
            }
        }

        // ==========================================
        // ПЕРЕКЛЮЧЕНИЕ МЕСЯЦЕВ
        // ==========================================

        private void PreviousMonth_Click(
            object sender,
            RoutedEventArgs e)
        {
            CurrentMonth = CurrentMonth.AddMonths(-1);

            SelectedDate = CurrentMonth;

            BuildCalendar();

            UpdateSelectedTasks();
        }

        private void NextMonth_Click(
            object sender,
            RoutedEventArgs e)
        {
            CurrentMonth = CurrentMonth.AddMonths(1);

            SelectedDate = CurrentMonth;

            BuildCalendar();

            UpdateSelectedTasks();
        }

        private void Today_Click(object sender, RoutedEventArgs e)
        {
            // Получаем сегодняшнюю дату
            DateTime today = DateTime.Today;

            // Переключаем календарь на текущий месяц
            CurrentMonth = new DateTime(
                today.Year,
                today.Month,
                1);

            // Выбираем сегодняшний день
            SelectedDate = today;

            // Перестраиваем календарь
            BuildCalendar();

            // Обновляем список задач
            UpdateSelectedTasks();
        }

    }
}
