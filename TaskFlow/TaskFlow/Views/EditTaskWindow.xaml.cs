using System;
using System.Windows;
using System.Windows.Controls;
using TaskFlow.Models;


namespace TaskFlow.Views
{
    public partial class EditTaskWindow : Window
    {
        private readonly TaskItem _task;

        public EditTaskWindow(TaskItem task)
        {
            InitializeComponent();

            _task = task;

            // Заполняем поля текущими данными задачи
            TitleInput.Text = task.Title;

            foreach (ComboBoxItem item in PriorityInput.Items)
            {
                if (item.Content.ToString() == task.Priority)
                {
                    PriorityInput.SelectedItem = item;
                    break;
                }
            }

            DeadlineInput.SelectedDate = task.Deadline;
        }

        // Сохранение изменений
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleInput.Text))
            {
                MessageBox.Show(
                    "Введите название задачи.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (PriorityInput.SelectedItem is not ComboBoxItem selectedPriority)
            {
                MessageBox.Show("Выберите приоритет задачи.");
                return;
            }

            _task.Title = TitleInput.Text.Trim();
            _task.Priority = selectedPriority.Content.ToString();
            _task.Deadline = DeadlineInput.SelectedDate;

            DialogResult = true;
        }

        // Отмена редактирования
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
