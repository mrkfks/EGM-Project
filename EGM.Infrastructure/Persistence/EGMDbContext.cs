using EGM.Domain.Attributes;
using EGM.Domain.Entities;
using EGM.Domain.Interfaces;
using EGM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace EGM.Infrastructure
{
    public class EGMDbContext : DbContext
    {
        private readonly IEncryptionService _encryptionService;

        public EGMDbContext(
            DbContextOptions<EGMDbContext> options,
            IEncryptionService encryptionService)
            : base(options)
        {
            _encryptionService = encryptionService;
        }

        // Kullanıcılar
        public DbSet<User> Users { get; set; } = null!;

        // Olay Yönetimi
        public DbSet<Olay> Olaylar { get; set; } = null!;
        public DbSet<SosyalMedyaOlay> SosyalMedyaOlaylar { get; set; } = null!;
        public DbSet<YuruyusRota> YuruyusRotasi { get; set; } = null!;

        // Organizator ve Konu
        public DbSet<Organizator> Organizatorler { get; set; } = null!;
        public DbSet<KategoriOrganizator> KategoriOrganizatorler { get; set; } = null!;
        public DbSet<Konu> Konular { get; set; } = null!;

        // Seçim
        public DbSet<SandikOlay> SandikOlaylar { get; set; } = null!;

        // VIP Ziyaret
        public DbSet<VIPZiyaret> VIPZiyaretler { get; set; } = null!;

        // Audit
        public DbSet<AuditLog> AuditLoglar { get; set; } = null!;

        // Bildirimler
        public DbSet<Notification> Bildirimler { get; set; } = null!;

        public DbSet<OlayTuru> OlayTurleri { get; set; } = null!;
        public DbSet<GerceklesmeSekli> GerceklesmeSekilleri { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Global soft-delete query filter ─────────────────────────
            // BaseEntity'den türeyen tüm entity'lerde IsDeleted=true kayıtlar
            // otomatik olarak sorgu dışında bırakılır.
            modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Olay>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<SosyalMedyaOlay>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<YuruyusRota>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Organizator>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<KategoriOrganizator>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Konu>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<SandikOlay>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<VIPZiyaret>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Notification>().HasQueryFilter(e => !e.IsDeleted);

            // ── User ────────────────────────────────────────────────────
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Sicil)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Role);

            // ── Olay ────────────────────────────────────────────────────
            modelBuilder.Entity<Olay>()
                .HasIndex(o => o.CityId);

            modelBuilder.Entity<Olay>()
                .HasIndex(o => o.Tarih);

            modelBuilder.Entity<Olay>()
                .HasIndex(o => o.Durum);

            modelBuilder.Entity<Olay>()
                .HasIndex(o => o.CreatedByUserId);

            // Add geospatial index for Latitude and Longitude
            modelBuilder.Entity<Olay>()
                .HasIndex(o => new { o.Latitude, o.Longitude })
                .HasDatabaseName("IX_Olay_Latitude_Longitude");

            // ── SosyalMedyaOlay ──────────────────────────────────────────
            modelBuilder.Entity<SosyalMedyaOlay>()
                .HasIndex(s => s.OlayId);

            // ── AuditLog ─────────────────────────────────────────────────
            modelBuilder.Entity<AuditLog>()
                .HasIndex(a => a.UserId);

            modelBuilder.Entity<AuditLog>()
                .HasIndex(a => a.Timestamp);

            modelBuilder.Entity<AuditLog>()
                .HasIndex(a => new { a.EntityName, a.EntityId });

            // ── Olay - Organizator ──────────────────────────────────────
            modelBuilder.Entity<Olay>()
                .HasOne(o => o.Organizator)
                .WithMany(org => org.Olaylar)
                .HasForeignKey(o => o.OrganizatorId);

            // ── Olay - Konu ─────────────────────────────────────────────
            modelBuilder.Entity<Olay>()
                .HasOne(o => o.Konu)
                .WithMany(k => k.Olaylar)
                .HasForeignKey(o => o.KonuId);

            // ── Konu hiyerarşi (self-reference) ─────────────────────────
            modelBuilder.Entity<Konu>()
                .HasOne(k => k.UstKonu)
                .WithMany(k => k.AltKonular)
                .HasForeignKey(k => k.UstKonuId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── SosyalMedyaOlay - Olay ──────────────────────────────────
            modelBuilder.Entity<SosyalMedyaOlay>()
                .HasOne(s => s.Olay)
                .WithMany(o => o.SosyalMedyaOlaylar)
                .HasForeignKey(s => s.OlayId);

            // ── YuruyusRota - Olay ──────────────────────────────────────
            modelBuilder.Entity<YuruyusRota>()
                .HasOne(r => r.Olay)
                .WithMany(o => o.YuruyusRotasi)
                .HasForeignKey(r => r.OlayId);

            // ── KategoriOrganizator - Organizator (M:N) ─────────────────
            modelBuilder.Entity<KategoriOrganizator>()
                .HasMany(k => k.Organizatorler)
                .WithMany(o => o.Kategoriler);

            // ── AuditLog ────────────────────────────────────────────────
            // AuditLog soft-delete filtresinden muaf tutulur; tüm kayıtlar görünür kalır.
            modelBuilder.Entity<AuditLog>().HasQueryFilter(e => !e.IsDeleted);

            modelBuilder.Entity<AuditLog>()
                .Property(a => a.Changes)
                .HasColumnType("TEXT");

            modelBuilder.Entity<AuditLog>()
                .Property(a => a.Action)
                .HasConversion<string>();

            // ── [Encrypted] Value Converter ─────────────────────────────
            // Tüm entity'lerde [Encrypted] niteliği taşıyan string? property'lere
            // EncryptedValueConverter otomatik olarak uygulanır.
            var converter = new EncryptedValueConverter(_encryptionService);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(string))
                    {
                        var memberInfo = property.PropertyInfo ?? (MemberInfo?)property.FieldInfo;
                        if (memberInfo?.GetCustomAttribute<EncryptedAttribute>() is not null)
                        {
                            property.SetValueConverter(converter);
                        }
                    }
                }
            }

            // OlayTuru - GerceklesmeSekli ilişkisi
            modelBuilder.Entity<OlayTuru>()
                .HasMany(o => o.GerceklesmeSekilleri)
                .WithOne(g => g.OlayTuru)
                .HasForeignKey(g => g.OlayTuruId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── Olay - GerceklesmeSekli (optional FK) ───────────────────
            modelBuilder.Entity<Olay>()
                .HasOne(o => o.GerceklesmeSekli)
                .WithMany()
                .HasForeignKey(o => o.GerceklesmeSekliId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
        }
    }
}

