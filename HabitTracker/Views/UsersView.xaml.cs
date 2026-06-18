using System.Windows;
using System.Windows.Controls;
using HabitTracker.Data;
using HabitTracker.Models;

namespace HabitTracker.Views
{
    public partial class UsersView : UserControl
    {
        private class RoleOption
        {
            public UserRole Role { get; set; }
            public string Display { get; set; } = "";
            public override string ToString() => Display;
        }

        public UsersView()
        {
            InitializeComponent();

            RoleCombo.ItemsSource = new[]
            {
                new RoleOption { Role = UserRole.User, Display = "Пользователь" },
                new RoleOption { Role = UserRole.Admin, Display = "Администратор" }
            };
            RoleCombo.DisplayMemberPath = "Display";
            RoleCombo.SelectedIndex = 0;

            Grid.ItemsSource = DataStore.Instance.Users;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var name = NameBox.Text.Trim();
            var login = LoginBox.Text.Trim();
            var pass = PassBox.Text.Trim();

            if (name.Length == 0 || login.Length == 0 || pass.Length == 0)
            {
                MessageBox.Show("Заполните все поля", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (DataStore.Instance.Users.Any(u => u.Login == login))
            {
                MessageBox.Show("Логин уже занят", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var role = ((RoleOption)RoleCombo.SelectedItem).Role;
            DataStore.Instance.AddUser(new User
            {
                FullName = name,
                Login = login,
                Password = pass,
                Role = role
            });

            NameBox.Clear();
            LoginBox.Clear();
            PassBox.Clear();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.Tag is User u)
            {
                if (u.Id == DataStore.Instance.CurrentUser!.Id)
                {
                    MessageBox.Show("Нельзя удалить собственную учётную запись.",
                        "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (MessageBox.Show($"Удалить пользователя «{u.FullName}»?", "Подтверждение",
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    DataStore.Instance.DeleteUser(u);
                }
            }
        }
    }
}
