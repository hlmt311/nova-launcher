using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace NovaGamingOptimizer
{
    public partial class DashboardOverlay : Window
    {
        private DispatcherTimer _timer = new();
        private PerformanceCounter? _cpu;
        private PerformanceCounter? _ram;
        private Color _accent = Colors.LimeGreen;
        private readonly string[] _gn = { "fortnite", "valorant", "csgo", "cs2", "apex", "warzone", "minecraft", "gta5", "lol" };

        public DashboardOverlay()
        {
            InitializeComponent();
            try
            {
                _cpu = new PerformanceCounter("Processor", "% Processor Time", "_Total", true);
                _ram = new PerformanceCounter("Memory", "% Committed Bytes In Use", null!, true);
            }
            catch { }
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += async (s, e) => await UpdateStats();
            _timer.Start();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) { if (e.ChangedButton == MouseButton.Left) DragMove(); }
        private void CloseOverlay(object sender, RoutedEventArgs e) { _timer.Stop(); Close(); }

        private async Task UpdateStats()
        {
            float cpu = 0, ram = 0, gpu = 0, temp = 0; int ping = 0; string game = "";
            await Task.Run(() =>
            {
                try { if (_cpu != null) cpu = Math.Min(100, _cpu.NextValue()); } catch { }
                try { if (_ram != null) ram = Math.Min(100, _ram.NextValue()); } catch { }
                try { var r = new Ping().Send("8.8.8.8", 500); if (r?.Status == IPStatus.Success) ping = (int)r.RoundtripTime; } catch { }
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "nvidia-smi",
                        Arguments = "--query-gpu=utilization.gpu,temperature.gpu --format=csv,noheader,nounits",
                        UseShellExecute = false, RedirectStandardOutput = true, CreateNoWindow = true
                    };
                    using var p = Process.Start(psi);
                    if (p != null)
                    {
                        var o = p.StandardOutput.ReadToEnd().Trim().Split(',');
                        if (o.Length >= 2) { float.TryParse(o[0].Trim(), out gpu); float.TryParse(o[1].Trim(), out temp); }
                    }
                }
                catch { }
                try { foreach (var proc in Process.GetProcesses()) { if (_gn.Any(g => proc.ProcessName.ToLower().Contains(g))) { game = proc.ProcessName; break; } } } catch { }
            });
            CpuText.Text = $"{cpu:F0}%"; CpuBar.Value = cpu;
            GpuText.Text = gpu > 0 ? $"{gpu:F0}%" : "N/A"; GpuBar.Value = gpu;
            RamText.Text = $"{ram:F0}%"; RamBar.Value = ram;
            PingText.Text = ping > 0 ? $"{ping}ms" : "N/A"; PingBar.Value = Math.Min(ping, 300);
            TempText.Text = temp > 0 ? $"{temp:F0}C" : "N/A"; TempBar.Value = Math.Min(temp, 100);
            FpsText.Text = game != "" ? "Active" : "-- fps";
            FpsText.Foreground = new SolidColorBrush(_accent);
            GameText.Text = game != "" ? game : "No game detected";
        }

        private void HexInput_Changed(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            try
            {
                string hex = HexInput.Text.Trim();
                if (!hex.StartsWith("#")) hex = "#" + hex;
                if (hex.Length == 7) { _accent = (Color)ColorConverter.ConvertFromString(hex); ColorDot.Background = new SolidColorBrush(_accent); }
            }
            catch { }
        }
    }
}
