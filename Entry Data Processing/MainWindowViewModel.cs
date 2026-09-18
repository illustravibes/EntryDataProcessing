using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Entry_Data_Processing.Core.Common;
using Entry_Data_Processing.Core.Session;
using System.Threading;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace Entry_Data_Processing;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IUserSession _userSession;
    private readonly IContentDialogService _contentDialogService;

    [ObservableProperty]
    private string _profileName = "Guest";

    public MainWindowViewModel(
        IUserSession userSession,
        IContentDialogService contentDialogService)
    {
        _userSession = userSession;
        _contentDialogService = contentDialogService;
    }

    public void RefreshProfile()
    {
        ProfileName = _userSession.CurrentUser?.Name ?? _userSession.CurrentUser?.Nama ?? "Guest";
    }

    public event EventHandler? LogoutRequested;

    [RelayCommand]
    private async Task LogoutAsync()
    {
        var result = await _contentDialogService.ShowAsync(
            new ContentDialog
            {
                Title = "Konfirmasi Logout",
                Content = "Apakah Anda yakin ingin keluar dari aplikasi?",
                PrimaryButtonText = "Logout",
                CloseButtonText = "Batal",
                DefaultButton = ContentDialogButton.Primary
            },
            CancellationToken.None);

        if (result != ContentDialogResult.Primary)
        {
            return;
        }

        _userSession.Logout();
        LogoutRequested?.Invoke(this, EventArgs.Empty);
    }
}
