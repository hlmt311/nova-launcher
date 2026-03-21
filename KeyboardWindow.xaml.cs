using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;
namespace NovaGamingOptimizer
{
    public partial class KeyboardWindow : Window
    {
        public KeyboardWindow() { InitializeComponent(); }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e) { if (e.ChangedButton == MouseButton.Left) DragMove(); }
        private void Close_Click(object sender, RoutedEventArgs e) => Close();
        private void Rate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Keyboard", "KeyboardDelay", "0", RegistryValueKind.String);
                Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Keyboard", "KeyboardSpeed", "31", RegistryValueKind.String);
                MessageBox.Show("Keyboard Rate Optimized!");
            }
            catch (System.Exception ex) { MessageBox.Show($"Error: {ex.Message}"); }
        }
        private void Sticky_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Accessibility\StickyKeys", "Flags", "506", RegistryValueKind.String);
                Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Accessibility\ToggleKeys", "Flags", "58", RegistryValueKind.String);
                Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Accessibility\Keyboard Response", "Flags", "122", RegistryValueKind.String);
                MessageBox.Show("Accessibility Keys Disabled!");
            }
            catch (System.Exception ex) { MessageBox.Show($"Error: {ex.Message}"); }
        }
    }
}
