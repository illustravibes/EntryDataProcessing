using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Entry_Data_Processing.Core.Common;
using Entry_Data_Processing.Features.Auth.Models;
using Entry_Data_Processing.Features.Auth.Services;

namespace Entry_Data_Processing.Features.Auth.ViewModels
{
    public partial class LoginViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;

        [ObservableProperty]
        private string _nip = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private bool _isLoading;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        partial void OnErrorMessageChanged(string value) => OnPropertyChanged(nameof(HasError));

        partial void OnNipChanged(string value) => ErrorMessage = string.Empty;
        partial void OnPasswordChanged(string value) => ErrorMessage = string.Empty;

        public LoginViewModel(IAuthService authService)
        {
            _authService = authService;
        }

        public event EventHandler? LoginSucceeded;

        private bool CanLogin() => !IsLoading;

        [RelayCommand(CanExecute = nameof(CanLogin))]
        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Nip) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "NIP dan password tidak boleh kosong.";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var request = new LoginRequest { Nip = Nip, Password = Password };
                var result = await _authService.LoginAsync(request);

                if (result.IsSuccess)
                {
                    LoginSucceeded?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    ErrorMessage = result.ErrorMessage ?? "NIP atau password salah. Silakan coba lagi.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Gagal terhubung ke server: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
