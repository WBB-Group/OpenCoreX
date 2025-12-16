using System.Windows;
using OpenCoreX.Dashboard.SplashScreen;

namespace OpenCoreX
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize and show the loading window
            var loadingWindow = new LoadingWindow();
            loadingWindow.Show();
        }
    }
}
