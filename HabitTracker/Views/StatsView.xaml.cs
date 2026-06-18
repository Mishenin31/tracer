using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using QRCoder;
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

            var report = $"Пользователь: {DataStore.Instance.CurrentUser!.FullName}\n" +
                         $"Дата отчёта: {DateTime.Now:dd.MM.yyyy HH:mm}\n" +
                         $"Всего привычек: {habits.Count}\n" +
                         $"Отметок за неделю: {weekChecks}\n" +
                         $"Задач выполнено: {tasks.Count(t => t.IsDone)} / {tasks.Count}\n" +
                         $"Лучшая серия: {best} дн.\n" +
                         $"Хранилище: {DataStore.Instance.StorageInfo}";
            ReportQrImage.Source = CreateQrImage(report);
            ReportQrText.Text = "QR отчёта: сводка статистики и источник данных";
        }

        private static BitmapImage CreateQrImage(string text)
        {
            using var generator = new QRCodeGenerator();
            using var qrData = generator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            var png = new PngByteQRCode(qrData);
            var bytes = png.GetGraphic(12);

            using var stream = new MemoryStream(bytes);
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = stream;
            image.EndInit();
            image.Freeze();
            return image;
        }
    }
}
