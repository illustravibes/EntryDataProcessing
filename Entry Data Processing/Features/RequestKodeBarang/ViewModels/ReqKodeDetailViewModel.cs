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
         private readonly IApprovalWizardViewModelFactory _approvalWizardFactory;
         private readonly IRejectReasonViewModelFactory _rejectReasonFactory;

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
             IApprovalWizardViewModelFactory approvalWizardFactory,
             IRejectReasonViewModelFactory rejectReasonFactory)
        {
            _service = service;
            _navigationService = navigationService;
            _userSession = userSession;
             _snackbarService = snackbarService;
             _approvalWizardFactory = approvalWizardFactory;
             _rejectReasonFactory = rejectReasonFactory;
        }

        public async Task LoadRequestAsync(int id)
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

            var wizardVm = _approvalWizardFactory.Create();
            await wizardVm.InitializeAsync(RequestDetail.Id);

            var dialog = new Views.Dialogs.ApprovalWizardDialog(wizardVm, _snackbarService);
            dialog.ShowDialog();

            if (dialog.IsApproved)
            {
                await LoadRequestAsync(RequestDetail.Id);
            }
        }

        [RelayCommand]
        private async Task RejectAsync()
        {
            if (RequestDetail == null || _userSession.CurrentUser == null) return;

            
            
            var dialog = new Views.Dialogs.RejectReasonDialog(_rejectReasonFactory.Create())
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
                    await LoadRequestAsync(RequestDetail.Id);
                }
                else
                {
                    _snackbarService.Show("Error", result.ErrorMessage ?? "Gagal memproses", ControlAppearance.Danger, null, System.TimeSpan.FromSeconds(3));
                }
            }
        }
    }
}
