using Entry_Data_Processing.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entry_Data_Processing.Services
{
    public class SessionService
    {
        public UserRecord CurrentUser { get; set; }

        public bool IsLoggedIn => CurrentUser != null;

        public void Logout()
        {
            CurrentUser = null;
        }
    }
}
