using CommunityToolkit.Mvvm.ComponentModel;
using Entry_Data_Processing.Features.Auth.Models;

namespace Entry_Data_Processing.Core.Session
{
    public partial class UserSession : ObservableObject, IUserSession
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsLoggedIn))]
        private UserRecord? _currentUser;

        public bool IsLoggedIn => CurrentUser != null;

        public void Login(UserRecord user)
        {
            CurrentUser = user;
        }

        public void Logout()
        {
            CurrentUser = null;
        }
    }
}
