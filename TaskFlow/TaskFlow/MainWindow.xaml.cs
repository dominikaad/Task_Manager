using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using TaskFlow.Models;
using TaskFlow.Views;
using System.ComponentModel;
using System.Windows.Data;

namespace TaskFlow
{
    public partial class MainWindow : Window
    {
        // Коллекция всех задач
        public ObservableCollection<TaskItem> Tasks { get; set; }
        private ICollectionView? _tasksView;

        public MainWindow()
        {
            InitializeComponent();

            // Создаем коллекцию задач
            Tasks = new ObservableCollection<TaskItem>();

            // Связываем коллекцию с интерфейсом
            DataContext = this;

            // Получаем представление коллекции задач
            _tasksView = CollectionViewSource.GetDefaultView(Tasks);

            // Подключаем фильтрацию
            _tasksView.Filter = FilterTasks;

            // Подписываемся на изменения коллекции
            Tasks.CollectionChanged += Tasks_CollectionChanged;

            // Добавляем демонстрационные задачи
            AddInitialTasks();

            // Обновляем статистику
            UpdateStatistics();


            // Проверка задачи по выбранным фильтрам
            private bool FilterTasks(object item)
            {
                if (item is not TaskItem task)
                    return false;

                // Поиск по названию
                string searchText = SearchInput.Text.Trim();

                if (!string.IsNullOrWhiteSpace(searchText) &&
                    !task.Title.Contains(
                        searchText,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                // Фильтр по приоритету
                string priority = "Все приоритеты";

                if (PriorityFilter.SelectedItem is ComboBoxItem priorityItem)
                    priority = priorityItem.Content.ToString() ?? priority;

                if (priority != "Все приоритеты" &&
                    task.Priority != priority)
                {
                    return false;
                }

                // Фильтр по статусу
                string status = "Все задачи";

                if (StatusFilter.SelectedItem is ComboBoxItem statusItem)
                    status = statusItem.Content.ToString() ?? status;

                if (status == "Активные" && task.IsCompleted)
                    return false;

                if (status == "Выполненные" && !task.IsCompleted)
                    return false;

                return true;
            }

            // Обновление фильтрации
            private void RefreshTaskFilter()
            {
                _tasksView?.Refresh();
            }

            // Изменение поискового запроса
            private void SearchInput_TextChanged(
                object sender,
                System.Windows.Controls.TextChangedEventArgs e)
            {
                RefreshTaskFilter();
            }

            // Изменение фильтров
            private void Filter_SelectionChanged(
                object sender,
                System.Windows.Controls.SelectionChangedEventArgs e)
            {
                RefreshTaskFilter();
            }
        }
        // Добавление начальных задач для демонстрации
        private void AddInitialTasks()
        {
            Tasks.Add(new TaskItem
            {
                Title = "Изучить C#",
                IsCompleted = false,
                Priority = "Высокий",
                Deadline = DateTime.Today.AddDays(3)
            });

            Tasks.Add(new TaskItem
            {
                Title = "Выполнить лабораторную работу",
                IsCompleted = false,
                Priority = "Критический",
                Deadline = DateTime.Today.AddDays(1)
            });

            Tasks.Add(new TaskItem
            {
                Title = "Создать проект TaskFlow",
                IsCompleted = true,
                Priority = "Средний",
                Deadline = DateTime.Today.AddDays(-1)
            });
        }



        // Добавление новой задачи
        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            string title = TaskTitleInput.Text.Trim();

            // Проверяем, что название не пустое
            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show(
                    "Введите название задачи!",
                    "Предупреждение",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            // Получаем выбранный приоритет
            string priority = "Средний";

            if (TaskPriorityInput.SelectedItem is ComboBoxItem selectedItem)
            {
                priority = selectedItem.Content.ToString() ?? "Средний";
            }

            // Получаем выбранную дату
            DateTime? deadline = TaskDeadlineInput.SelectedDate;

            // Создаем новую задачу
            TaskItem newTask = new TaskItem
            {
                Title = title,
                IsCompleted = false,
                Priority = priority,
                Deadline = deadline
            };

            // Добавляем задачу в коллекцию
            Tasks.Insert(0, newTask);

            // Очищаем поле ввода
            TaskTitleInput.Clear();

            // Сбрасываем приоритет
            TaskPriorityInput.SelectedIndex = 1;

            // Очищаем выбранную дату
            TaskDeadlineInput.SelectedDate = null;

            // Возвращаем курсор в поле ввода
            TaskTitleInput.Focus();
        }

        // Удаление задачи
        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button &&
                button.Tag is TaskItem task)
            {
                Tasks.Remove(task);
            }
        }

        // Редактирование задачи
        private void EditTask_Click(object sender, RoutedEventArgs e)
        {
            // Получаем задачу, для которой нажали кнопку
            if (sender is Button button &&
                button.Tag is TaskItem task)
            {
                // Создаем окно редактирования
                EditTaskWindow editWindow = new EditTaskWindow(task);

                // Указываем главное окно владельцем
                editWindow.Owner = this;

                // Открываем окно
                bool? result = editWindow.ShowDialog();

                // Если изменения сохранены, обновляем статистику
                if (result == true)
                {
                    UpdateStatistics();
                }
            }
        }


        // Реакция на изменение коллекции задач
        private void Tasks_CollectionChanged(
            object? sender,
            NotifyCollectionChangedEventArgs e)
        {
            // Подписываемся на изменения новых задач
            if (e.NewItems != null)
            {
                foreach (TaskItem task in e.NewItems)
                {
                    task.PropertyChanged += Task_PropertyChanged;
                }
            }

            // Отписываемся от удаленных задач
            if (e.OldItems != null)
            {
                foreach (TaskItem task in e.OldItems)
                {
                    task.PropertyChanged -= Task_PropertyChanged;
                }
            }

            UpdateStatistics();
        }

        // Реакция на изменение свойств задачи
        private void Task_PropertyChanged(
            object? sender,
            PropertyChangedEventArgs e)
        {
            UpdateStatistics();
            _tasksView?.Refresh();

        }

        // Обновление статистики
        private void UpdateStatistics()
        {
            int total = Tasks.Count;

            int completed = Tasks.Count(
                task => task.IsCompleted
            );

            int active = total - completed;

            TotalCountText.Text = total.ToString();
            ActiveCountText.Text = active.ToString();
            CompletedCountText.Text = completed.ToString();
        }
    }
}
