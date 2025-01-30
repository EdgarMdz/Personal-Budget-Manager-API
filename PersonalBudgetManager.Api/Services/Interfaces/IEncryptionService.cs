namespace PersonalBudgetManager.Api.Services.Interfaces
{
    public interface IEncryptionService
    {
        public string HashString(string inputString, out string salt);
        public bool CompareHashStrings(string inputString, string hashed, string salt);

        public Task<string> EncryptAsync(int data, CancellationToken token);
        public Task<string> DecryptAsync(string encryptedData, CancellationToken token);
    }
}
