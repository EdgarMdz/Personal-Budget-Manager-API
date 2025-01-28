namespace PersonalBudgetManager.Api.Services.Interfaces
{
    public interface IHasher
    {
        public string HashString(string inputString, out string salt);
        public bool VerifyPassword(string password, string hashedPassword, string salt);
    }
}
