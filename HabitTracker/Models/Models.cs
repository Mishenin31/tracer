using System.ComponentModel;
using System.Collections.ObjectModel;

namespace HabitTracker.Models
{
    public enum UserRole { Admin = 0, User = 1 }

    public enum TaskPriority { Низкий = 0, Средний = 1, Высокий = 2 }

    public enum HabitFrequency { Ежедневно = 0, Еженедельно = 1 }

    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; } = "";
        public string Password { get; set; } = "";
        public string FullName { get; set; } = "";
        public UserRole Role { get; set; }

        public string RoleDisplay => Role == UserRole.Admin ? "Администратор" : "Пользователь";
        public string Display => $"{FullName} ({Login})";
        public override string ToString() => Display;
    }

    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string ColorHex { get; set; } = "#7C6BAD";

        public string Display => Name;
        public override string ToString() => Display;
    }

    public class Habit : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = "";
        public Category? Category { get; set; }
        public HabitFrequency Frequency { get; set; }
        public int TargetPerWeek { get; set; } = 7;

        private int _currentStreak;
        public int CurrentStreak
        {
            get => _currentStreak;
            set { _currentStreak = value; OnPropertyChanged(nameof(CurrentStreak)); OnPropertyChanged(nameof(StreakDisplay)); }
        }

        private bool _doneToday;
        public bool DoneToday
        {
            get => _doneToday;
            set { _doneToday = value; OnPropertyChanged(nameof(DoneToday)); }
        }

        public string FrequencyDisplay => Frequency.ToString();
        public string StreakDisplay => $"{CurrentStreak} дн.";
        public string CategoryName => Category?.Name ?? "—";
        public string ColorHex => Category?.ColorHex ?? "#7C6BAD";

        public ObservableCollection<HabitLog> Logs { get; set; } = new();

        public string Display => Name;
        public override string ToString() => Display;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    public class HabitLog
    {
        public int Id { get; set; }
        public int HabitId { get; set; }
        public DateTime Date { get; set; }
        public bool Completed { get; set; }

        public string DateDisplay => Date.ToString("dd.MM.yyyy");
    }

    public class TaskItem : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = "";
        public TaskPriority Priority { get; set; }
        public DateTime? DueDate { get; set; }

        private bool _isDone;
        public bool IsDone
        {
            get => _isDone;
            set { _isDone = value; OnPropertyChanged(nameof(IsDone)); }
        }

        public string PriorityDisplay => Priority.ToString();
        public string DueDisplay => DueDate?.ToString("dd.MM.yyyy") ?? "—";

        public string Display => Title;
        public override string ToString() => Display;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
