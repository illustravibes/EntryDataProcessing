namespace Entry_Data_Processing.Core.Security
{
    public interface IPasswordHasher
    {
        bool VerifyPassword(string password, string hashedPassword);
    }
}
