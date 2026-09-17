using System.Windows;
using System.Windows.Input;

namespace Entry_Data_Processing.Features.RequestKodeBarang.Views.Dialogs
{
    public partial class RejectReasonDialog : Window
    {
        public string RejectReason { get; private set; } = string.Empty;
        public bool IsConfirmed { get; private set; } = false;

        public RejectReasonDialog()
        {
            InitializeComponent();

            ReasonTextBox.TextChanged += (s, e) =>
            {
                var text = ReasonTextBox.Text;
                var len = text.Length;
                CharCountLabel.Text = $"{len} karakter";
                BtnConfirm.IsEnabled = len > 0;
            };

            Loaded += (_, _) => ReasonTextBox.Focus();
        }

        private void OnConfirmClicked(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ReasonTextBox.Text)) return;
            RejectReason = ReasonTextBox.Text.Trim();
            IsConfirmed = true;
            Close();
        }

        private void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            IsConfirmed = false;
            Close();
        }

        private void OnCloseClicked(object sender, RoutedEventArgs e)
        {
            IsConfirmed = false;
            Close();
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
