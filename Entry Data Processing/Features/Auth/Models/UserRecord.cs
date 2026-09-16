namespace Entry_Data_Processing.Features.Auth.Models
{
    public class UserRecord
    {
        public int Id { get; set; }
        public string? Nip { get; set; }
        public string? Nama { get; set; }
        public string? Name 
        { 
            get => !string.IsNullOrEmpty(Nama) ? Nama : _name; 
            set => _name = value; 
        }
        private string? _name;
        public string? Email { get; set; }
        public string? Password { get; set; }
        public int IdDepart { get; set; }
        public string? Role { get; set; }
        public string? Status { get; set; }
    }
}
