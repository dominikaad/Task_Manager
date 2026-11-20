using System.ComponentModel;

namespace TaskFlow.Models
{
    public class TaskItem : INotifyPropertyChanged
    {
        private string _title = string.Empty;
        private string _priority = "Средний";
        private bool _isCompleted;
        private DateTime? _deadline;

        // Название задачи
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        // Статус выполнения задачи
        public bool IsCompleted
        {
            get { return _isCompleted; }
            set
            {
                _isCompleted = value;

                OnPropertyChanged(nameof(IsCompleted));
                OnPropertyChanged(nameof(StatusText));
            }
        }

        // Текстовое отображение статуса
        public string StatusText
        {
            get
            {
                return IsCompleted ? "Выполнена" : "Активна";
            }
        }

        // Уведомление интерфейса об изменении свойств
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName)
            );
        }

        // Приоритет задачи
        public string Priority
        {
            get { return _priority; }
            set
            {
                _priority = value;
                OnPropertyChanged(nameof(Priority));
            }
        }
        // Срок выполнения задачи
        public DateTime? Deadline
        {
            get { return _deadline; }
            set
            {
                _deadline = value;

                OnPropertyChanged(nameof(Deadline));
                OnPropertyChanged(nameof(DeadlineText));
            }
        }

        // Текстовое отображение срока выполнения
        public string DeadlineText
        {
            get
            {
                if (Deadline.HasValue)
                {
                    return "Срок: " + Deadline.Value.ToString("dd.MM.yyyy");
                }

                return "Срок не указан";
            }
        }
    }
}