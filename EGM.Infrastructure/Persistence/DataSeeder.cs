using EGM.Domain.Constants;
using EGM.Domain.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using BCrypt.Net;

namespace EGM.Infrastructure.Persistence
{
    public static class DataSeeder
    {
        public static void Seed(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<EGMDbContext>();

            // Check if database is locked before migrating
            try
            {
                context.Database.Migrate();
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 5)
            {
                Console.WriteLine("Database is locked. Skipping migration.");
                return;
            }

            // Add roles and example users
            Console.WriteLine("DataSeeder: Seeding started.");
            try
            {
                if (!context.Users.Any())
                {
                    context.Users.AddRange(
                        new User
                        {
                            Id = Guid.NewGuid(),
                            Sicil = 425394, // Sicil integer olarak düzeltildi
                            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Egm123!!"),
                            Role = Roles.Yetkili,
                            FullName = "Sistem Yetkilisi",
                            Email = "admin@egm.gov.tr",
                            GSM = "5300839355",
                            Birim = "Başkanlık",
                            CityId = null, // CityId nullable olarak düzeltildi
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            CreatedByUserId = "system",
                            IsDeleted = false
                        }
                    );

                    context.SaveChanges();
                    Console.WriteLine("DataSeeder: User seeding completed successfully.");
                }

                if (!context.Groups.Any())
                {
                    context.Groups.AddRange(
                        new Group { Id = Guid.NewGuid(), Name = "Siyasi Parti" },
                        new Group { Id = Guid.NewGuid(), Name = "Sendika" },
                        new Group { Id = Guid.NewGuid(), Name = "STK" },
                        new Group { Id = Guid.NewGuid(), Name = "Meslek Örgütü" },
                        new Group { Id = Guid.NewGuid(), Name = "Öğrenci Grubu" },
                        new Group { Id = Guid.NewGuid(), Name = "İnisiyatifler" },
                        new Group { Id = Guid.NewGuid(), Name = "Spor Taraftar Grubu" }
                    );
                    context.SaveChanges();
                    Console.WriteLine("DataSeeder: Group seeding completed successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DataSeeder: Seeding failed. Error: {ex.Message}");
                throw;
            }
        }
    }
}