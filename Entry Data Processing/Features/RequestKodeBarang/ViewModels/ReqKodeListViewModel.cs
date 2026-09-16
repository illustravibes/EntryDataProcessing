using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Entry_Data_Processing.Core.Common;
using Entry_Data_Processing.Core.Navigation;
using Entry_Data_Processing.Core.Session;
using Entry_Data_Processing.Features.RequestKodeBarang.Models;
using Entry_Data_Processing.Features.RequestKodeBarang.Services;
using Wpf.Ui;

namespace Entry_Data_Processing.Features.RequestKodeBarang.ViewModels
{
    public partial class ReqKodeListViewModel : ViewModelBase
    {
        private readonly IReqEdpKodeService _service;
        private readonly Core.Navigation.INavigationService _navigationService;
        private readonly IUserSession _userSession;
        private readonly ISnackbarService _snackbarService;
        private readonly IContentDialogService _contentDialogService;

        public ObservableCollection<ReqEdpKodeRecord> Requests { get; } = new();
        private System.Collections.Generic.List<ReqEdpKodeRecord> _allLoadedRequests = new();

        // Status Tabs: "Semua", "Draft", "Pending", "Approved", "Rejected"
        [ObservableProperty]
        private string _selectedTab = "Semua";

        // Tab Counts
        [ObservableProperty]
        private int _countAll;

        [ObservableProperty]
        private int _countDraft;

        [ObservableProperty]
        private int _countPending;

        [ObservableProperty]
        private int _countApproved;

        [ObservableProperty]
        private int _countRejected;

        // Filters
        public ObservableCollection<string> Areas { get; } = new() { "< Semua Area >" };
        
        [ObservableProperty]
        private string _selectedArea = "< Semua Area >";

        public ObservableCollection<string> Tokos { get; } = new() { "< Semua Toko >" };
        
        [ObservableProperty]
        private string _selectedToko = "< Semua Toko >";

        [ObservableProperty]
        private DateTime? _startDate;

        [ObservableProperty]
        private DateTime? _endDate;

        [ObservableProperty]
        private string _searchKeyword = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private ReqEdpKodeRecord? _selectedRequest;

        // Pagination
        [ObservableProperty]
        private int _currentPage = 1;

        [ObservableProperty]
        private int _pageSize = 15;

        [ObservableProperty]
        private int _totalRecords;

        [ObservableProperty]
        private int _totalPages = 1;

        [ObservableProperty]
        private string _paginationSummary = "Menampilkan 0 dari 0 request";

        [ObservableProperty]
        private int _totalPendingCount;

        // Status bar
        public string ServerInfo => "Terkoneksi (172.22.167.232)";
        public string UserInfo => $"User: {_userSession.CurrentUser?.Name ?? _userSession.CurrentUser?.Nama ?? "User"} (EDP Accounting)";
        public string CurrentDateTime => DateTime.Now.ToString("dd/MM/yyyy HH:mm");

        public ReqKodeListViewModel(
            IReqEdpKodeService service, 
            Core.Navigation.INavigationService navigationService,
            IUserSession userSession,
            ISnackbarService snackbarService,
            IContentDialogService contentDialogService)
        {
            _service = service;
            _navigationService = navigationService;
            _userSession = userSession;
            _snackbarService = snackbarService;
            _contentDialogService = contentDialogService;
        }

        public async Task InitializeLookupsAsync()
        {
            try
            {
                var areas = await _service.GetDistinctAreasAsync();
                foreach (var a in areas)
                {
                    if (!Areas.Contains(a)) Areas.Add(a);
                }

                var tokos = await _service.GetDistinctTokosAsync();
                foreach (var t in tokos)
                {
                    if (!Tokos.Contains(t)) Tokos.Add(t);
                }
            }
            catch
            {
                // Ignored if lookup fails
            }
        }

        [RelayCommand]
        public void SelectTab(string tab)
        {
            if (SelectedTab == tab) return;
            SelectedTab = tab;
            _ = LoadDataAsync();
        }

