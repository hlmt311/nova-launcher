using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NovaGamingOptimizer
{
    public class GameInfo
    {
        public string Name { get; set; } = "";
        public string Path { get; set; } = "";
        public string Size { get; set; } = "";
        public string Engine { get; set; } = "";
        public string Source { get; set; } = "";
    }

    public partial class GameDetector : Window
    {
        private ObservableCollection<GameInfo> _games = new();
        private GameInfo? _selected;

        public GameDetector()
        {
            InitializeComponent();
            GamesList.ItemsSource = _games;
            _ = Scan();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        { if (e.ChangedButton == MouseButton.Left) DragMove(); }

        private void MinimizeWindow(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void MaximizeWindow(object sender, RoutedEventArgs e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        private void CloseWindow(object sender, RoutedEventArgs e) => Close();

        private void OpenDashboard_Click(object sender, RoutedEventArgs e) => new DashboardOverlay().Show();
        private void OpenNetwork_Click(object sender, RoutedEventArgs e) => new NetworkWindow().Show();
        private void OpenMouse_Click(object sender, RoutedEventArgs e) => new MouseWindow().Show();
        private void OpenKeyboard_Click(object sender, RoutedEventArgs e) => new KeyboardWindow().Show();
        private void OpenAudio_Click(object sender, RoutedEventArgs e) => new AudioWindow().Show();
        private void OpenCrosshair_Click(object sender, RoutedEventArgs e) => new CrosshairWindow().Show();

        private void Discord_Click(object sender, RoutedEventArgs e)
        { try { Process.Start(new ProcessStartInfo { FileName = "https://discord.gg/rWBRmHAz", UseShellExecute = true }); } catch { } }

        private void PayPal_Click(object sender, RoutedEventArgs e)
        { try { Process.Start(new ProcessStartInfo { FileName = "https://www.paypal.me/AdamJaber348", UseShellExecute = true }); } catch { } }

        private void OpenWebButton_Click(object sender, RoutedEventArgs e)
        { try { Process.Start(new ProcessStartInfo { FileName = "http://localhost:3000", UseShellExecute = true }); } catch { } }

        private void RefreshButton_Click(object sender, RoutedEventArgs e) => _ = Scan();

        private void LaunchButton_Click(object sender, RoutedEventArgs e)
        { if (_selected != null && File.Exists(_selected.Path)) Process.Start(new ProcessStartInfo { FileName = _selected.Path, UseShellExecute = true }); }

        private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        { if (_selected != null) Process.Start("explorer.exe", Path.GetDirectoryName(_selected.Path)!); }

        private void GamesList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            _selected = GamesList.SelectedItem as GameInfo;
            if (_selected == null) return;
            DetailName.Text = _selected.Name;
            DetailPath.Text = _selected.Path;
            DetailSize.Text = _selected.Size;
            DetailEngine.Text = _selected.Engine;
            LaunchButton.IsEnabled = true;
            OpenFolderButton.IsEnabled = true;
        }

        private async Task Scan()
        {
            try
            {
                var all = ScanSteam().Concat(ScanDirs()).Concat(ScanReg())
                    .GroupBy(g => g.Name).Select(g => g.First()).OrderBy(g => g.Name).ToList();
                _games.Clear();
                foreach (var g in all) _games.Add(g);
                GamesCountLabel.Text = $"Total: {all.Count} games";
            }
            catch { }
        }

        private List<GameInfo> ScanSteam()
        {
            var list = new List<GameInfo>();
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam", "steamapps", "common");
            if (!Directory.Exists(path)) return list;
            foreach (var dir in Directory.GetDirectories(path).Take(80))
                try
                {
                    var exe = Directory.GetFiles(dir, "*.exe", SearchOption.TopDirectoryOnly)
                        .Where(f => !Path.GetFileName(f).ToLower().Contains("uninstall") && !Path.GetFileName(f).ToLower().Contains("setup"))
                        .OrderByDescending(f => new FileInfo(f).Length).FirstOrDefault();
                    if (exe != null)
                        list.Add(new GameInfo { Name = Path.GetFileName(dir), Path = exe, Size = Fmt(new FileInfo(exe).Length), Engine = Eng(Path.GetFileName(dir)), Source = "Steam" });
                }
                catch { }
            return list;
        }

        private List<GameInfo> ScanDirs()
        {
            var list = new List<GameInfo>();
            var launchers = new[] { "Epic Games", "Ubisoft", "EA Games", "Rockstar Games", "GOG Galaxy", "Riot Games" };
            foreach (var root in new[] { Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) })
                foreach (var l in launchers)
                {
                    var p = Path.Combine(root, l);
                    if (!Directory.Exists(p)) continue;
                    foreach (var dir in Directory.GetDirectories(p).Take(20))
                        try
                        {
                            var exe = Directory.GetFiles(dir, "*.exe", SearchOption.TopDirectoryOnly)
                                .Where(f => !Path.GetFileName(f).ToLower().Contains("uninstall"))
                                .OrderByDescending(f => new FileInfo(f).Length).FirstOrDefault();
                            if (exe == null) continue;
                            var name = FileVersionInfo.GetVersionInfo(exe).ProductName ?? Path.GetFileName(dir);
                            if (!string.IsNullOrEmpty(name))
                                list.Add(new GameInfo { Name = name.Trim(), Path = exe, Size = Fmt(new FileInfo(exe).Length), Engine = Eng(name), Source = l });
                        }
                        catch { }
                }
            return list;
        }

        private List<GameInfo> ScanReg()
        {
            var list = new List<GameInfo>();
            try
            {
                var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
                if (key == null) return list;
                foreach (var name in key.GetSubKeyNames().Take(200))
                    try
                    {
                        var sub = key.OpenSubKey(name);
                        var dn = sub?.GetValue("DisplayName") as string;
                        var loc = sub?.GetValue("InstallLocation") as string;
                        if (string.IsNullOrEmpty(dn) || string.IsNullOrEmpty(loc) || !Directory.Exists(loc)) continue;
                        if (!new[] { "game", "steam", "epic", "ubisoft", "ea ", "riot" }.Any(k => dn.ToLower().Contains(k))) continue;
                        var exe = Directory.GetFiles(loc, "*.exe").Where(f => !Path.GetFileName(f).ToLower().Contains("uninstall")).FirstOrDefault();
                        if (exe != null)
                            list.Add(new GameInfo { Name = dn, Path = exe, Size = Fmt(new FileInfo(exe).Length), Engine = Eng(dn), Source = "Registry" });
                    }
                    catch { }
            }
            catch { }
            return list;
        }

        private string Eng(string n)
        {
            n = n.ToLower();
            if (n.Contains("valorant") || n.Contains("league")) return "Riot Custom";
            if (n.Contains("fortnite")) return "Unreal Engine 5";
            if (n.Contains("cs2") || n.Contains("counter")) return "Source 2";
            if (n.Contains("gta")) return "RAGE";
            if (n.Contains("minecraft")) return "Java/Bedrock";
            if (n.Contains("cyberpunk")) return "REDengine 4";
            if (n.Contains("elden")) return "Havok";
            return "Unknown";
        }

        private string Fmt(long b)
        {
            string[] s = { "B", "KB", "MB", "GB" };
            double v = b; int i = 0;
            while (v >= 1024 && i < 3) { v /= 1024; i++; }
            return $"{v:0.#} {s[i]}";
        }
    }
}
