using System;
using System.Collections.Generic;
using System.Text;

namespace Entry_Data_Processing.Models
{
    public class AuthResult
    {
        public bool IsSuccess { get; private set; }
        public string ErrorMessage { get; private set; }
        public UserRecord User { get; set; }

        public static AuthResult Fail(string message)
        {
            return new AuthResult { IsSuccess = false,
                ErrorMessage = message
            };
        }

        public static AuthResult Success(UserRecord user)
        {
            return new AuthResult { IsSuccess = true, User = user };
        }
    }
}
