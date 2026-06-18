using System.Windows;
using System.Windows.Controls;
using HabitTracker.Data;
using HabitTracker.Models;

namespace HabitTracker.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void ShowLogin_Click(object sender, RoutedEventArgs e)
        {
            LoginPanel.Visibility = Visibility.Visible;
            RegPanel.Visibility = Visibility.Collapsed;
            TabLoginBtn.Style = (Style)FindResource("PrimaryButton");
            TabRegBtn.Style = (Style)FindResource("GhostButton");
        }

        private void ShowReg_Click(object sender, RoutedEventArgs e)
        {
            LoginPanel.Visibility = Visibility.Collapsed;
            RegPanel.Visibility = Visibility.Visible;
            TabRegBtn.Style = (Style)FindResource("PrimaryButton");
            TabLoginBtn.Style = (Style)FindResource("GhostButton");
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            LoginError.Visibility = Visibility.Collapsed;
            var login = LoginLogin.Text.Trim();
            var pass = LoginPassword.Password;

            var user = DataStore.Instance.Users.FirstOrDefault(u => u.Login == login && u.Password == pass);
            if (user == null)
            {
                LoginError.Text = "Неверный логин или пароль";
                LoginError.Visibility = Visibility.Visible;
                return;
            }

            DataStore.Instance.CurrentUser = user;
            var main = new MainWindow();
            main.Show();
            Close();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            RegError.Visibility = Visibility.Collapsed;
            var name = RegName.Text.Trim();
            var login = RegLogin.Text.Trim();
            var pass = RegPassword.Password;

            if (name.Length == 0 || login.Length == 0 || pass.Length == 0)
            {
                RegError.Text = "Заполните все поля";
                RegError.Visibility = Visibility.Visible;
                return;
            }
            if (DataStore.Instance.Users.Any(u => u.Login == login))
            {
                RegError.Text = "Логин уже занят";
                RegError.Visibility = Visibility.Visible;
                return;
            }

            var user = new User
            {
                Id = DataStore.Instance.NextUserId(),
                Login = login,
                Password = pass,
                FullName = name,
                Role = UserRole.User
            };
            DataStore.Instance.AddUser(user);
            DataStore.Instance.CurrentUser = user;

            var main = new MainWindow();
            main.Show();
            Close();
        }
    }
}
