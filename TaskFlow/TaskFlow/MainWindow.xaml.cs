using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TaskFlow.Models;
using TaskFlow.Services;
using TaskFlow.Views;

namespace TaskFlow
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<TaskItem> Tasks { get; set; }

        private ICollectionView? _tasksView;

        // ==========================================
        // ОТКРЫТИЕ КАЛЕНДАРЯ
        // ==========================================

        private void OpenCalendar_Click(
            object sender,
            RoutedEventArgs e)
        {
            CalendarWindow calendarWindow = new CalendarWindow(Tasks);

            calendarWindow.Owner = this;

            calendarWindow.ShowDialog();
        }

        public MainWindow()
        {
            InitializeComponent();

            // Загружаем задачи из файла
            var savedTasks = TaskStorageService.LoadTasks();

            if (savedTasks != null)
            {
                Tasks = new ObservableCollection<TaskItem>(savedTasks);
            }
            else
            {
                Tasks = new ObservableCollection<TaskItem>();
            }

            // Устанавливаем источник данных
            DataContext = this;

            // Настраиваем представление коллекции
            _tasksView = CollectionViewSource.GetDefaultView(Tasks);

            // Настраиваем фильтрацию
            _tasksView.Filter = FilterTasks;

            // Подписываем задачи на отслеживание изменений
            foreach (var task in Tasks)
            {
                task.PropertyChanged += Task_PropertyChanged;
            }

            // Отслеживаем добавление и удаление задач
            Tasks.CollectionChanged += Tasks_CollectionChanged;

            // Демонстрационные задачи добавляем только при первом запуске
            if (savedTasks == null)
            {
                AddInitialTasks();
            }

            // Применяем сортировку
            ApplyTaskSort();

            // Обновляем статистику
            UpdateStatistics();
        }

        // ==========================================
        // ФИЛЬТРАЦИЯ ЗАДАЧ
        // ==========================================

        private bool FilterTasks(object item)
        {
            if (item is not TaskItem task)
                return false;

            // Поиск по названию задачи
            string searchText = SearchInput.Text?.Trim() ?? "";

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                if (!task.Title.Contains(
                    searchText,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            // Фильтр по приоритету
            if (PriorityFilter.SelectedItem is ComboBoxItem priorityItem)
            {
                string selectedPriority =
                    priorityItem.Content?.ToString() ?? "";

                if (selectedPriority != "Все приоритеты" &&
                    task.Priority != selectedPriority)
                {
                    return false;
                }
            }

            // Фильтр по статусу
            if (StatusFilter.SelectedItem is ComboBoxItem statusItem)
            {
                string selectedStatus =
                    statusItem.Content?.ToString() ?? "";

                if (selectedStatus == "Активные" && task.IsCompleted)
                {
                    return false;
                }

                if (selectedStatus == "Выполненные" && !task.IsCompleted)
                {
                    return false;
                }
            }

            return true;
        }

        // Обновление поиска и фильтров
        private void RefreshTaskFilter()
        {
            _tasksView?.Refresh();
        }

        // Обработчик поиска
        private void SearchInput_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            RefreshTaskFilter();
        }

        // Обработчик изменения фильтров
        private void Filter_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            RefreshTaskFilter();

            ApplyTaskSort();
        }

        // ==========================================
        // СОРТИРОВКА ЗАДАЧ
        // ==========================================

        private void ApplyTaskSort()
        {
            if (_tasksView is not ListCollectionView listView)
                return;

            int sortMode = SortFilter.SelectedIndex;

            if (sortMode == 0)
            {
                // Возвращаем исходный порядок
                listView.CustomSort = null;
            }
            else
            {
                // Применяем выбранную сортировку
                listView.CustomSort = new TaskItemComparer(sortMode);
            }

            listView.Refresh();
        }

        // ==========================================
        // ДЕМОНСТРАЦИОННЫЕ ЗАДАЧИ
        // ==========================================

        private void AddInitialTasks()
        {
            Tasks.Add(new TaskItem
            {
                Title = "Подготовить курсовую работу",
                Priority = "Высокий",
                Deadline = DateTime.Today.AddDays(3),
                IsCompleted = false
            });

            Tasks.Add(new TaskItem
            {
                Title = "Выполнить лабораторную работу",
                Priority = "Критический",
                Deadline = DateTime.Today.AddDays(1),
                IsCompleted = false
            });

            Tasks.Add(new TaskItem
            {
                Title = "Повторить материал",
                Priority = "Средний",
                Deadline = DateTime.Today.AddDays(5),
                IsCompleted = false
            });
        }

        // ==========================================
        // ДОБАВЛЕНИЕ ЗАДАЧИ
        // ==========================================

        private void AddTask_Click(
            object sender,
            RoutedEventArgs e)
        {
            string title = TaskTitleInput.Text.Trim();

            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show(
                    "Введите название задачи.",
                    "Внимание",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            string priority = "Средний";

            if (TaskPriorityInput.SelectedItem is ComboBoxItem priorityItem)
            {
                priority = priorityItem.Content?.ToString() ?? "Средний";
            }

            TaskItem newTask = new TaskItem
            {
                Title = title,
                Priority = priority,
                Deadline = TaskDeadlineInput.SelectedDate,
                IsCompleted = false
            };

            Tasks.Add(newTask);

            // Очищаем поля ввода
            TaskTitleInput.Clear();
            TaskDeadlineInput.SelectedDate = null;
            TaskPriorityInput.SelectedIndex = 1;

            // Обновляем статистику
            UpdateStatistics();
        }

        // ==========================================
        // УДАЛЕНИЕ ЗАДАЧИ
        // ==========================================

        private void DeleteTask_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is Button button &&
                button.Tag is TaskItem task)
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Вы действительно хотите удалить задачу \"{task.Title}\"?",
                    "Удаление задачи",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    Tasks.Remove(task);
                }
            }
        }

        // ==========================================
        // РЕДАКТИРОВАНИЕ ЗАДАЧИ
        // ==========================================

        private void EditTask_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is Button button &&
                button.Tag is TaskItem task)
            {
                EditTaskWindow editWindow = new EditTaskWindow(task);

                editWindow.Owner = this;

                bool? result = editWindow.ShowDialog();

                if (result == true)
                {
                    UpdateStatistics();

                    RefreshTaskFilter();

                    ApplyTaskSort();
                }
            }
        }

        // ==========================================
        // ОТСЛЕЖИВАНИЕ ИЗМЕНЕНИЙ КОЛЛЕКЦИИ
        // ==========================================

        private void Tasks_CollectionChanged(
            object? sender,
            NotifyCollectionChangedEventArgs e)
        {
            // Отписываемся от удалённых задач
            if (e.OldItems != null)
            {
                foreach (TaskItem task in e.OldItems)
                {
                    task.PropertyChanged -= Task_PropertyChanged;
                }
            }

            // Подписываемся на новые задачи
            if (e.NewItems != null)
            {
                foreach (TaskItem task in e.NewItems)
                {
                    task.PropertyChanged += Task_PropertyChanged;
                }
            }

            UpdateStatistics();

            RefreshTaskFilter();

            ApplyTaskSort();

            SaveTasks();
        }

        // ==========================================
        // ОТСЛЕЖИВАНИЕ ИЗМЕНЕНИЙ СВОЙСТВ ЗАДАЧИ
        // ==========================================

        private void Task_PropertyChanged(
            object? sender,
            PropertyChangedEventArgs e)
        {
            // Обновляем статистику
            UpdateStatistics();

            // Обновляем фильтрацию
            RefreshTaskFilter();

            // Обновляем сортировку
            ApplyTaskSort();

            // Сохраняем изменения
            SaveTasks();
        }

        // ==========================================
        // СОХРАНЕНИЕ ЗАДАЧ
        // ==========================================

        private void SaveTasks()
        {
            try
            {
                TaskStorageService.SaveTasks(Tasks);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось сохранить задачи.\n" + ex.Message,
                    "Ошибка сохранения",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // ==========================================
        // ОБНОВЛЕНИЕ СТАТИСТИКИ
        // ==========================================

        private void UpdateStatistics()
        {
            if (TotalTasksText == null ||
                ActiveTasksText == null ||
                CompletedTasksText == null ||
                OverdueTasksText == null)
            {
                return;
            }

            int total = Tasks.Count;

            int completed = Tasks.Count(task => task.IsCompleted);

            int active = total - completed;

            int overdue = Tasks.Count(task => task.IsOverdue);

            TotalTasksText.Text = total.ToString();

            ActiveTasksText.Text = active.ToString();

            CompletedTasksText.Text = completed.ToString();

            OverdueTasksText.Text = overdue.ToString();
        }
    }

    // ==========================================
    // КЛАСС СОРТИРОВКИ ЗАДАЧ
    // ==========================================

    public class TaskItemComparer : IComparer
    {
        private readonly int _sortMode;

        public TaskItemComparer(int sortMode)
        {
            _sortMode = sortMode;
        }

        public int Compare(object? x, object? y)
        {
            if (x is not TaskItem task1 ||
                y is not TaskItem task2)
            {
                return 0;
            }

            // Сортировка по ближайшему сроку
            if (_sortMode == 1)
            {
                if (!task1.Deadline.HasValue &&
                    !task2.Deadline.HasValue)
                {
                    return 0;
                }

                if (!task1.Deadline.HasValue)
                    return 1;

                if (!task2.Deadline.HasValue)
                    return -1;

                return task1.Deadline.Value.CompareTo(
                    task2.Deadline.Value
                );
            }

            // Сортировка по приоритету
            if (_sortMode == 2)
            {
                int priority1 = GetPriorityValue(task1.Priority);

                int priority2 = GetPriorityValue(task2.Priority);

                int result = priority2.CompareTo(priority1);

                if (result != 0)
                    return result;

                // При одинаковом приоритете
                // сначала показываем ближайший срок

                if (!task1.Deadline.HasValue &&
                    !task2.Deadline.HasValue)
                {
                    return 0;
                }

                if (!task1.Deadline.HasValue)
                    return 1;

                if (!task2.Deadline.HasValue)
                    return -1;

                return task1.Deadline.Value.CompareTo(
                    task2.Deadline.Value
                );
            }

            return 0;
        }

        // Определение числового значения приоритета
        private int GetPriorityValue(string priority)
        {
            return priority switch
            {
                "Критический" => 4,
                "Высокий" => 3,
                "Средний" => 2,
                "Низкий" => 1,
                _ => 0
            };
        }
    }
}
