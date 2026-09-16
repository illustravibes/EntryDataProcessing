using System.Windows;
using Entry_Data_Processing.Features.RequestKodeBarang.ViewModels;

namespace Entry_Data_Processing.Features.RequestKodeBarang.Views
{
    public partial class ReqKodeDetailPage : System.Windows.Controls.Page
    {
        public ReqKodeDetailPage(ReqKodeDetailViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
