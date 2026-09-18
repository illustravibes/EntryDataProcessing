using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Entry_Data_Processing.Core.Common;
using Entry_Data_Processing.Core.Session;

namespace Entry_Data_Processing;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IUserSession _userSession;

    [ObservableProperty]
    private string _profileName = "Guest";

    public MainWindowViewModel(IUserSession userSession)
    {
        _userSession = userSession;
    }

    public void RefreshProfile()
    {
        ProfileName = _userSession.CurrentUser?.Name ?? _userSession.CurrentUser?.Nama ?? "Guest";
    }

    public event EventHandler? LogoutRequested;

    [RelayCommand]
    private void Logout()
    {
        _userSession.Logout();
        LogoutRequested?.Invoke(this, EventArgs.Empty);
    }
}
