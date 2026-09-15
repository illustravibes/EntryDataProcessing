using System;
using System.Collections.Generic;
using System.Text;

namespace Entry_Data_Processing.Models
{
    public class UserRecord
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Nip { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }
}
