using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace Entry_Data_Processing.Features.RequestKodeBarang.Views.Dialogs
{
    public partial class RejectReasonDialog : ContentDialog
    {
        public string RejectReason => ReasonTextBox.Text;

        public RejectReasonDialog(ContentPresenter dialogHost) : base(dialogHost)
        {
            InitializeComponent();
        }
    }
}
