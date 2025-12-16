using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace OpenCoreX.Dashboard.SplashScreen
{
    public partial class LoadingWindow : Window
    {
        public LoadingWindow()
        {
            InitializeComponent();
            Loaded += LoadingWindow_Loaded;
        }

        private async void LoadingWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Simulate high FPS loading
            var duration = TimeSpan.FromSeconds(3); // Total load time
            
            var animation = new DoubleAnimation
            {
                From = 0,
                To = 350, // Width of the grid container
                Duration = duration,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };
            
            ProgressIndicator.BeginAnimation(FrameworkElement.WidthProperty, animation);
            
            // Status text updates synchronized with the animation time
            await UpdateStatus("Initializing core modules...", 600);
            await UpdateStatus("Loading heuristic engine...", 800);
            await UpdateStatus("Verifying integrity signatures...", 800);
            await UpdateStatus("Preparing dashboard...", 600);

            await Task.Delay(200); // Tiny pause at 100%

            // Open Main Window
            var mainWindow = new DashboardWindow();
            Application.Current.MainWindow = mainWindow;
            mainWindow.Show();
            this.Close();
        }

        private async Task UpdateStatus(string text, int delay)
        {
            StatusText.Text = text;
            await Task.Delay(delay);
        }
    }
}
