using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using HabitTracker.Data;
using HabitTracker.Models;

namespace HabitTracker.Views
{
    public partial class HabitsView : UserControl
    {
        private readonly ObservableCollection<Habit> _items = new();

        public HabitsView()
        {
            InitializeComponent();

            CategoryCombo.ItemsSource = DataStore.Instance.Categories;
            FreqCombo.ItemsSource = Enum.GetValues(typeof(HabitFrequency));
            FreqCombo.SelectedIndex = 0;
            if (DataStore.Instance.Categories.Count > 0) CategoryCombo.SelectedIndex = 0;

            Refresh();
            HabitsGrid.ItemsSource = _items;
        }

        private void Refresh()
        {
            _items.Clear();
            var uid = DataStore.Instance.CurrentUser!.Id;
            foreach (var h in DataStore.Instance.Habits.Where(x => x.UserId == uid))
                _items.Add(h);
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var name = NameBox.Text.Trim();
            if (name.Length == 0)
            {
                MessageBox.Show("Введите название привычки", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var freq = (HabitFrequency)FreqCombo.SelectedItem;
            var habit = new Habit
            {
                UserId = DataStore.Instance.CurrentUser!.Id,
                Name = name,
                Category = CategoryCombo.SelectedItem as Category,
                Frequency = freq,
                TargetPerWeek = freq == HabitFrequency.Ежедневно ? 7 : 1
            };
            DataStore.Instance.AddHabit(habit);
            _items.Add(habit);

            NameBox.Clear();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.Tag is Habit h)
            {
                if (MessageBox.Show($"Удалить привычку «{h.Name}»?", "Подтверждение",
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    DataStore.Instance.DeleteHabit(h);
                    _items.Remove(h);
                }
            }
        }
    }
}
