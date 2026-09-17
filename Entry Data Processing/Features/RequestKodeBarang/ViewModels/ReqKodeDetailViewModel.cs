using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Entry_Data_Processing.Core.Common;
using Entry_Data_Processing.Core.Navigation;
using Entry_Data_Processing.Core.Session;
using Entry_Data_Processing.Features.RequestKodeBarang.Models;
using Entry_Data_Processing.Features.RequestKodeBarang.Services;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace Entry_Data_Processing.Features.RequestKodeBarang.ViewModels
{
    public partial class ReqKodeDetailViewModel : ViewModelBase
    {
        private readonly IReqEdpKodeService _service;
        private readonly Core.Navigation.INavigationService _navigationService;
        private readonly IUserSession _userSession;
        private readonly ISnackbarService _snackbarService;
        private readonly IContentDialogService _contentDialogService;

        [ObservableProperty]
        private ReqEdpKodeRecord? _requestDetail;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _canApproveOrReject;

        public ReqKodeDetailViewModel(
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

        public async void LoadRequest(int id)
        {
            IsLoading = true;
            try
            {
                RequestDetail = await _service.GetRequestByIdAsync(id);
                CanApproveOrReject = RequestDetail?.AccTidak == 0;
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void GoBack()
        {
            _navigationService.GoBack();
        }

        [RelayCommand]
        private async Task ApproveAsync()
        {
            if (RequestDetail == null || _userSession.CurrentUser == null) return;

            var wizardVm = new ApprovalWizardViewModel(_service, _userSession, _snackbarService, _contentDialogService);
            await wizardVm.InitializeAsync(RequestDetail.Id);

            var dialog = new Views.Dialogs.ApprovalWizardDialog(wizardVm);
            dialog.ShowDialog();

            if (dialog.IsApproved)
            {
                LoadRequest(RequestDetail.Id); 
            }
        }

        [RelayCommand]
        private async Task RejectAsync()
        {
            if (RequestDetail == null || _userSession.CurrentUser == null) return;

            // Here we should show a dialog, but for simplicity we'll just prompt for a reason string via custom UI
            // Assuming the View code-behind or a dialog service handles getting the reason
            
            // For now, we'll just create a mock reason or expect it to be passed.
            // A more robust implementation would use a DialogService to show the RejectReasonDialog
            
            var dialog = new Views.Dialogs.RejectReasonDialog
            {
                Owner = System.Windows.Application.Current.MainWindow
            };
            dialog.ShowDialog();

            if (dialog.IsConfirmed)
            {

                IsLoading = true;
                var action = new ApprovalActionDto
                {
                    Id = RequestDetail.Id,
                    ApproverNip = _userSession.CurrentUser.Nip ?? "UNKNOWN",
                    Alasan = dialog.RejectReason
                };

                var result = await _service.RejectRequestAsync(action);
                IsLoading = false;

                if (result.IsSuccess)
                {
                    _snackbarService.Show("Ditolak", "Permohonan berhasil ditolak", ControlAppearance.Caution, null, System.TimeSpan.FromSeconds(3));
                    LoadRequest(RequestDetail.Id);
                }
                else
                {
                    _snackbarService.Show("Error", result.ErrorMessage ?? "Gagal memproses", ControlAppearance.Danger, null, System.TimeSpan.FromSeconds(3));
                }
            }
        }
    }
}
