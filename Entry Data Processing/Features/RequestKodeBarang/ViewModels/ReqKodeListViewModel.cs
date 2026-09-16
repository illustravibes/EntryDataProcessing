using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
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
        private System.Collections.Generic.List<ReqEdpKodeRecord> _rawLoadedRequests = new();
        private System.Collections.Generic.List<ReqEdpKodeRecord> _filteredRequests = new();

        // Status Tabs: "Semua", "Pending", "Approved", "Rejected"
        [ObservableProperty]
        private string _selectedTab = "Semua";

        // Tab Counts
        [ObservableProperty]
        private int _countAll;

        [ObservableProperty]
        private int _countPending;

        [ObservableProperty]
        private int _countApproved;

        [ObservableProperty]
        private int _countRejected;

        // Bulk Selection
        [ObservableProperty]
        private bool _isAllSelected;

        [ObservableProperty]
        private int _selectedCount;

        [ObservableProperty]
        private bool _hasSelection;

        public bool IsBulkActionAvailable => SelectedTab == "Pending" || SelectedTab == "Semua";

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
        private int _pageSize = 10;

        [ObservableProperty]
        private int _totalRecords;

        [ObservableProperty]
        private int _totalPages = 1;

        [ObservableProperty]
        private string _paginationSummary = "Menampilkan 0 dari 0 request";

        public ObservableCollection<PageNumberItem> PageNumbers { get; } = new();

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

            Requests.CollectionChanged += OnRequestsCollectionChanged;
        }

        private void OnRequestsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (ReqEdpKodeRecord item in e.NewItems)
                {
                    item.PropertyChanged += OnItemPropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (ReqEdpKodeRecord item in e.OldItems)
                {
                    item.PropertyChanged -= OnItemPropertyChanged;
                }
            }
            UpdateSelectionState();
        }

        private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ReqEdpKodeRecord.IsSelected))
            {
                UpdateSelectionState();
            }
        }

        private bool _isUpdatingSelection;

        private void UpdateSelectionState()
        {
            if (_isUpdatingSelection) return;
            _isUpdatingSelection = true;
            try
            {
                var actionable = Requests.Where(x => x.CanApproveOrReject).ToList();
                var selected = actionable.Count(x => x.IsSelected);
                SelectedCount = selected;
                HasSelection = selected > 0 && IsBulkActionAvailable;
                IsAllSelected = actionable.Count > 0 && selected == actionable.Count;
            }
            finally
            {
                _isUpdatingSelection = false;
            }
        }

        partial void OnIsAllSelectedChanged(bool value)
        {
            if (_isUpdatingSelection) return;
            _isUpdatingSelection = true;
            try
            {
                foreach (var item in Requests)
                {
                    if (item.CanApproveOrReject && IsBulkActionAvailable)
                    {
                        item.IsSelected = value;
                    }
                    else
                    {
                        item.IsSelected = false;
                    }
                }
                var actionable = Requests.Where(x => x.CanApproveOrReject).ToList();
                var selected = (value && IsBulkActionAvailable) ? actionable.Count : 0;
                SelectedCount = selected;
                HasSelection = selected > 0 && IsBulkActionAvailable;
            }
            finally
            {
                _isUpdatingSelection = false;
            }
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
            ApplyTabFilter();
        }

        private void ApplyTabFilter()
        {
            IEnumerable<ReqEdpKodeRecord> filtered = SelectedTab switch
            {
                "Pending" => _rawLoadedRequests.Where(x => x.AccTidak == 0 || x.Status == "pending"),
                "Approved" => _rawLoadedRequests.Where(x => x.AccTidak == 1 || x.Status == "approve"),
                "Rejected" => _rawLoadedRequests.Where(x => x.AccTidak == 2 || x.Status == "reject"),
                _ => _rawLoadedRequests
            };

            _filteredRequests = filtered.ToList();
            for (int i = 0; i < _filteredRequests.Count; i++)
            {
                _filteredRequests[i].RowNumber = i + 1;
                _filteredRequests[i].IsSelected = false;
            }

            OnPropertyChanged(nameof(IsBulkActionAvailable));
            TotalRecords = _filteredRequests.Count;
            TotalPages = Math.Max(1, (int)Math.Ceiling(TotalRecords / (double)PageSize));
            CurrentPage = 1;
            RefreshPagedView();
        }

        [RelayCommand]
        public async Task LoadDataAsync()
        {
            IsLoading = true;

            var filter = new ReqEdpFilter
            {
                TabStatus = "Semua", // Fetch all records matching the search criteria for instant tab switches
                Area = SelectedArea,
                Toko = SelectedToko,
                Keyword = SearchKeyword,
                StartDate = StartDate,
                EndDate = EndDate?.Date.AddDays(1).AddTicks(-1)
            };

            try
            {
                var data = (await Task.Run(() => _service.GetRequestsAsync(filter))).ToList();

                // EDP only processes submitted / non-draft records
                _rawLoadedRequests = data.Where(x => x.AccTidak != 3 && x.Status != "draft").ToList();

                // Real-time tab counts computed from the dataset
                CountAll = _rawLoadedRequests.Count;
                CountPending = _rawLoadedRequests.Count(x => x.AccTidak == 0 || x.Status == "pending");
                CountApproved = _rawLoadedRequests.Count(x => x.AccTidak == 1 || x.Status == "approve");
                CountRejected = _rawLoadedRequests.Count(x => x.AccTidak == 2 || x.Status == "reject");
                TotalPendingCount = CountPending;

                ApplyTabFilter();
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
            var paged = _filteredRequests
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            foreach (var item in paged)
            {
                Requests.Add(item);
            }

            PageNumbers.Clear();
            for (int p = 1; p <= TotalPages; p++)
            {
                PageNumbers.Add(new PageNumberItem { PageNumber = p, IsActive = p == CurrentPage });
            }

            var startIdx = TotalRecords > 0 ? (CurrentPage - 1) * PageSize + 1 : 0;
            var endIdx = Math.Min(CurrentPage * PageSize, TotalRecords);
            PaginationSummary = $"Menampilkan {startIdx} – {endIdx} dari {TotalRecords} request ({SelectedTab})";
            UpdateSelectionState();
        }

        [RelayCommand]
        public void ClearSelection()
        {
            foreach (var item in Requests)
            {
                item.IsSelected = false;
            }
            UpdateSelectionState();
        }

        [RelayCommand]
        public async Task ApproveBulk()
        {
            var selectedIds = Requests.Where(x => x.IsSelected).Select(x => x.Id).ToList();
            if (!selectedIds.Any()) return;

            var nip = _userSession.CurrentUser?.Nip ?? "SYSTEM";
            var res = await _service.ApproveBulkAsync(selectedIds, nip);
            if (res.IsSuccess)
            {
                _snackbarService.Show("Sukses", $"{res.Value} request berhasil disetujui (Bulk Approve).", Wpf.Ui.Controls.ControlAppearance.Success, null, TimeSpan.FromSeconds(3));
                await LoadDataAsync();
            }
            else
            {
                _snackbarService.Show("Gagal", res.ErrorMessage ?? "Gagal memproses bulk approve.", Wpf.Ui.Controls.ControlAppearance.Danger, null, TimeSpan.FromSeconds(3));
            }
        }

        [RelayCommand]
        public async Task RejectBulk()
        {
            var selectedIds = Requests.Where(x => x.IsSelected).Select(x => x.Id).ToList();
            if (!selectedIds.Any()) return;

            var dialog = new Views.Dialogs.RejectReasonDialog(_contentDialogService.GetDialogHost());
            var result = await dialog.ShowAsync();

            if (result == Wpf.Ui.Controls.ContentDialogResult.Primary)
            {
                var nip = _userSession.CurrentUser?.Nip ?? "SYSTEM";
                var res = await _service.RejectBulkAsync(selectedIds, nip, dialog.RejectReason);
                if (res.IsSuccess)
                {
                    _snackbarService.Show("Sukses", $"{res.Value} request telah ditolak (Bulk Reject).", Wpf.Ui.Controls.ControlAppearance.Caution, null, TimeSpan.FromSeconds(3));
                    await LoadDataAsync();
                }
                else
                {
                    _snackbarService.Show("Gagal", res.ErrorMessage ?? "Gagal memproses bulk reject.", Wpf.Ui.Controls.ControlAppearance.Danger, null, TimeSpan.FromSeconds(3));
                }
            }
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
    }

    public class PageNumberItem
    {
        public int PageNumber { get; set; }
        public bool IsActive { get; set; }
    }
}
