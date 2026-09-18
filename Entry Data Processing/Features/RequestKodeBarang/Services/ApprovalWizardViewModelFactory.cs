using Entry_Data_Processing.Core.Session;
using Entry_Data_Processing.Features.RequestKodeBarang.ViewModels;
using Wpf.Ui;

namespace Entry_Data_Processing.Features.RequestKodeBarang.Services;

public sealed class ApprovalWizardViewModelFactory : IApprovalWizardViewModelFactory
{
    private readonly IReqEdpKodeService _service;
    private readonly IUserSession _userSession;
    private readonly ISnackbarService _snackbarService;
    private readonly IContentDialogService _contentDialogService;

    public ApprovalWizardViewModelFactory(
        IReqEdpKodeService service,
        IUserSession userSession,
        ISnackbarService snackbarService,
        IContentDialogService contentDialogService)
    {
        _service = service;
        _userSession = userSession;
        _snackbarService = snackbarService;
        _contentDialogService = contentDialogService;
    }

    public ApprovalWizardViewModel Create()
    {
        return new ApprovalWizardViewModel(
            _service,
            _userSession,
            _snackbarService,
            _contentDialogService);
    }
}
