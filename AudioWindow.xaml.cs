using Microsoft.Win32;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
namespace NovaGamingOptimizer
{
    public partial class AudioWindow : Window
    {
        public AudioWindow() { InitializeComponent(); }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e) { if (e.ChangedButton == MouseButton.Left) DragMove(); }
        private void Close_Click(object sender, RoutedEventArgs e) => Close();
        private void Enhance_Click(object sender, RoutedEventArgs e) { try { Cmd("powershell", @"Get-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\MMDevices\Audio\Render\*\FxProperties' -ErrorAction SilentlyContinue | ForEach-Object { Set-ItemProperty -Path $_.PSPath -Name '{1da5d803-d492-4edd-8c23-e0c0ffee7f0e},5' -Value 1 -ErrorAction SilentlyContinue }"); MessageBox.Show("Audio Enhancements Disabled!"); } catch (System.Exception ex) { MessageBox.Show($"Error: {ex.Message}"); } }
        private void Exclusive_Click(object sender, RoutedEventArgs e) { try { Cmd("powershell", @"Get-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\MMDevices\Audio\Render\*\Properties' -ErrorAction SilentlyContinue | ForEach-Object { Set-ItemProperty -Path $_.PSPath -Name '{b3f8fa53-0004-438e-9003-51a46e139bfc},3' -Value 1 -ErrorAction SilentlyContinue }"); MessageBox.Show("Exclusive Mode Enabled!"); } catch (System.Exception ex) { MessageBox.Show($"Error: {ex.Message}"); } }
        private void Boost_Click(object sender, RoutedEventArgs e) { try { int k = 0; foreach (var a in new[] { "NvBroadcast", "Nahimic3", "NahimicSvc" }) try { foreach (var p in Process.GetProcessesByName(a)) { p.Kill(); k++; } } catch { } Cmd("powershell", "Restart-Service -Name Audiosrv -Force"); Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "Priority", 6, RegistryValueKind.DWord); MessageBox.Show($"Audio Booster Done! Killed {k} apps."); } catch (System.Exception ex) { MessageBox.Show($"Error: {ex.Message}"); } }
        private void Restart_Click(object sender, RoutedEventArgs e) { try { Cmd("powershell", "Restart-Service -Name Audiosrv -Force"); MessageBox.Show("Audio Service Restarted!"); } catch (System.Exception ex) { MessageBox.Show($"Error: {ex.Message}"); } }
        private void Popping_Click(object sender, RoutedEventArgs e) { try { Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "NetworkThrottlingIndex", unchecked((int)0xFFFFFFFF), RegistryValueKind.DWord); MessageBox.Show("Audio Popping Fixed!"); } catch (System.Exception ex) { MessageBox.Show($"Error: {ex.Message}"); } }
        private void Cmd(string f, string a) { try { using var p = Process.Start(new ProcessStartInfo { FileName = f, Arguments = a, UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true }); p?.WaitForExit(5000); } catch { } }
    }
}
