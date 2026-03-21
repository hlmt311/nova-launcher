using System.Windows;
using System.Threading.Tasks;
namespace NovaGamingOptimizer
{
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            await AutoUpdater.CheckOnStartup();
        }
    }
}
