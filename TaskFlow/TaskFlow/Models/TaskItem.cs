using System.ComponentModel;

namespace TaskFlow.Models
{
    public class TaskItem : INotifyPropertyChanged
    {
        private string _title = string.Empty;
        private bool _isCompleted;

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
    }
}
