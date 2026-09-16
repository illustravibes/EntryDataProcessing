using System.Windows;
using Entry_Data_Processing.Features.Dashboard.ViewModels;

namespace Entry_Data_Processing.Features.Dashboard.Views
{
    public partial class DashboardPage : System.Windows.Controls.Page
    {
        private readonly DashboardViewModel _viewModel;

        public DashboardPage(DashboardViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = viewModel;
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.InitializeAsync();
        }
    }
}
