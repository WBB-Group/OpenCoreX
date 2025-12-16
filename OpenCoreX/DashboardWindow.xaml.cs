using System.Windows;
using OpenCoreX.Dashboard.Functions;

namespace OpenCoreX;

public partial class DashboardWindow : Window
{
    private readonly DashboardViewModel _viewModel;

    public DashboardWindow()
    {
        InitializeComponent();
        
        _viewModel = new DashboardViewModel();
        DataContext = _viewModel;
    }

    private void NavButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button button && button.Tag != null)
        {
            if (int.TryParse(button.Tag.ToString(), out int index))
            {
                MainTabs.SelectedIndex = index;
            }
        }
    }

    private void TitleBar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
        {
            this.DragMove();
        }
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        if (this.WindowState == WindowState.Maximized)
            this.WindowState = WindowState.Normal;
        else
            this.WindowState = WindowState.Maximized;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _viewModel.Dispose();
    }

    private async void ApplyDebloat_Click(object sender, RoutedEventArgs e)
    {
        // Show Loading Overlay
        LoadingOverlay.Visibility = Visibility.Visible;
        LoadingStatus.Text = "Generating optimization script...";
        
        // Reset Terminal


        // Collect Options
        var options = new DebloatOptions
        {
            RemoveOneDrive = chkOneDrive.IsChecked == true,
            RemoveCortana = chkCortana.IsChecked == true,
            RemoveXbox = chkXbox.IsChecked == true,
            RemoveSkype = chkSkype.IsChecked == true,
            RemoveWeather = chkWeather.IsChecked == true,
            RemoveNews = chkNews.IsChecked == true,
            RemoveFeedback = chkFeedback.IsChecked == true,
            RemoveGetHelp = chkGetHelp.IsChecked == true,
            RemoveTips = chkTips.IsChecked == true,
            RemoveMaps = chkMaps.IsChecked == true,
            RemoveSolitaire = chkSolitaire.IsChecked == true,
            RemovePeople = chkPeople.IsChecked == true,
            RemoveYourPhone = chkYourPhone.IsChecked == true,
            RemovePhotos = chkPhotos.IsChecked == true,
            RemoveCalculator = chkCalculator.IsChecked == true,

            DisableTelemetry = chkTelemetry.IsChecked == true,
            DisableAds = chkAds.IsChecked == true,
            DisableLocation = chkLocation.IsChecked == true,
            DisableCortanaVoice = chkCortanaVoice.IsChecked == true,
            DisableErrorReporting = chkErrorReporting.IsChecked == true,
            DisableFeedbackNotif = chkFeedbackNotif.IsChecked == true,
            RestrictBackgroundApps = chkBackgroundApps.IsChecked == true,
            DisableStartSuggestions = chkStartSuggestions.IsChecked == true,

            EnableUltimatePerformance = chkPowerPlan.IsChecked == true,
            DisableGameDVR = chkGameDVR.IsChecked == true,
            DisableHibernation = chkHibernation.IsChecked == true,
            DisableVisualEffects = chkVisualEffects.IsChecked == true,
            DisableMouseAccel = chkMouseAccel.IsChecked == true,
            DisableStickyKeys = chkStickyKeys.IsChecked == true,
            OptimizeNetwork = chkNetworkThrottling.IsChecked == true
        };

        // Initialize Service
        var service = new DebloaterService();
        string script = service.GenerateScript(options);

        // Animate Progress Bar (Fake animation for UX)
        var animation = new System.Windows.Media.Animation.DoubleAnimation
        {
            From = 0,
            To = 350, 
            Duration = TimeSpan.FromSeconds(2),
            EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseInOut }
        };
        LoadingProgress.BeginAnimation(FrameworkElement.WidthProperty, animation);

        LoadingStatus.Text = "Executing optimizations...";
        
        // Execute Script
        await service.ExecuteScriptAsync(script, (output) =>
        {
            // Update UI on UI Thread
            Dispatcher.Invoke(() =>
            {

            });
        });

        // Hide Overlay after short delay
        LoadingStatus.Text = "Finalizing...";
        await Task.Delay(500);
        LoadingOverlay.Visibility = Visibility.Collapsed;
        
        // Reset Progress Bar
        LoadingProgress.BeginAnimation(FrameworkElement.WidthProperty, null);
        LoadingProgress.Width = 0;
    }
}
