using System;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
namespace NovaGamingOptimizer
{
    public partial class AutoUpdater : Window
    {
        private const string VERSION_URL = "https://raw.githubusercontent.com/hlmt311/nova-optimizer/main/version.json";
        private const string DOWNLOAD_URL = "https://github.com/hlmt311/nova-optimizer/releases/latest";
        private readonly string _v;

        public AutoUpdater()
        {
            InitializeComponent();
            _v = Ver();
            CurrentVersionText.Text = _v;
            Loaded += async (_, _) => await Check();
        }

        private string Ver()
        {
            var v = Assembly.GetExecutingAssembly().GetName().Version;
            return $"v{v?.Major}.{v?.Minor}.{v?.Build}";
        }

        private async Task Check()
        {
            try
            {
                using var c = new HttpClient(); c.Timeout = TimeSpan.FromSeconds(10);
                var json = await c.GetStringAsync(VERSION_URL);
                var d = JsonSerializer.Deserialize<VersionInfo>(json);
                var latest = d?.version ?? _v;
                LatestVersionText.Text = latest;
                ProgressBar.IsIndeterminate = false;
                ProgressBar.Value = 100;
                if (Version.Parse(latest.TrimStart('v')) > Version.Parse(_v.TrimStart('v')))
                {
                    TitleText.Text = "Update Available!";
                    StatusText.Text = "New Version";
                    StatusText.Foreground = System.Windows.Media.Brushes.LimeGreen;
                    UpdateButton.IsEnabled = true;
                    UpdateButton.Content = $"Download {latest}";
                }
                else
                {
                    TitleText.Text = "Up to Date!";
                    StatusText.Text = "Up to Date";
                    StatusText.Foreground = System.Windows.Media.Brushes.LimeGreen;
                    SkipButton.Content = "Close";
                    await Task.Delay(1500);
                    Close();
                }
            }
            catch { TitleText.Text = "Could not check for updates"; ProgressBar.IsIndeterminate = false; SkipButton.Content = "Close"; }
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        { Process.Start(new ProcessStartInfo { FileName = DOWNLOAD_URL, UseShellExecute = true }); Close(); }

        private void Skip_Click(object sender, RoutedEventArgs e) => Close();

        public static async Task CheckOnStartup()
        {
            try
            {
                using var c = new HttpClient(); c.Timeout = TimeSpan.FromSeconds(8);
                var json = await c.GetStringAsync(VERSION_URL);
                var d = JsonSerializer.Deserialize<VersionInfo>(json);
                var latest = d?.version ?? "";
                var cur = Assembly.GetExecutingAssembly().GetName().Version;
                var cs = $"v{cur?.Major}.{cur?.Minor}.{cur?.Build}";
                if (!string.IsNullOrEmpty(latest) && Version.Parse(latest.TrimStart('v')) > Version.Parse(cs.TrimStart('v')))
                    Application.Current.Dispatcher.Invoke(() => new AutoUpdater().Show());
            }
            catch { }
        }
    }

    public class VersionInfo
    {
        public string version { get; set; } = "";
        public string notes { get; set; } = "";
    }
}
