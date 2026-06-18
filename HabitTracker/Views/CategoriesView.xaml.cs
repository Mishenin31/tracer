using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using HabitTracker.Data;
using HabitTracker.Models;

namespace HabitTracker.Views
{
    public partial class CategoriesView : UserControl
    {
        public CategoriesView()
        {
            InitializeComponent();
            Grid.ItemsSource = DataStore.Instance.Categories;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var name = NameBox.Text.Trim();
            var color = ColorBox.Text.Trim();

            if (name.Length == 0)
            {
                MessageBox.Show("Введите название категории", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!Regex.IsMatch(color, "^#([A-Fa-f0-9]{6})$"))
            {
                MessageBox.Show("Цвет должен быть в формате #RRGGBB", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataStore.Instance.AddCategory(new Category
            {
                Name = name,
                ColorHex = color
            });

            NameBox.Clear();
            ColorBox.Text = "#7C6BAD";
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.Tag is Category c)
            {
                if (DataStore.Instance.Habits.Any(h => h.Category == c))
                {
                    MessageBox.Show("Нельзя удалить категорию: есть привычки, которые её используют.",
                        "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (MessageBox.Show($"Удалить категорию «{c.Name}»?", "Подтверждение",
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    DataStore.Instance.DeleteCategory(c);
                }
            }
        }
    }
}
