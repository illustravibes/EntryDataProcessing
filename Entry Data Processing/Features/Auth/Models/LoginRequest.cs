namespace Entry_Data_Processing.Features.Auth.Models
{
    public class LoginRequest
    {
        public string Nip { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
