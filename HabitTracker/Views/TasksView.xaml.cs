using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using HabitTracker.Data;
using HabitTracker.Models;

namespace HabitTracker.Views
{
    public partial class TasksView : UserControl
    {
        private readonly ObservableCollection<TaskItem> _items = new();

        public TasksView()
        {
            InitializeComponent();

            PriorityCombo.ItemsSource = Enum.GetValues(typeof(TaskPriority));
            PriorityCombo.SelectedIndex = 1;

            Refresh();
            TasksGrid.ItemsSource = _items;
        }

        private void Refresh()
        {
            _items.Clear();
            var uid = DataStore.Instance.CurrentUser!.Id;
            foreach (var t in DataStore.Instance.Tasks.Where(x => x.UserId == uid)
                         .OrderBy(x => x.IsDone).ThenByDescending(x => x.Priority))
                _items.Add(t);
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var title = TitleBox.Text.Trim();
            if (title.Length == 0)
            {
                MessageBox.Show("Введите текст задачи", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var task = new TaskItem
            {
                UserId = DataStore.Instance.CurrentUser!.Id,
                Title = title,
                Priority = (TaskPriority)PriorityCombo.SelectedItem,
                DueDate = DuePicker.SelectedDate
            };
            DataStore.Instance.AddTask(task);
            Refresh();

            TitleBox.Clear();
            DuePicker.SelectedDate = null;
        }

        private void Done_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.Tag is TaskItem task)
            {
                task.IsDone = cb.IsChecked == true;
                DataStore.Instance.UpdateTask(task);
            }
            Refresh();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.Tag is TaskItem t)
            {
                DataStore.Instance.DeleteTask(t);
                _items.Remove(t);
            }
        }
    }
}
