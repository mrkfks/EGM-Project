using EGM.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EGM.Infrastructure.Persistence
{
    /// <summary>
    /// [Encrypted] niteliği taşıyan string? property'leri için EF Core Value Converter.
    /// Veritabanına yazarken şifreler, okurken çözer.
    /// null değerler olduğu gibi geçirilir (şifreleme uygulanmaz).
    /// </summary>
    public sealed class EncryptedValueConverter : ValueConverter<string?, string?>
    {
        public EncryptedValueConverter(IEncryptionService encryptionService)
            : base(
                plainText  => plainText == null ? null : encryptionService.Encrypt(plainText),
                cipherText => cipherText == null ? null : encryptionService.Decrypt(cipherText)
            )
        {
        }
    }
}
