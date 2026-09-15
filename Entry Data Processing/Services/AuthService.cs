using Dapper;
using Entry_Data_Processing.Data;
using Entry_Data_Processing.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entry_Data_Processing.Services
{
    public class AuthService
    {
        private readonly DbConnectionFactory _factory;

        public AuthService(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<AuthResult> LoginAsync(string nip, string password)
        {
            using var connection = _factory.CreateConnection();
            var user = await connection.QueryFirstOrDefaultAsync<UserRecord>(
                "SELECT id, name, nip, password, role FROM user WHERE nip = @nip LIMIT 1",
                 new { nip });

            if ( user == null)
                return AuthResult.Fail("User not found");

            bool valid = BCrypt.Net.BCrypt.Verify(password, user.Password);
            if (!valid)
                return AuthResult.Fail("Invalid password");

            return AuthResult.Success(user);
        }
    }
}
