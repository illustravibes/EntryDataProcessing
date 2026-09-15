using Entry_Data_Processing.Data;
using Entry_Data_Processing.Services;
using Entry_Data_Processing.ViewModels;
using Entry_Data_Processing.Views;
using System.Windows;

namespace Entry_Data_Processing
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            const string connectionString = "Server=localhost;Database=edp;User ID=root;Password=CHANGE_ME;";
            var session = new SessionService();
            var authService = new AuthService(new DbConnectionFactory(connectionString));
            var loginWindow = new LoginWindow(new LoginViewModel(authService, session));

            if (loginWindow.ShowDialog() == true)
            {
                var mainWindow = new MainWindow();
                MainWindow = mainWindow;
                ShutdownMode = ShutdownMode.OnMainWindowClose;
                mainWindow.Show();
            }
            else
            {
                Shutdown();
            }
        }
    }

}
