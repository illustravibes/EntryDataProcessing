using System.Windows;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Entry_Data_Processing.Core.Configuration;
using Entry_Data_Processing.Core.Data;
using Entry_Data_Processing.Core.Navigation;
using Entry_Data_Processing.Core.Security;
using Entry_Data_Processing.Core.Session;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace Entry_Data_Processing
{
    public partial class App : Application
    {
        private static IHost? _host;

        public static IHost Host => _host ??= Program.CreateHostBuilder(System.Environment.GetCommandLineArgs()).Build();

        public static T GetService<T>() where T : class
        {
            return (Host.Services.GetService(typeof(T)) as T)!;
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            DispatcherUnhandledException += (s, args) =>
            {
                System.Diagnostics.Debug.WriteLine(args.Exception);

                var snackbarService = GetService<ISnackbarService>();
                if (snackbarService.GetSnackbarPresenter() is not null)
                {
                    snackbarService.Show(
                        "Terjadi kesalahan",
                        "Aplikasi mengalami masalah. Silakan coba lagi.",
                        ControlAppearance.Danger,
                        null,
                        TimeSpan.FromSeconds(3));
                }

                args.Handled = true;
            };

            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

            await Host.StartAsync();

            var loginWindow = GetService<Features.Auth.Views.LoginWindow>();
            loginWindow.Show();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await Host.StopAsync();
            Host.Dispose();

            base.OnExit(e);
        }
    }

    public static class Program
    {
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(
                    (context, builder) =>
                {
                    builder.AddJsonFile(
                        "appsettings.json",
                        optional: false,
                        reloadOnChange: true
                    );
                })
                .ConfigureServices((context, services) =>
                {
                    var appConfig = new AppConfig();
                    context.Configuration.Bind(appConfig);
                    services.AddSingleton(appConfig);

                    if (appConfig.ConnectionStrings.Provider.Equals("Access", System.StringComparison.OrdinalIgnoreCase))
                    {
                        services.AddSingleton<IDbConnectionFactory, AccessConnectionFactory>();
                    }
                    else
                    {
                        services.AddSingleton<IDbConnectionFactory, MySqlConnectionFactory>();
                    }
                    services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
                    services.AddSingleton<IUserSession, UserSession>();

                    services.AddSingleton<Wpf.Ui.Abstractions.INavigationViewPageProvider, Entry_Data_Processing.Core.Navigation.PageService>();
                    services.AddSingleton<Wpf.Ui.INavigationService, Wpf.Ui.NavigationService>();
                    services.AddSingleton<Entry_Data_Processing.Core.Navigation.INavigationService, Entry_Data_Processing.Core.Navigation.NavigationService>();

                    services.AddSingleton<Wpf.Ui.ISnackbarService, Wpf.Ui.SnackbarService>();
                    services.AddSingleton<Wpf.Ui.IContentDialogService, Wpf.Ui.ContentDialogService>();

                    services.AddSingleton<MainWindow>();
                    services.AddTransient<MainWindowViewModel>();

                    services.AddSingleton<Features.Auth.Services.IAuthService, Features.Auth.Services.AuthService>();
                    services.AddTransient<Features.Auth.ViewModels.LoginViewModel>();
                    services.AddTransient<Features.Auth.Views.LoginWindow>();

                    services.AddSingleton<Features.Dashboard.Services.DashboardService>();
                    services.AddTransient<Features.Dashboard.ViewModels.DashboardViewModel>();
                    services.AddTransient<Features.Dashboard.Views.DashboardPage>();

                    services.AddSingleton<Features.RequestKodeBarang.Services.IReqEdpKodeService, Features.RequestKodeBarang.Services.ReqEdpKodeService>();
                    services.AddSingleton<Features.RequestKodeBarang.Services.IApprovalWizardViewModelFactory, Features.RequestKodeBarang.Services.ApprovalWizardViewModelFactory>();
                    services.AddTransient<Features.RequestKodeBarang.ViewModels.ReqKodeListViewModel>();
                    services.AddTransient<Features.RequestKodeBarang.ViewModels.ReqKodeDetailViewModel>();
                    services.AddTransient<Features.RequestKodeBarang.Views.ReqKodeListPage>();
                    services.AddTransient<Features.RequestKodeBarang.Views.ReqKodeDetailPage>();
                });
    }
}
