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
        private readonly IUserSession _userSession;

        public MainWindow(
            Wpf.Ui.INavigationService wpfUiNavigationService,
            Entry_Data_Processing.Core.Navigation.INavigationService coreNavigationService,
            ISnackbarService snackbarService,
            IContentDialogService contentDialogService,
            IUserSession userSession)
        {
            InitializeComponent();
            _wpfUiNavigationService = wpfUiNavigationService;
            _coreNavigationService = coreNavigationService;
            _snackbarService = snackbarService;
            _contentDialogService = contentDialogService;
            _userSession = userSession;

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _wpfUiNavigationService.SetNavigationControl(RootNavigation);
            _snackbarService.SetSnackbarPresenter(RootSnackbar);
            _contentDialogService.SetDialogHost(RootContentDialog);
            if (_coreNavigationService is Core.Navigation.NavigationService coreNav)
            {
                coreNav.SetNavigationControl(_wpfUiNavigationService);
            }
            // Set User name
            if (_userSession.IsLoggedIn)
            {
                ProfileMenuItem.Content = _userSession.CurrentUser?.Name ?? "User";
            }
        }
    }
}