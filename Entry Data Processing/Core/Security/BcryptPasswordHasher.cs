using System;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;

namespace Entry_Data_Processing.Core.Security
{
    public class BcryptPasswordHasher : IPasswordHasher
    {
        public bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
            {
                return false;
            }

            try
            {
                if (hashedPassword.Length == 32 && IsHex(hashedPassword))
                {
                    using var md5 = MD5.Create();
                    var inputBytes = Encoding.UTF8.GetBytes(password);
                    var hashBytes = md5.ComputeHash(inputBytes);
                    var computedHash = Convert.ToHexString(hashBytes).ToLowerInvariant();
                    if (computedHash.Equals(hashedPassword, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

                if (hashedPassword.StartsWith("$2"))
                {
                    return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
                }

                if (password == hashedPassword)
                {
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsHex(string input)
        {
            foreach (char c in input)
            {
                if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F')))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
