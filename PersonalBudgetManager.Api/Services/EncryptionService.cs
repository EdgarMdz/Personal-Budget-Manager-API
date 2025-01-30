using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.DataProtection.KeyManagement.Internal;
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

        public async Task<string> EncryptAsync(int data, CancellationToken token)
        {
            using var aesAlg = Aes.Create();
            var x = Encoding.UTF8.GetBytes(_secretKey);
            aesAlg.Key = x[..32];
            aesAlg.GenerateIV();

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            byte[] bytes = BitConverter.GetBytes(data);

            var encryptedData = await PerformCryptography(bytes, aesAlg, encryptor, token);

            return Convert.ToBase64String(encryptedData);
        }

        private static async Task<byte[]> PerformCryptography(
            byte[] data,
            Aes aes,
            ICryptoTransform encryptor,
            CancellationToken token
        )
        {
            using var ms = new MemoryStream();
            ms.Write(aes.IV, 0, aes.IV.Length);
            using var cryptoStream = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(data, 0, data.Length);
            await cryptoStream.FlushFinalBlockAsync(token);
            return ms.ToArray();
        }

        public async Task<string> DecryptAsync(string encryptedData, CancellationToken token)
        {
            var parts = encryptedData.Split(":");
            var iv = Convert.FromBase64String(parts[0]);
            var cipherText = Convert.FromBase64String(parts[1]);

            using var aesAlg = Aes.Create();
            aesAlg.Key = Encoding.UTF8.GetBytes(_secretKey);
            aesAlg.IV = iv;

            //decrypting data
            using var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
            var decrypted = await PerformCryptography(cipherText, aesAlg, decryptor, token);
            return Encoding.UTF8.GetString(decrypted);
        }
    }
}
