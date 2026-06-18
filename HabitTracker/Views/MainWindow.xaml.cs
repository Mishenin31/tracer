using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HabitTracker.Data;
using HabitTracker.Models;

namespace HabitTracker.Views
{
    public partial class MainWindow : Window
    {
        private Button? _activeNav;

        public MainWindow()
        {
            InitializeComponent();
            var user = DataStore.Instance.CurrentUser!;
            UserNameText.Text = user.FullName;
            UserRoleText.Text = user.RoleDisplay;

            BuildNav(user.Role);
        }

        private void BuildNav(UserRole role)
        {
            NavPanel.Children.Clear();

            AddNav("Сегодня", () => new DashboardView());
            AddNav("Привычки", () => new HabitsView());
            AddNav("Задачи", () => new TasksView());
            AddNav("Статистика", () => new StatsView());

            if (role == UserRole.Admin)
            {
                AddNav("Категории", () => new CategoriesView());
                AddNav("Пользователи", () => new UsersView());
            }

            if (NavPanel.Children.Count > 0)
                ((Button)NavPanel.Children[0]).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        private void AddNav(string title, Func<UserControl> factory)
        {
            var btn = new Button
            {
                Content = title,
                Style = (Style)FindResource("GhostButton"),
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(14, 11, 14, 11),
                Margin = new Thickness(0, 2, 0, 2),
                FontSize = 14
            };
            btn.Click += (s, e) =>
            {
                if (_activeNav != null)
                    _activeNav.Style = (Style)FindResource("GhostButton");
                btn.Style = (Style)FindResource("PrimaryButton");
                _activeNav = btn;
                ContentArea.Content = factory();
            };
            NavPanel.Children.Add(btn);
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            DataStore.Instance.CurrentUser = null;
            var login = new LoginWindow();
            login.Show();
            Close();
        }
    }
}
