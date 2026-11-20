using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using TaskFlow.Models;

namespace TaskFlow
{
    public partial class MainWindow : Window
    {
        // Коллекция всех задач
        public ObservableCollection<TaskItem> Tasks { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            // Создаем коллекцию задач
            Tasks = new ObservableCollection<TaskItem>();

            // Связываем коллекцию с интерфейсом
            DataContext = this;

            // Подписываемся на изменения коллекции
            Tasks.CollectionChanged += Tasks_CollectionChanged;

            // Добавляем демонстрационные задачи
            AddInitialTasks();

            // Обновляем статистику
            UpdateStatistics();
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
