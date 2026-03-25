namespace EGM.Domain.Interfaces
{
    /// <summary>
    /// [Encrypted] niteliği taşıyan property'lerin veritabanına yazılmadan önce
    /// şifrelenmesi ve okunurken çözülmesi için kullanılan servis sözleşmesi.
    /// </summary>
    public interface IEncryptionService
    {
        /// <summary>Açık metni AES-256 ile şifreler ve Base64 kodlu şifreli metni döner.</summary>
        string Encrypt(string plainText);

        /// <summary>Base64 kodlu şifreli metni AES-256 ile çözer ve açık metni döner.</summary>
        string Decrypt(string cipherText);
    }
}
