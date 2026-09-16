using System.Windows;
using System.Windows.Input;
using Entry_Data_Processing.Features.RequestKodeBarang.Models;

namespace Entry_Data_Processing.Features.RequestKodeBarang.Views.Dialogs
{
    public partial class ReqKodeDetailDialog : Window
    {
        public string? ActionTaken { get; private set; }

        public ReqKodeDetailDialog(ReqEdpKodeRecord record)
        {
            InitializeComponent();
            DataContext = record;
        }

        private void OnHeaderMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void OnWindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                ActionTaken = "Close";
                Close();
            }
        }

        private void OnApproveClicked(object sender, RoutedEventArgs e)
        {
            ActionTaken = "Approve";
            Close();
        }

        private void OnRejectClicked(object sender, RoutedEventArgs e)
        {
            ActionTaken = "Reject";
            Close();
        }

        private void OnCloseClicked(object sender, RoutedEventArgs e)
        {
            ActionTaken = "Close";
            Close();
        }
    }
}
