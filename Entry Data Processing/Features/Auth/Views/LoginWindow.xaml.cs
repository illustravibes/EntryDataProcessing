using System.Windows;
using Entry_Data_Processing.Features.Auth.ViewModels;
using Wpf.Ui;

namespace Entry_Data_Processing.Features.Auth.Views
{
    public partial class LoginWindow : Wpf.Ui.Controls.FluentWindow
    {
        private readonly ISnackbarService _snackbarService;

        public LoginWindow(LoginViewModel viewModel, ISnackbarService snackbarService)
        {
            InitializeComponent();
            DataContext = viewModel;
            _snackbarService = snackbarService;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _snackbarService.SetSnackbarPresenter(RootSnackbar);
        }
    }
}
