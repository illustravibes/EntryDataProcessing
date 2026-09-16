using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Entry_Data_Processing.Core.Common;
using Entry_Data_Processing.Core.Navigation;
using Entry_Data_Processing.Features.RequestKodeBarang.Models;
using Entry_Data_Processing.Features.RequestKodeBarang.Services;

namespace Entry_Data_Processing.Features.RequestKodeBarang.ViewModels
{
    public partial class ReqKodeListViewModel : ViewModelBase
    {
        private readonly IReqEdpKodeService _service;
        private readonly INavigationService _navigationService;

        public ObservableCollection<ReqEdpKodeRecord> Requests { get; } = new();

        [ObservableProperty]
        private string _searchKeyword = string.Empty;

        [ObservableProperty]
        private int _selectedFilterIndex = 0; // 0=Pending, 1=Approved, 2=Rejected, 3=All

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private ReqEdpKodeRecord? _selectedRequest;

        public ReqKodeListViewModel(IReqEdpKodeService service, INavigationService navigationService)
        {
            _service = service;
            _navigationService = navigationService;
        }

        [RelayCommand]
        public async Task LoadDataAsync()
        {
            IsLoading = true;
            Requests.Clear();

            int? statusFilter = SelectedFilterIndex switch
            {
                0 => 0, // Pending
                1 => 1, // Approved
                2 => 2, // Rejected
                _ => null // All
            };

            var filter = new ReqEdpFilter
            {
                Status = statusFilter,
                Keyword = SearchKeyword
            };

            try
            {
                var data = await _service.GetRequestsAsync(filter);
                foreach (var item in data)
                {
                    Requests.Add(item);
                }
            }
            catch
            {
                // Handle error
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void ViewDetail()
        {
            if (SelectedRequest != null)
            {
                // Simple navigation - in a real app we might pass the ID to the detail view model
                App.GetService<ReqKodeDetailViewModel>().LoadRequest(SelectedRequest.Id);
                _navigationService.NavigateTo(typeof(Views.ReqKodeDetailPage));
            }
        }

        partial void OnSelectedFilterIndexChanged(int value)
        {
            LoadDataCommand.Execute(null);
        }
    }
}
