using System.Windows;
using Wpf.Ui.Controls;
using Wpf.Ui;
using Entry_Data_Processing.Core.Navigation;
using Entry_Data_Processing.Core.Session;

namespace Entry_Data_Processing
{
    public partial class MainWindow : FluentWindow
    {
        private readonly Wpf.Ui.INavigationService _wpfUiNavigationService;
        private readonly Entry_Data_Processing.Core.Navigation.INavigationService _coreNavigationService;
        private readonly ISnackbarService _snackbarService;
        private readonly IContentDialogService _contentDialogService;
        private readonly Wpf.Ui.Abstractions.INavigationViewPageProvider _pageProvider;
        private readonly MainWindowViewModel _viewModel;

        public MainWindow(
            Wpf.Ui.INavigationService wpfUiNavigationService,
            Entry_Data_Processing.Core.Navigation.INavigationService coreNavigationService,
            Wpf.Ui.Abstractions.INavigationViewPageProvider pageProvider,
            ISnackbarService snackbarService,
            IContentDialogService contentDialogService,
            MainWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = _viewModel = viewModel;
            _wpfUiNavigationService = wpfUiNavigationService;
            _coreNavigationService = coreNavigationService;
            _pageProvider = pageProvider;
            _snackbarService = snackbarService;
            _contentDialogService = contentDialogService;
            _viewModel.LogoutRequested += OnLogoutRequested;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            RootNavigation.SetPageProviderService(_pageProvider);
            _wpfUiNavigationService.SetNavigationControl(RootNavigation);
            _snackbarService.SetSnackbarPresenter(RootSnackbar);
            _contentDialogService.SetDialogHost(RootContentDialog);
            if (_coreNavigationService is Core.Navigation.NavigationService coreNav)
            {
                coreNav.SetNavigationControl(_wpfUiNavigationService);
            }
            _viewModel.RefreshProfile();
            _wpfUiNavigationService.Navigate(typeof(Features.Dashboard.Views.DashboardPage));
        }

        private void OnLogoutRequested(object? sender, EventArgs e)
        {
            var loginWindow = App.GetService<Features.Auth.Views.LoginWindow>();
            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            loginWindow.Show();
            Application.Current.MainWindow = loginWindow;
            Close();
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }
    }
}
