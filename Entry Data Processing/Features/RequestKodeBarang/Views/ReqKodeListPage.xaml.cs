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
            
            Loaded += (_, _) => _viewModel.InitializeCommand.Execute(null);
        }

        private void DataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
}
