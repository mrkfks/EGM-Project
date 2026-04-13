using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using EGM.Domain.Interfaces;

namespace EGM.Infrastructure
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<EGMDbContext>
    {
        public EGMDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<EGMDbContext>();
            // EGM.API klasöründeki egm.db'yi hedefle (hem dotnet ef hem dotnet run uyumlu)
            var basePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "EGM.API"));
            if (!Directory.Exists(basePath))
                basePath = Directory.GetCurrentDirectory();
            var dbPath = Path.Combine(basePath, "egm.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");

            // Provide a mock or default implementation for IEncryptionService
            var encryptionService = new MockEncryptionService();

            return new EGMDbContext(optionsBuilder.Options, encryptionService);
        }
    }

    public class MockEncryptionService : IEncryptionService
    {
        public string Encrypt(string plainText)
        {
            // Mock implementation: return the plain text as is
            return plainText;
        }

        public string Decrypt(string cipherText)
        {
            // Mock implementation: return the cipher text as is
            return cipherText;
        }
    }
}