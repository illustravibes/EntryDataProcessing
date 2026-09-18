using System.Windows;
using System.Windows.Input;
using Entry_Data_Processing.Features.RequestKodeBarang.Models;

namespace Entry_Data_Processing.Features.RequestKodeBarang.Views.Dialogs
{
    public partial class ReqKodeDetailDialog : Window
    {
        public string? ActionTaken { get; private set; }
        private readonly Wpf.Ui.ISnackbarService _snackbarService;
        private readonly Wpf.Ui.Controls.SnackbarPresenter? _previousPresenter;

        public ReqKodeDetailDialog(ReqEdpKodeRecord record, Wpf.Ui.ISnackbarService snackbarService)
        {
            InitializeComponent();
            DataContext = record;
            Owner = Application.Current.MainWindow;

            _snackbarService = snackbarService;
            if (Application.Current.MainWindow is MainWindow mainWin)
            {
                _previousPresenter = mainWin.RootSnackbar;
            }

            Loaded += (s, e) =>
            {
                _snackbarService.SetSnackbarPresenter(DialogSnackbar);
            };

            Closed += (s, e) =>
            {
                if (_previousPresenter != null)
                {
                    _snackbarService.SetSnackbarPresenter(_previousPresenter);
                }
            };
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
