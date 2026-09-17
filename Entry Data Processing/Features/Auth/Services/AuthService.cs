using System.Threading.Tasks;
using Dapper;
using MySqlConnector;
using Entry_Data_Processing.Core.Common;
using Entry_Data_Processing.Core.Configuration;
using Entry_Data_Processing.Core.Security;
using Entry_Data_Processing.Core.Session;
using Entry_Data_Processing.Features.Auth.Models;

namespace Entry_Data_Processing.Features.Auth.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppConfig _config;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUserSession _userSession;

        // Login selalu pakai MySQL — tabel `user` ada di MySQL, bukan Access
        public AuthService(AppConfig config, IPasswordHasher passwordHasher, IUserSession userSession)
        {
            _config = config;
            _passwordHasher = passwordHasher;
            _userSession = userSession;
        }

        public async Task<Result<UserRecord>> LoginAsync(LoginRequest request)
        {
            try
            {
                using var connection = new MySqlConnection(_config.ConnectionStrings.WambDatabase);
                const string query = "SELECT * FROM user WHERE nip = @Nip LIMIT 1";
                var userRecord = await connection.QueryFirstOrDefaultAsync<UserRecord>(query, new { Nip = request.Nip.Trim() });

                if (userRecord == null)
                {
                    return Result<UserRecord>.Failure("Pengguna dengan NIP tersebut tidak ditemukan.");
                }

                if (userRecord.Status == "3" || 
                    (!string.IsNullOrEmpty(userRecord.Status) && 
                     userRecord.Status != "2" && 
                     !userRecord.Status.Equals("aktif", System.StringComparison.OrdinalIgnoreCase)))
                {
                    return Result<UserRecord>.Failure("Status akun tidak aktif.");
                }

                if (!_passwordHasher.VerifyPassword(request.Password, userRecord.Password ?? string.Empty))
                {
                    return Result<UserRecord>.Failure("Password yang Anda masukkan salah.");
                }

                _userSession.Login(userRecord);
                
                return Result<UserRecord>.Success(userRecord);
            }
            catch (System.Exception ex)
            {
                return Result<UserRecord>.Failure($"Terjadi kesalahan saat login: {ex.Message}");
            }
        }
    }
}