        [RelayCommand]
        public async Task LoadDataAsync()
        {
            IsLoading = true;

            var filter = new ReqEdpFilter
            {
                TabStatus = SelectedTab,
                Area = SelectedArea,
                Toko = SelectedToko,
                Keyword = SearchKeyword,
                StartDate = StartDate,
                EndDate = EndDate?.Date.AddDays(1).AddTicks(-1)
            };

            try
            {
                // 1. Fetch filtered list
                var data = (await _service.GetRequestsAsync(filter)).ToList();
                _allLoadedRequests = data;
                TotalRecords = data.Count;
                TotalPages = Math.Max(1, (int)Math.Ceiling(TotalRecords / (double)PageSize));
                CurrentPage = 1;
                
                RefreshPagedView();

                // 2. Fetch tab count badges
                var countFilter = new ReqEdpFilter
                {
                    Area = SelectedArea,
                    Toko = SelectedToko,
                    Keyword = SearchKeyword,
                    StartDate = StartDate,
                    EndDate = EndDate?.Date.AddDays(1).AddTicks(-1)
                };
                var counts = await _service.GetStatusCountsAsync(countFilter);
                CountAll = counts.All;
                CountDraft = counts.Draft;
                CountPending = counts.Pending;
                CountApproved = counts.Approved;
                CountRejected = counts.Rejected;
                TotalPendingCount = counts.Draft + counts.Pending;
            }
            catch (Exception ex)
            {
                _snackbarService.Show("Error", $"Gagal memuat data: {ex.Message}", Wpf.Ui.Controls.ControlAppearance.Danger, null, TimeSpan.FromSeconds(3));
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void RefreshPagedView()
        {
            Requests.Clear();
            var paged = _allLoadedRequests
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            foreach (var item in paged)
            {
                Requests.Add(item);
            }

            var startIdx = TotalRecords > 0 ? (CurrentPage - 1) * PageSize + 1 : 0;
            var endIdx = Math.Min(CurrentPage * PageSize, TotalRecords);
            PaginationSummary = $"Menampilkan {startIdx} – {endIdx} dari {TotalRecords} request ({SelectedTab})";
        }

        [RelayCommand]
        public void NextPage()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                RefreshPagedView();
            }
        }

        [RelayCommand]
        public void PrevPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                RefreshPagedView();
            }
        }

        [RelayCommand]
        public void GoToPage(object? pageObj)
        {
            if (pageObj == null) return;
            
            if (int.TryParse(pageObj.ToString(), out int page))
            {
                if (page >= 1 && page <= TotalPages)
                {
                    CurrentPage = page;
                    RefreshPagedView();
                }
            }
        }

        [RelayCommand]
        public void ResetFilter()
        {
            SelectedArea = "< Semua Area >";
            SelectedToko = "< Semua Toko >";
            SelectedTab = "Semua";
            StartDate = null;
            EndDate = null;
            SearchKeyword = string.Empty;
            LoadDataCommand.Execute(null);
        }

        [RelayCommand]
        public void ViewDetail(ReqEdpKodeRecord? record = null)
        {
            var target = record ?? SelectedRequest;
            if (target != null)
            {
                App.GetService<ReqKodeDetailViewModel>().LoadRequest(target.Id);
                _navigationService.NavigateTo(typeof(Views.ReqKodeDetailPage));
            }
        }

        [RelayCommand]
        public async Task Approve(ReqEdpKodeRecord? record = null)
        {
            var target = record ?? SelectedRequest;
            if (target == null) return;

            var dto = new ApprovalActionDto
            {
                Id = target.Id,
                ApproverNip = _userSession.CurrentUser?.Nip ?? "SYSTEM"
            };

            var res = await _service.ApproveRequestAsync(dto);
            if (res.IsSuccess)
            {
                _snackbarService.Show("Sukses", $"Request {target.NmBrg} berhasil disetujui.", Wpf.Ui.Controls.ControlAppearance.Success, null, TimeSpan.FromSeconds(3));
                await LoadDataAsync();
            }
            else
            {
                _snackbarService.Show("Gagal", res.ErrorMessage ?? "Terjadi kesalahan", Wpf.Ui.Controls.ControlAppearance.Danger, null, TimeSpan.FromSeconds(3));
            }
        }

        [RelayCommand]
        public async Task Reject(ReqEdpKodeRecord? record = null)
        {
            var target = record ?? SelectedRequest;
            if (target == null) return;

            var dialog = new Views.Dialogs.RejectReasonDialog(_contentDialogService.GetDialogHost());
            var result = await dialog.ShowAsync();

            if (result == Wpf.Ui.Controls.ContentDialogResult.Primary)
            {
                var dto = new ApprovalActionDto
                {
                    Id = target.Id,
                    ApproverNip = _userSession.CurrentUser?.Nip ?? "SYSTEM",
                    Alasan = dialog.RejectReason
                };

                var res = await _service.RejectRequestAsync(dto);
                if (res.IsSuccess)
                {
                    _snackbarService.Show("Sukses", $"Request {target.NmBrg} telah ditolak.", Wpf.Ui.Controls.ControlAppearance.Caution, null, TimeSpan.FromSeconds(3));
                    await LoadDataAsync();
                }
                else
                {
                    _snackbarService.Show("Gagal", res.ErrorMessage ?? "Terjadi kesalahan", Wpf.Ui.Controls.ControlAppearance.Danger, null, TimeSpan.FromSeconds(3));
                }
            }
        }

        [RelayCommand]
        public void ExportExcel()
        {
            _snackbarService.Show("Info", "Data siap diexport ke Excel.", Wpf.Ui.Controls.ControlAppearance.Info, null, TimeSpan.FromSeconds(2));
        }

        [RelayCommand]
        public void Print()
        {
            _snackbarService.Show("Info", "Mencetak daftar approval...", Wpf.Ui.Controls.ControlAppearance.Info, null, TimeSpan.FromSeconds(2));
        }
    }
}
