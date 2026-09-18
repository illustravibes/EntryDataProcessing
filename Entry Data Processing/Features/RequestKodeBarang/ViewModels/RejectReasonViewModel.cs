using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Entry_Data_Processing.Core.Common;

namespace Entry_Data_Processing.Features.RequestKodeBarang.ViewModels;

public partial class RejectReasonViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CharacterCount))]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    private string _reason = string.Empty;

    public string CharacterCount => $"{Reason.Length} karakter";

    public bool CanConfirm => !string.IsNullOrWhiteSpace(Reason);

    public event EventHandler? Confirmed;
    public event EventHandler? Cancelled;

    [RelayCommand(CanExecute = nameof(CanConfirm))]
    private void Confirm()
    {
        Reason = Reason.Trim();
        Confirmed?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void Cancel()
    {
        Cancelled?.Invoke(this, EventArgs.Empty);
    }
}
