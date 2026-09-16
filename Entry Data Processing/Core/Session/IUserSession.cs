using Entry_Data_Processing.Features.Auth.Models;

namespace Entry_Data_Processing.Core.Session
{
    public interface IUserSession
    {
        UserRecord? CurrentUser { get; }
        bool IsLoggedIn { get; }

        void Login(UserRecord user);
        void Logout();
    }
}
