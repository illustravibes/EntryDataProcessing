using System.Windows;
using Entry_Data_Processing.Features.Auth.ViewModels;
using Entry_Data_Processing;
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
            viewModel.LoginSucceeded += OnLoginSucceeded;
            Loaded += OnLoaded;
        }

        private void OnLoginSucceeded(object? sender, EventArgs e)
        {
            var mainWindow = App.GetService<MainWindow>();
            Application.Current.MainWindow = mainWindow;
            mainWindow.Show();
            Close();
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _snackbarService.SetSnackbarPresenter(RootSnackbar);
        }
    }
}
