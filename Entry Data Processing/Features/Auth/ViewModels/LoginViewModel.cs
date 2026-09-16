using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Entry_Data_Processing.Core.Common;
using Entry_Data_Processing.Features.Auth.Models;
using Entry_Data_Processing.Features.Auth.Services;
using Wpf.Ui;
using System.Windows;

namespace Entry_Data_Processing.Features.Auth.ViewModels
{
    public partial class LoginViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;
        private readonly ISnackbarService _snackbarService;
        
        [ObservableProperty]
        private string _nip = string.Empty;
        
        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        public LoginViewModel(IAuthService authService, ISnackbarService snackbarService)
        {
            _authService = authService;
            _snackbarService = snackbarService;
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Nip) || string.IsNullOrWhiteSpace(Password))
            {
                _snackbarService.Show("Error", "NIP dan password tidak boleh kosong.", Wpf.Ui.Controls.ControlAppearance.Danger, null, System.TimeSpan.FromSeconds(3));
                return;
            }

            IsLoading = true;

            var request = new LoginRequest { Nip = Nip, Password = Password };
            var result = await _authService.LoginAsync(request);

            IsLoading = false;

            if (result.IsSuccess)
            {
                var mainWindow = App.GetService<MainWindow>();
                Application.Current.MainWindow = mainWindow;
                mainWindow.Show();
                
                // Close LoginWindow safely
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is Features.Auth.Views.LoginWindow)
                    {
                        window.Close();
                        break;
                    }
                }
                
                Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            }
            else
            {
                _snackbarService.Show("Gagal Login", result.ErrorMessage ?? "Terjadi kesalahan", Wpf.Ui.Controls.ControlAppearance.Danger, null, System.TimeSpan.FromSeconds(4));
            }
        }
    }
}
