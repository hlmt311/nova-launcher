using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
namespace NovaGamingOptimizer
{
    public partial class NetworkWindow : Window
    {
        public NetworkWindow() { InitializeComponent(); }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e) { if (e.ChangedButton == MouseButton.Left) DragMove(); }
        private void Close_Click(object sender, RoutedEventArgs e) => Close();
        private void Speed_Click(object sender, RoutedEventArgs e) => _ = SpeedTest();
        private async Task SpeedTest()
        {
            try
            {
                using var c = new HttpClient();
                var sw = Stopwatch.StartNew();
                var d = await c.GetByteArrayAsync("https://speed.cloudflare.com/__down?bytes=25000000");
                sw.Stop();
                double mbps = (d.Length * 8) / (sw.Elapsed.TotalSeconds * 1024 * 1024);
                MessageBox.Show($"Download Speed: {mbps:F2} Mbps");
            }
            catch { MessageBox.Show("Speed test failed."); }
        }
        private void DNS_Click(object sender, RoutedEventArgs e) { try { Cmd("ipconfig", "/flushdns"); Cmd("netsh", "winsock reset"); MessageBox.Show("DNS Flushed + Winsock Reset!"); } catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}"); } }
        private void Throttle_Click(object sender, RoutedEventArgs e) { try { Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "NetworkThrottlingIndex", unchecked((int)0xFFFFFFFF), RegistryValueKind.DWord); MessageBox.Show("Network Throttling Disabled!"); } catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}"); } }
        private void Nagle_Click(object sender, RoutedEventArgs e) { try { Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces", "TcpAckFrequency", 1, RegistryValueKind.DWord); Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces", "TCPNoDelay", 1, RegistryValueKind.DWord); MessageBox.Show("Nagle Algorithm Disabled!"); } catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}"); } }
        private void CF_Click(object sender, RoutedEventArgs e) { try { Cmd("powershell", "Get-NetAdapter | Where-Object {$_.Status -eq 'Up'} | Set-DnsClientServerAddress -ServerAddresses ('1.1.1.1','1.0.0.1')"); MessageBox.Show("DNS set to Cloudflare 1.1.1.1!"); } catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}"); } }
        private void Cmd(string f, string a) { try { using var p = Process.Start(new ProcessStartInfo { FileName = f, Arguments = a, UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true }); p?.WaitForExit(5000); } catch { } }
    }
}
