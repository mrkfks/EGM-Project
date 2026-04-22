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
        public DbSet<YuruyusRota> YuruyusRotasi { get; set; } = null!;

        // Organizator ve Konu
        public DbSet<Organizator> Organizatorler { get; set; } = null!;
        public DbSet<KategoriOrganizator> KategoriOrganizatorler { get; set; } = null!;
        public DbSet<Konu> Konular { get; set; } = null!;

        // Audit
        public DbSet<AuditLog> AuditLoglar { get; set; } = null!;

        // Bildirimler
        public DbSet<Notification> Bildirimler { get; set; } = null!;

        public DbSet<OlayTuru> OlayTurleri { get; set; } = null!;
        public DbSet<GerceklesmeSekli> GerceklesmeSekilleri { get; set; } = null!;

        // Yeni İlişkisel Yapı
        public DbSet<Resource> Resources { get; set; } = null!;
        public DbSet<Location> Locations { get; set; } = null!;
        public DbSet<EventDetail> EventDetails { get; set; } = null!;
        public DbSet<Group> Groups { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Global soft-delete query filter ─────────────────────────
            // BaseEntity'den türeyen tüm entity'lerde IsDeleted=true kayıtlar
            // otomatik olarak sorgu dışında bırakılır.
            modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Olay>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<YuruyusRota>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Organizator>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<KategoriOrganizator>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Konu>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Notification>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Resource>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Location>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<EventDetail>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Group>().HasQueryFilter(e => !e.IsDeleted);

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
                .HasIndex(o => o.OlayNo)
                .IsUnique();

            modelBuilder.Entity<Olay>()
                .HasIndex(o => o.CityId);

            modelBuilder.Entity<Olay>()
                .HasIndex(o => o.BaslangicTarihi);

            modelBuilder.Entity<Olay>()
                .HasIndex(o => o.Durum);

            modelBuilder.Entity<Olay>()
                .HasIndex(o => o.PersonelId);

            // ── Olay İlişkileri ──────────────────────────────────────────
            modelBuilder.Entity<Olay>()
                .HasMany(o => o.Resources)
                .WithOne(r => r.Olay)
                .HasForeignKey(r => r.OlayId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Olay>()
                .HasMany(o => o.Locations)
                .WithOne(l => l.Olay)
                .HasForeignKey(l => l.OlayId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Olay>()
                .HasOne(o => o.EventDetail)
                .WithOne(d => d.Olay)
                .HasForeignKey<EventDetail>(d => d.OlayId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Olay>()
                .HasMany(o => o.ParticipantOrganizators)
                .WithMany(org => org.KatilimciOlduguOlaylar)
                .UsingEntity(j => j.ToTable("OlayKatilimciOrganizator"));

            modelBuilder.Entity<Olay>()
                .HasOne(o => o.Tur)
                .WithMany()
                .HasForeignKey(o => o.TurId);

            modelBuilder.Entity<Olay>()
                .HasOne(o => o.Sekil)
                .WithMany()
                .HasForeignKey(o => o.SekilId);

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

            // ── YuruyusRota - Olay ──────────────────────────────────────
            modelBuilder.Entity<YuruyusRota>()
                .HasOne(r => r.Olay)
                .WithMany()
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


        }
    }
}

