
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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

            // Отображаем название месяца
            CultureInfo culture = new CultureInfo("ru-RU");

            MonthTitle.Text = culture.TextInfo.ToTitleCase(
                CurrentMonth.ToString("MMMM yyyy", culture)
            );

            // Первый день месяца
            DateTime firstDay = CurrentMonth;

            // Количество дней в месяце
            int daysInMonth = DateTime.DaysInMonth(
                CurrentMonth.Year,
                CurrentMonth.Month
            );

            // Определяем день недели.
            // Понедельник = 0, воскресенье = 6.
            int startOffset = ((int)firstDay.DayOfWeek + 6) % 7;

            // Создаём 42 ячейки (6 недель по 7 дней)
            for (int i = 0; i < 42; i++)
            {
                int dayNumber = i - startOffset + 1;

                DateTime date = firstDay.AddDays(dayNumber - 1);

                bool isCurrentMonth =
                    date.Month == CurrentMonth.Month;

                // Количество задач на выбранную дату
                int taskCount = _allTasks.Count(task =>
                    task.Deadline.HasValue &&
                    task.Deadline.Value.Date == date.Date
                );

                // Создаём кнопку дня
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
                        Color.FromRgb(226, 232, 240)
                    )
                };

                // Фон ячейки
                if (!isCurrentMonth)
                {
                    dayButton.Background = Brushes.Transparent;
                    dayButton.BorderThickness = new Thickness(0);
                }
                else if (date.Date == SelectedDate.Date)
                {
                    dayButton.Background = new SolidColorBrush(
                        Color.FromRgb(224, 231, 255)
                    );

                    dayButton.BorderBrush = new SolidColorBrush(
                        Color.FromRgb(99, 102, 241)
                    );
                }
                else
                {
                    dayButton.Background = Brushes.White;
                }

                // Содержимое ячейки
                StackPanel content = new StackPanel
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                TextBlock dayText = new TextBlock
                {
                    Text = isCurrentMonth
                        ? date.Day.ToString()
                        : "",

                    FontSize = 16,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = date.Date == DateTime.Today
                        ? new SolidColorBrush(Color.FromRgb(79, 70, 229))
                        : new SolidColorBrush(Color.FromRgb(30, 41, 59)),

                    HorizontalAlignment = HorizontalAlignment.Center
                };

                content.Children.Add(dayText);

                // Показываем количество задач
                if (isCurrentMonth && taskCount > 0)
                {
                    TextBlock countText = new TextBlock
                    {
                        Text = taskCount == 1
                            ? "1 задача"
                            : $"{taskCount} задач",

                        FontSize = 10,

                        Foreground = new SolidColorBrush(
                            Color.FromRgb(99, 102, 241)
                        ),

                        HorizontalAlignment = HorizontalAlignment.Center,

                        Margin = new Thickness(0, 4, 0, 0)
                    };

                    content.Children.Add(countText);
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
    }
}
