using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Entry_Data_Processing.Core.Common;
using Entry_Data_Processing.Core.Navigation;
using Entry_Data_Processing.Core.Session;
using Entry_Data_Processing.Features.Dashboard.Services;

namespace Entry_Data_Processing.Features.Dashboard.ViewModels
{
    public partial class DashboardViewModel : ViewModelBase
    {
        private readonly DashboardService _dashboardService;
        private readonly INavigationService _navigationService;
        private readonly IUserSession _userSession;

        [ObservableProperty]
        private string _greeting = string.Empty;

        [ObservableProperty]
        private int _pendingRequestCount;

        public DashboardViewModel(DashboardService dashboardService, INavigationService navigationService, IUserSession userSession)
        {
            _dashboardService = dashboardService;
            _navigationService = navigationService;
            _userSession = userSession;
        }

        public async Task InitializeAsync()
        {
            UpdateGreeting();
            
            try
            {
                var summary = await _dashboardService.GetSummaryAsync();
                PendingRequestCount = summary.PendingRequestCount;
            }
            catch
            {
                // In a real app we'd log this or show a toast
                PendingRequestCount = 0;
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
            // Note: We need to resolve the type of ReqKodeListPage
            // Assuming we use type-based navigation
            _navigationService.NavigateTo(typeof(Features.RequestKodeBarang.Views.ReqKodeListPage));
        }
    }
}
