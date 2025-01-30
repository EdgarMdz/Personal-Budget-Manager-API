using System.Security.Cryptography;
using System.Text;
using Humanizer;
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
            salt = GenerateSalt(16);
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

        private static string GenerateSalt(int size)
        {
            var saltBytes = new byte[size];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(saltBytes);
            return Convert.ToBase64String(saltBytes);
        }

        public string Encrypt(int data)
        {
            using var aesAlg = Aes.Create();
            var x = Encoding.UTF8.GetBytes(_secretKey);
            aesAlg.Key = x[..32];
            aesAlg.GenerateIV();

            ICryptoTransform encriptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            byte[] bytes = BitConverter.GetBytes(data);
            using MemoryStream ms = new();

            ms.Write(aesAlg.IV, 0, aesAlg.IV.Length);

            using CryptoStream cryptoStream = new(ms, encriptor, CryptoStreamMode.Write);
            cryptoStream.Write(bytes, 0, bytes.Length);
            cryptoStream.FlushFinalBlock();
            return Convert.ToBase64String(ms.ToArray());
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
