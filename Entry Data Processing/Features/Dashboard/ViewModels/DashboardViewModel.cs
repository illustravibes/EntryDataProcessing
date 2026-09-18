using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Entry_Data_Processing.Core.Common;
using Entry_Data_Processing.Core.Navigation;
using Entry_Data_Processing.Core.Session;
using Entry_Data_Processing.Features.Dashboard.Services;
using Wpf.Ui.Controls;
using SnackbarService = Wpf.Ui.ISnackbarService;

namespace Entry_Data_Processing.Features.Dashboard.ViewModels
{
    public partial class DashboardViewModel : ViewModelBase
    {
        private readonly DashboardService _dashboardService;
        private readonly INavigationService _navigationService;
        private readonly IUserSession _userSession;
        private readonly SnackbarService _snackbarService;

        [ObservableProperty]
        private string _greeting = string.Empty;

        [ObservableProperty]
        private int _pendingRequestCount;

        public DashboardViewModel(
            DashboardService dashboardService,
            INavigationService navigationService,
            IUserSession userSession,
            SnackbarService snackbarService)
        {
            _dashboardService = dashboardService;
            _navigationService = navigationService;
            _userSession = userSession;
            _snackbarService = snackbarService;
        }

        [RelayCommand]
        public async Task InitializeAsync()
        {
            UpdateGreeting();
            
            try
            {
                var summary = await _dashboardService.GetSummaryAsync();
                PendingRequestCount = summary.PendingRequestCount;
            }
            catch (Exception ex)
            {
                PendingRequestCount = 0;
                _snackbarService.Show(
                    "Dashboard tidak tersedia",
                    $"Gagal memuat ringkasan: {ex.Message}",
                    ControlAppearance.Danger,
                    null,
                    TimeSpan.FromSeconds(3));
            }
        }

        private void UpdateGreeting()
        {
            var hour = DateTime.Now.Hour;
            var timeGreeting = hour switch
            {
                < 11 => "Selamat Pagi",
                < 15 => "Selamat Siang",
                < 18 => "Selamat Sore",
                _ => "Selamat Malam"
            };

            var name = _userSession.CurrentUser?.Name ?? "User";
            Greeting = $"{timeGreeting}, {name}";
        }

        [RelayCommand]
        private void NavigateToRequestKode()
        {
            _navigationService.NavigateTo(typeof(Features.RequestKodeBarang.Views.ReqKodeListPage));
        }
    }
}
