using System.Collections.ObjectModel;
using HabitTracker.Models;

namespace HabitTracker.Data
{
    public sealed class DataStore
    {
        private static readonly DataStore _instance = new();
        public static DataStore Instance => _instance;

        public ObservableCollection<User> Users { get; } = new();
        public ObservableCollection<Category> Categories { get; } = new();
        public ObservableCollection<Habit> Habits { get; } = new();
        public ObservableCollection<TaskItem> Tasks { get; } = new();

        public User? CurrentUser { get; set; }

        private int _userId = 1, _catId = 1, _habitId = 1, _taskId = 1, _logId = 1;

        private DataStore() => Seed();

        private void Seed()
        {
            Users.Add(new User { Id = _userId++, Login = "admin", Password = "admin", FullName = "Администратор", Role = UserRole.Admin });
            Users.Add(new User { Id = _userId++, Login = "user", Password = "user", FullName = "Иван Петров", Role = UserRole.User });

            var cHealth = new Category { Id = _catId++, Name = "Здоровье", ColorHex = "#6FB98F" };
            var cWork = new Category { Id = _catId++, Name = "Работа", ColorHex = "#5B8DEF" };
            var cMind = new Category { Id = _catId++, Name = "Развитие", ColorHex = "#C77DD6" };
            var cHome = new Category { Id = _catId++, Name = "Быт", ColorHex = "#E8A14B" };
            Categories.Add(cHealth); Categories.Add(cWork); Categories.Add(cMind); Categories.Add(cHome);

            AddHabit("Зарядка утром", cHealth, HabitFrequency.Ежедневно, 7, 2, 5);
            AddHabit("Читать 30 минут", cMind, HabitFrequency.Ежедневно, 7, 2, 12);
            AddHabit("Пить 2 л воды", cHealth, HabitFrequency.Ежедневно, 7, 2, 8);
            AddHabit("Планёрка недели", cWork, HabitFrequency.Еженедельно, 1, 2, 3);
            AddHabit("Уборка", cHome, HabitFrequency.Еженедельно, 2, 2, 4);

            Tasks.Add(new TaskItem { Id = _taskId++, UserId = 2, Title = "Сдать квартальный отчёт", Priority = TaskPriority.Высокий, DueDate = DateTime.Today.AddDays(2) });
            Tasks.Add(new TaskItem { Id = _taskId++, UserId = 2, Title = "Купить продукты", Priority = TaskPriority.Средний, DueDate = DateTime.Today });
            Tasks.Add(new TaskItem { Id = _taskId++, UserId = 2, Title = "Записаться к врачу", Priority = TaskPriority.Высокий, DueDate = DateTime.Today.AddDays(1) });
            Tasks.Add(new TaskItem { Id = _taskId++, UserId = 2, Title = "Разобрать почту", Priority = TaskPriority.Низкий, IsDone = true });
        }

        private void AddHabit(string name, Category cat, HabitFrequency freq, int target, int userId, int streak)
        {
            var h = new Habit { Id = _habitId++, UserId = userId, Name = name, Category = cat, Frequency = freq, TargetPerWeek = target, CurrentStreak = streak };
            for (int i = streak; i >= 1; i--)
                h.Logs.Add(new HabitLog { Id = _logId++, HabitId = h.Id, Date = DateTime.Today.AddDays(-i), Completed = true });
            Habits.Add(h);
        }

        public int NextUserId() => _userId++;
        public int NextCatId() => _catId++;
        public int NextHabitId() => _habitId++;
        public int NextTaskId() => _taskId++;
        public int NextLogId() => _logId++;

        public void ToggleHabitToday(Habit h)
        {
            var today = DateTime.Today;
            var log = h.Logs.FirstOrDefault(l => l.Date.Date == today);
            if (log == null)
            {
                h.Logs.Add(new HabitLog { Id = NextLogId(), HabitId = h.Id, Date = today, Completed = true });
            }
            else
            {
                h.Logs.Remove(log);
            }
            RecalcStreak(h);
        }

        public void RecalcStreak(Habit h)
        {
            int streak = 0;
            var day = DateTime.Today;
            while (h.Logs.Any(l => l.Date.Date == day && l.Completed))
            {
                streak++;
                day = day.AddDays(-1);
            }
            h.CurrentStreak = streak;
            h.DoneToday = h.Logs.Any(l => l.Date.Date == DateTime.Today && l.Completed);
        }
    }
}
