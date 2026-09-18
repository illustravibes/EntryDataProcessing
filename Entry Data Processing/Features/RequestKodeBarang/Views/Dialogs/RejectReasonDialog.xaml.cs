using System.Windows;
using System.Windows.Input;
using Entry_Data_Processing.Features.RequestKodeBarang.ViewModels;

namespace Entry_Data_Processing.Features.RequestKodeBarang.Views.Dialogs
{
    public partial class RejectReasonDialog : Window
    {
        private readonly RejectReasonViewModel _viewModel;

        public string RejectReason => _viewModel.Reason;
        public bool IsConfirmed { get; private set; } = false;

        public RejectReasonDialog(RejectReasonViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = viewModel;
            _viewModel.Confirmed += OnConfirmed;
            _viewModel.Cancelled += OnCancelled;

            Loaded += (_, _) => ReasonTextBox.Focus();
        }

        private void OnConfirmed(object? sender, EventArgs e)
        {
            IsConfirmed = true;
            Close();
        }

        private void OnCancelled(object? sender, EventArgs e)
        {
            IsConfirmed = false;
            Close();
        }

        private void OnCloseClicked(object sender, RoutedEventArgs e)
        {
            _viewModel.CancelCommand.Execute(null);
        }

        private void OnWindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                IsConfirmed = false;
                Close();
            }
        }
    }
}
