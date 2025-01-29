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

        public string Encrypt(string data)
        {
            using var aesAlg = Aes.Create();
            aesAlg.Key = Encoding.UTF8.GetBytes(_secretKey);
            aesAlg.GenerateIV();

            using var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
            var encrypted = PerformCryptography(Encoding.UTF8.GetBytes(data), encryptor);
            var iv = Convert.ToBase64String(aesAlg.IV);
            var encryptedData = Convert.ToBase64String(encrypted);

            return $"{iv}:{encrypted}"; // Return the IV along with the encrypted data
        }

        private static byte[] PerformCryptography(byte[] data, ICryptoTransform encryptor)
        {
            using var ms = new MemoryStream();
            using var cryptoStream = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(data, 0, data.Length);
            return ms.ToArray();
        }

        public string Decrypt(string encryptedData)
        {
            var parts = encryptedData.Split(":");
            var iv = Convert.FromBase64String(parts[0]);
            var cipherText = Convert.FromBase64String(parts[1]);

            using var aesAlg = Aes.Create();
            aesAlg.Key = Encoding.UTF8.GetBytes(_secretKey);
            aesAlg.IV = iv;

            //decrypting data
            using var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
            var decrypted = PerformCryptography(cipherText, decryptor);
            return Encoding.UTF8.GetString(decrypted);
        }
    }
}
