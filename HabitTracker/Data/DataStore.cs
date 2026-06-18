using System.Collections.ObjectModel;
using Microsoft.Data.SqlClient;
using HabitTracker.Models;

namespace HabitTracker.Data
{
    public sealed class DataStore
    {
        private const string DefaultConnectionString = @"Server=(localdb)\MSSQLLocalDB;Database=HabitTracker;Trusted_Connection=True;TrustServerCertificate=True";
        private static readonly DataStore _instance = new();
        public static DataStore Instance => _instance;

        public ObservableCollection<User> Users { get; } = new();
        public ObservableCollection<Category> Categories { get; } = new();
        public ObservableCollection<Habit> Habits { get; } = new();
        public ObservableCollection<TaskItem> Tasks { get; } = new();

        public User? CurrentUser { get; set; }
        public bool IsDatabaseConnected { get; private set; }
        public string StorageInfo => IsDatabaseConnected ? "SQL Server" : "демо-данные в памяти";

        private readonly string _connectionString;
        private int _userId = 1, _catId = 1, _habitId = 1, _taskId = 1, _logId = 1;

        private DataStore()
        {
            _connectionString = Environment.GetEnvironmentVariable("HABITTRACKER_CONNECTION_STRING") ?? DefaultConnectionString;
            try
            {
                LoadFromDatabase();
                IsDatabaseConnected = true;
            }
            catch
            {
                Seed();
            }
        }

        private void LoadFromDatabase()
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using (var command = new SqlCommand("SELECT Id, Login, Password, FullName, Role FROM dbo.Users ORDER BY Id", connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Users.Add(new User
                    {
                        Id = reader.GetInt32(0),
                        Login = reader.GetString(1),
                        Password = reader.GetString(2),
                        FullName = reader.GetString(3),
                        Role = (UserRole)reader.GetInt32(4)
                    });
                }
            }

            using (var command = new SqlCommand("SELECT Id, Name, ColorHex FROM dbo.Categories ORDER BY Id", connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                    Categories.Add(new Category { Id = reader.GetInt32(0), Name = reader.GetString(1), ColorHex = reader.GetString(2) });
            }

            using (var command = new SqlCommand("SELECT Id, UserId, Name, CategoryId, Frequency, TargetPerWeek FROM dbo.Habits ORDER BY Id", connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var categoryId = reader.IsDBNull(3) ? (int?)null : reader.GetInt32(3);
                    Habits.Add(new Habit
                    {
                        Id = reader.GetInt32(0),
                        UserId = reader.GetInt32(1),
                        Name = reader.GetString(2),
                        Category = categoryId.HasValue ? Categories.FirstOrDefault(c => c.Id == categoryId.Value) : null,
                        Frequency = (HabitFrequency)reader.GetInt32(4),
                        TargetPerWeek = reader.GetInt32(5)
                    });
                }
            }

            using (var command = new SqlCommand("SELECT Id, HabitId, LogDate, Completed FROM dbo.HabitLogs ORDER BY Id", connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var habit = Habits.FirstOrDefault(h => h.Id == reader.GetInt32(1));
                    habit?.Logs.Add(new HabitLog { Id = reader.GetInt32(0), HabitId = reader.GetInt32(1), Date = reader.GetDateTime(2), Completed = reader.GetBoolean(3) });
                }
            }

            using (var command = new SqlCommand("SELECT Id, UserId, Title, Priority, DueDate, IsDone FROM dbo.Tasks ORDER BY Id", connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                    Tasks.Add(new TaskItem { Id = reader.GetInt32(0), UserId = reader.GetInt32(1), Title = reader.GetString(2), Priority = (TaskPriority)reader.GetInt32(3), DueDate = reader.IsDBNull(4) ? null : reader.GetDateTime(4), IsDone = reader.GetBoolean(5) });
            }

            _userId = NextId(Users.Select(u => u.Id));
            _catId = NextId(Categories.Select(c => c.Id));
            _habitId = NextId(Habits.Select(h => h.Id));
            _taskId = NextId(Tasks.Select(t => t.Id));
            _logId = NextId(Habits.SelectMany(h => h.Logs).Select(l => l.Id));
            foreach (var h in Habits) RecalcStreak(h);
        }

        private static int NextId(IEnumerable<int> ids) => ids.Any() ? ids.Max() + 1 : 1;

        private int InsertAndGetId(string sql, params SqlParameter[] parameters)
        {
            if (!IsDatabaseConnected) return 0;
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql + "; SELECT CAST(SCOPE_IDENTITY() AS int);", connection);
            command.Parameters.AddRange(parameters);
            connection.Open();
            return (int)command.ExecuteScalar()!;
        }

