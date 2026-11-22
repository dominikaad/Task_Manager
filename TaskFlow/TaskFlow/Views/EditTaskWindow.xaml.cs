using System;
using System.Windows;
using System.Windows.Controls;
using TaskFlow.Models;

namespace TaskFlow.Views
{
    public partial class EditTaskWindow : Window
    {
        // Задача, которую редактируем
        private readonly TaskItem _task;

        public EditTaskWindow(TaskItem task)
        {
            InitializeComponent();

            _task = task;

            // Заполняем поля текущими значениями задачи
            TitleInput.Text = _task.Title;

            DeadlineInput.SelectedDate = _task.Deadline;

            // Устанавливаем текущий приоритет
            foreach (ComboBoxItem item in PriorityInput.Items)
            {
                if (item.Content.ToString() == _task.Priority)
                {
                    PriorityInput.SelectedItem = item;
                    break;
                }
            }
        }

        // Сохранение изменений
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string title = TitleInput.Text.Trim();

            // Проверяем название
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

            if (PriorityInput.SelectedItem is ComboBoxItem selectedItem)
            {
                priority = selectedItem.Content.ToString() ?? "Средний";
            }

            // Обновляем свойства задачи
            _task.Title = title;
            _task.Priority = priority;
            _task.Deadline = DeadlineInput.SelectedDate;

            // Сообщаем, что изменения сохранены
            DialogResult = true;
        }

        // Отмена редактирования
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
