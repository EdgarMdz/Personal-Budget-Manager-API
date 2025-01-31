using System.Security.Cryptography;
using System.Text;
using PersonalBudgetManager.Api.Services.Interfaces;

namespace PersonalBudgetManager.Api.Services
{
    public class EncryptionService : IEncryptionService
    {
        private readonly string _secretKey;

        public EncryptionService()
        {
            _secretKey =
                Environment.GetEnvironmentVariable("PersonalBudgetManager_SecretKey")
                ?? throw new InvalidOperationException(
                    "PersonalBudgetManager_SecretKey not defined in environment variables."
                );
        }

        public string HashString(string inputString, out string salt)
        {
            salt = GenerateSalt();
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(salt));
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(inputString));
            return Convert.ToBase64String(computedHash);
        }

        public bool CompareHashStrings(string password, string hashedPassword, string salt)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(salt));
            var hashedInputPassword = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return string.Compare(Convert.ToBase64String(hashedInputPassword), hashedPassword) == 0;
        }

        private static string GenerateSalt()
        {
            var saltBytes = new byte[16];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(saltBytes);
            return Convert.ToBase64String(saltBytes);
        }
    }
}
