using System.Windows;
using OpenCoreX.Dashboard.Functions;

namespace OpenCoreX;

public partial class DashboardWindow : Window
{
    private readonly DashboardViewModel _viewModel;
    private IntegrityService _integrityService;
    private InstallerService _installerService;

    public DashboardWindow()
    {
        InitializeComponent();
        
        _viewModel = new DashboardViewModel();
        DataContext = _viewModel;
        _integrityService = new IntegrityService();
        _installerService = new InstallerService();

        InitializeInstallerTab();
    }

    private void InitializeInstallerTab()
    {
        InstallerList.ItemsSource = _installerService.GetPrograms();
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

    public void AnimateIn()
    {
        var fadeIn = (System.Windows.Media.Animation.Storyboard)this.Resources["FadeInAnimation"];
        fadeIn.Begin(this);
    }

    private async void StartRepair_Click(object sender, RoutedEventArgs e)
    {
        IntegrityProgressPanel.Visibility = Visibility.Visible;
        (sender as System.Windows.Controls.Button).IsEnabled = false;

        _integrityService.StatusUpdated += (s, msg) => Dispatcher.Invoke(() => IntegrityStatusText.Text = msg);
        _integrityService.ProgressUpdated += (s, progress) => Dispatcher.Invoke(() => 
        {
            // Max width is tricky here without knowing the exact pixel width of the container
            // We can bind Width to parent ActualWidth * percentage
            // For now, let's assume a fixed max width or just update a standard ProgressBar if I had used one.
            // But I used a custom Border implementation.
            // Let's check the Border implementation in XAML... it is inside a Grid with no width specified, so it stretches. 
            // The container (Border "IntegrityProgressBar") has Width="0" initially.
            // The parent Grid has no explicit width, so it takes available space.
            // We need to calculate the width.
            
            // Actually, simpler approach for this custom progress bar:
            // Get the parent ActualWidth and set the Width of the indicator
            if (IntegrityProgressBar.Parent is System.Windows.Controls.Grid parent)
            {
                 IntegrityProgressBar.Width = parent.ActualWidth * (progress / 100.0);
            }
        });

        try 
        {
            await _integrityService.StartRepairAsync();
            IntegrityStatusText.Text = "Repair Completed.";
        }
        catch (Exception ex)
        {
            IntegrityStatusText.Text = "Error: " + ex.Message;
        }
        finally
        {
            (sender as System.Windows.Controls.Button).IsEnabled = true;
        }
    }

    private async void InstallProgram_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button btn && btn.Tag is InstallableProgram program)
        {
             btn.IsEnabled = false;
             btn.Content = "Installing...";
             
             var progress = new Progress<string>(status => 
             {
                 InstallerStatusText.Text = status;
             });

             await _installerService.InstallProgramAsync(program, progress);

             btn.Content = "Installed";
             btn.IsEnabled = true;
        }
    }
}
