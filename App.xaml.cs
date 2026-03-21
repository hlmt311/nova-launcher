using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
namespace NovaGamingOptimizer
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            DispatcherUnhandledException += (s, ex) =>
            {
                MessageBox.Show($"Error: {ex.Exception.Message}\n\n{ex.Exception.StackTrace}", "Nova Error");
                ex.Handled = true;
            };
            AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
            {
                MessageBox.Show($"Fatal: {ex.ExceptionObject}", "Nova Fatal Error");
            };
            base.OnStartup(e);
            Task.Run(async () => await AutoUpdater.CheckOnStartup());
        }
    }
}
