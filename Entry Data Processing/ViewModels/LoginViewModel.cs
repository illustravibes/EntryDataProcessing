using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Entry_Data_Processing.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Entry_Data_Processing.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly AuthService _authService;
        private readonly SessionService _session;

        [ObservableProperty] private string nip;
        [ObservableProperty] private string errorMessage;
        [ObservableProperty] private bool isLoading;

        public event Action LoginSucceeded;

        public LoginViewModel(AuthService authService, SessionService session)
        {
            _authService = authService;
            _session = session;
        }

        [RelayCommand]
        private async Task LoginAsync(string password)
        {
            ErrorMessage = string.Empty;
            IsLoading = true;

            try
            {
                var result = await _authService.LoginAsync(nip, password);
                if (result.IsSuccess)
                {
                    _session.CurrentUser = result.User;
                    LoginSucceeded?.Invoke();
                }
                else
                {
                    ErrorMessage = result.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