        private void Execute(string sql, params SqlParameter[] parameters)
        {
            if (!IsDatabaseConnected) return;
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddRange(parameters);
            connection.Open();
            command.ExecuteNonQuery();
        }

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

        public void AddUser(User user)
        {
            var id = InsertAndGetId("INSERT INTO dbo.Users (Login, Password, FullName, Role) VALUES (@login, @password, @fullName, @role)",
                new SqlParameter("@login", user.Login), new SqlParameter("@password", user.Password), new SqlParameter("@fullName", user.FullName), new SqlParameter("@role", (int)user.Role));
            if (id > 0) user.Id = id; else user.Id = NextUserId();
            Users.Add(user);
        }

        public void DeleteUser(User user)
        {
            Execute("DELETE FROM dbo.Users WHERE Id = @id", new SqlParameter("@id", user.Id));
            Users.Remove(user);
        }

        public void AddCategory(Category category)
        {
            var id = InsertAndGetId("INSERT INTO dbo.Categories (Name, ColorHex) VALUES (@name, @color)", new SqlParameter("@name", category.Name), new SqlParameter("@color", category.ColorHex));
            if (id > 0) category.Id = id; else category.Id = NextCatId();
            Categories.Add(category);
        }

        public void DeleteCategory(Category category)
        {
            Execute("DELETE FROM dbo.Categories WHERE Id = @id", new SqlParameter("@id", category.Id));
            Categories.Remove(category);
        }

        public void AddHabit(Habit habit)
        {
            var id = InsertAndGetId("INSERT INTO dbo.Habits (UserId, Name, CategoryId, Frequency, TargetPerWeek) VALUES (@userId, @name, @categoryId, @frequency, @target)",
                new SqlParameter("@userId", habit.UserId), new SqlParameter("@name", habit.Name), new SqlParameter("@categoryId", (object?)habit.Category?.Id ?? DBNull.Value), new SqlParameter("@frequency", (int)habit.Frequency), new SqlParameter("@target", habit.TargetPerWeek));
            if (id > 0) habit.Id = id; else habit.Id = NextHabitId();
            Habits.Add(habit);
        }

        public void DeleteHabit(Habit habit)
        {
            Execute("DELETE FROM dbo.Habits WHERE Id = @id", new SqlParameter("@id", habit.Id));
            Habits.Remove(habit);
        }

        public void AddTask(TaskItem task)
        {
            var id = InsertAndGetId("INSERT INTO dbo.Tasks (UserId, Title, Priority, DueDate, IsDone) VALUES (@userId, @title, @priority, @dueDate, @isDone)",
                new SqlParameter("@userId", task.UserId), new SqlParameter("@title", task.Title), new SqlParameter("@priority", (int)task.Priority), new SqlParameter("@dueDate", (object?)task.DueDate?.Date ?? DBNull.Value), new SqlParameter("@isDone", task.IsDone));
            if (id > 0) task.Id = id; else task.Id = NextTaskId();
            Tasks.Add(task);
        }

        public void UpdateTask(TaskItem task) => Execute("UPDATE dbo.Tasks SET IsDone = @isDone WHERE Id = @id", new SqlParameter("@isDone", task.IsDone), new SqlParameter("@id", task.Id));

        public void DeleteTask(TaskItem task)
        {
            Execute("DELETE FROM dbo.Tasks WHERE Id = @id", new SqlParameter("@id", task.Id));
            Tasks.Remove(task);
        }

        public void ToggleHabitToday(Habit h)
        {
            var today = DateTime.Today;
            var log = h.Logs.FirstOrDefault(l => l.Date.Date == today);
            if (log == null)
            {
                var newLog = new HabitLog { Id = NextLogId(), HabitId = h.Id, Date = today, Completed = true };
                var id = InsertAndGetId("INSERT INTO dbo.HabitLogs (HabitId, LogDate, Completed) VALUES (@habitId, @date, @completed)", new SqlParameter("@habitId", h.Id), new SqlParameter("@date", today), new SqlParameter("@completed", true));
                if (id > 0) newLog.Id = id;
                h.Logs.Add(newLog);
            }
            else
            {
                Execute("DELETE FROM dbo.HabitLogs WHERE Id = @id", new SqlParameter("@id", log.Id));
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
