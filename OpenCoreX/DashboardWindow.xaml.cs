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

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _viewModel.Dispose();
    }
}
