namespace PersonalBudgetManager.Api.Services.Interfaces
{
    public interface IEncryptionService
    {
        public string HashString(string inputString, out string salt);
        public bool CompareHashStrings(string inputString, string hashed, string salt);

        public string Encrypt(int data);
        public string Decrypt(string encryptedData);
    }
}
