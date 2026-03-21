using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;
namespace NovaGamingOptimizer
{
    public partial class MouseWindow : Window
    {
        public MouseWindow() { InitializeComponent(); }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e) { if (e.ChangedButton == MouseButton.Left) DragMove(); }
        private void Close_Click(object sender, RoutedEventArgs e) => Close();
        private void Fix_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseSpeed", "0", RegistryValueKind.String);
                Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseThreshold1", "0", RegistryValueKind.String);
                Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseThreshold2", "0", RegistryValueKind.String);
                MessageBox.Show("Mouse Acceleration Disabled! Restart recommended.");
            }
            catch (System.Exception ex) { MessageBox.Show($"Error: {ex.Message}"); }
        }
    }
}
