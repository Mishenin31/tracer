using System.Windows;
using System.Windows.Controls;
using HabitTracker.Data;
using HabitTracker.Models;

namespace HabitTracker.Views
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
            Load();
        }

        private void Load()
        {
            var user = DataStore.Instance.CurrentUser!;
            GreetingText.Text = $"Привет, {user.FullName.Split(' ')[0]}!";
            DateText.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy");

            var habits = DataStore.Instance.Habits.Where(h => h.UserId == user.Id).ToList();
            foreach (var h in habits) DataStore.Instance.RecalcStreak(h);

            HabitsList.ItemsSource = habits;
            EmptyHabits.Visibility = habits.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

            var doneCount = habits.Count(h => h.DoneToday);
            HabitsProgressText.Text = $"{doneCount} / {habits.Count}";

            var openTasks = DataStore.Instance.Tasks.Count(t => t.UserId == user.Id && !t.IsDone);
            TasksOpenText.Text = openTasks.ToString();

            var best = habits.Count > 0 ? habits.Max(h => h.CurrentStreak) : 0;
            BestStreakText.Text = $"{best} дн.";
        }

        private void HabitCheck_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.Tag is Habit h)
            {
                DataStore.Instance.ToggleHabitToday(h);
                Load();
            }
        }
    }
}
