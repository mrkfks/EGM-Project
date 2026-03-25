using EGM.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace EGM.Infrastructure.Security
{
    /// <summary>
    /// AES-256-CBC algoritması ile şifreleme/çözme işlemlerini gerçekleştirir.
    /// Anahtar ve IV değerleri appsettings.json'daki "Encryption:Key" ve "Encryption:IV"
    /// alanlarından Base64 formatında okunur.
    /// </summary>
    public sealed class AesEncryptionService : IEncryptionService
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;

        public AesEncryptionService(IConfiguration configuration)
        {
            var keyBase64 = configuration["Encryption:Key"]
                ?? throw new InvalidOperationException("Encryption:Key yapılandırması eksik.");
            var ivBase64  = configuration["Encryption:IV"]
                ?? throw new InvalidOperationException("Encryption:IV yapılandırması eksik.");

            _key = Convert.FromBase64String(keyBase64);
            _iv  = Convert.FromBase64String(ivBase64);

            if (_key.Length != 32)
                throw new InvalidOperationException("Encryption:Key 256-bit (32 byte) olmalıdır.");
            if (_iv.Length != 16)
                throw new InvalidOperationException("Encryption:IV 128-bit (16 byte) olmalıdır.");
        }

        /// <inheritdoc/>
        public string Encrypt(string plainText)
        {
            ArgumentNullException.ThrowIfNull(plainText);

            using var aes = Aes.Create();
            aes.Key     = _key;
            aes.IV      = _iv;
            aes.Mode    = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            var plainBytes      = Encoding.UTF8.GetBytes(plainText);
            var cipherBytes     = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            return Convert.ToBase64String(cipherBytes);
        }

        /// <inheritdoc/>
        public string Decrypt(string cipherText)
        {
            ArgumentNullException.ThrowIfNull(cipherText);

            using var aes = Aes.Create();
            aes.Key     = _key;
            aes.IV      = _iv;
            aes.Mode    = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            var cipherBytes     = Convert.FromBase64String(cipherText);
            var plainBytes      = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}
