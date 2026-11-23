using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TaskFlow.Models;
using TaskFlow.Views;
using TaskFlow.Services;

namespace TaskFlow
{
    public partial class MainWindow : Window
    {
        // Коллекция всех задач
        public ObservableCollection<TaskItem> Tasks { get; set; }

        // Представление коллекции для поиска и фильтрации
        private ICollectionView? _tasksView;

        public MainWindow()
        {
            InitializeComponent();

            // Загружаем задачи из файла
            var savedTasks = TaskStorageService.LoadTasks();

            Tasks = savedTasks == null
                ? new ObservableCollection<TaskItem>()
                : new ObservableCollection<TaskItem>(savedTasks);

            DataContext = this;

            // Настраиваем фильтрацию
            _tasksView = CollectionViewSource.GetDefaultView(Tasks);
            _tasksView.Filter = FilterTasks;

            // Подписываем задачи на отслеживание изменений
            foreach (var task in Tasks)
            {
                task.PropertyChanged += Task_PropertyChanged;
            }

            Tasks.CollectionChanged += Tasks_CollectionChanged;

            // Демонстрационные задачи создаём только при первом запуске
            if (savedTasks == null)
            {
                AddInitialTasks();
            }

            UpdateStatistics();
        }

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
            {
                priority = priorityItem.Content?.ToString()
                           ?? "Все приоритеты";
            }

            if (priority != "Все приоритеты" &&
                task.Priority != priority)
            {
                return false;
            }

            // Фильтр по статусу
            string status = "Все задачи";

            if (StatusFilter.SelectedItem is ComboBoxItem statusItem)
            {
                status = statusItem.Content?.ToString()
                         ?? "Все задачи";
            }

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
            TextChangedEventArgs e)
        {
            RefreshTaskFilter();
        }

        // Изменение выбранных фильтров
        private void Filter_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            RefreshTaskFilter();
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
                priority = selectedItem.Content?.ToString() ?? "Средний";
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

            // Добавляем задачу в начало списка
            Tasks.Insert(0, newTask);

            // Очищаем поле названия
            TaskTitleInput.Clear();

            // Сбрасываем приоритет на «Средний»
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

        // Редактирование задачи через отдельное окно
        private void EditTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button &&
                button.Tag is TaskItem task)
            {
                // Создаем окно редактирования
                EditTaskWindow editWindow = new EditTaskWindow(task);

                // Указываем главное окно владельцем
                editWindow.Owner = this;

                // Открываем окно редактирования
                bool? result = editWindow.ShowDialog();

                // Если изменения сохранены
                if (result == true)
                {
                    UpdateStatistics();
                    RefreshTaskFilter();
                }
            }
        }

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
        // Реакция на изменение коллекции задач
        private void Tasks_CollectionChanged(object? sender,NotifyCollectionChangedEventArgs e)
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

            // Сохраняем изменения
            SaveTasks();
        }

        // Реакция на изменение свойств задачи
        private void Task_PropertyChanged(
            object? sender,
            PropertyChangedEventArgs e)
        {
            UpdateStatistics();

            // Обновляем фильтрацию
            _tasksView?.Refresh();

            // Сохраняем изменения задачи
            SaveTasks();
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
