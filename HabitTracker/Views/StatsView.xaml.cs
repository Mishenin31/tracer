using System.Windows.Controls;
using HabitTracker.Data;

namespace HabitTracker.Views
{
    public partial class StatsView : UserControl
    {
        public class HabitBar
        {
            public string Name { get; set; } = "";
            public string StreakDisplay { get; set; } = "";
            public string ColorHex { get; set; } = "";
            public double BarWidth { get; set; }
        }

        public StatsView()
        {
            InitializeComponent();
            Load();
        }

        private void Load()
        {
            var uid = DataStore.Instance.CurrentUser!.Id;
            var habits = DataStore.Instance.Habits.Where(h => h.UserId == uid).ToList();
            foreach (var h in habits) DataStore.Instance.RecalcStreak(h);

            TotalHabits.Text = habits.Count.ToString();

            var weekAgo = DateTime.Today.AddDays(-6);
            var weekChecks = habits.Sum(h => h.Logs.Count(l => l.Completed && l.Date.Date >= weekAgo));
            WeekChecks.Text = weekChecks.ToString();

            var tasks = DataStore.Instance.Tasks.Where(t => t.UserId == uid).ToList();
            TasksDone.Text = $"{tasks.Count(t => t.IsDone)} / {tasks.Count}";

            var best = habits.Count > 0 ? habits.Max(h => h.CurrentStreak) : 0;
            BestStreak.Text = $"{best} дн.";

            var maxStreak = Math.Max(1, best);
            var bars = habits
                .OrderByDescending(h => h.CurrentStreak)
                .Select(h => new HabitBar
                {
                    Name = h.Name,
                    StreakDisplay = h.StreakDisplay,
                    ColorHex = h.ColorHex,
                    BarWidth = 60 + (h.CurrentStreak / (double)maxStreak) * 380
                })
                .ToList();

            ProgressList.ItemsSource = bars;
        }
    }
}
