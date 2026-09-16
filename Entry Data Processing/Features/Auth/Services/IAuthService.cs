using System.Threading.Tasks;
using Entry_Data_Processing.Core.Common;
using Entry_Data_Processing.Features.Auth.Models;

namespace Entry_Data_Processing.Features.Auth.Services
{
    public interface IAuthService
    {
        Task<Result<UserRecord>> LoginAsync(LoginRequest request);
    }
}
