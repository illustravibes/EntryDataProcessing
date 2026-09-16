namespace Entry_Data_Processing.Features.Auth.Models
{
    public class UserRecord
    {
        public int Id { get; set; }
        public string? Nip { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public int IdDepart { get; set; }
        public string? Role { get; set; }
        public string? Status { get; set; }
    }
}
