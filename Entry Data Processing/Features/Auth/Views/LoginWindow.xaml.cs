using System.Windows;
using Entry_Data_Processing.Features.Auth.ViewModels;
using Entry_Data_Processing;

namespace Entry_Data_Processing.Features.Auth.Views
{
    public partial class LoginWindow : Wpf.Ui.Controls.FluentWindow
    {
        public LoginWindow(LoginViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.LoginSucceeded += OnLoginSucceeded;
        }

        private void OnLoginSucceeded(object? sender, EventArgs e)
        {
            var mainWindow = App.GetService<MainWindow>();
            Application.Current.MainWindow = mainWindow;
            mainWindow.Show();
            Hide();
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }
    }
}
