using System.Windows;
using Entry_Data_Processing.Features.RequestKodeBarang.ViewModels;

namespace Entry_Data_Processing.Features.RequestKodeBarang.Views
{
    public partial class ReqKodeListPage : System.Windows.Controls.Page
    {
        private readonly ReqKodeListViewModel _viewModel;

        public ReqKodeListPage(ReqKodeListViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = viewModel;
            
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _viewModel.LoadDataCommand.Execute(null);
        }
    }
}
