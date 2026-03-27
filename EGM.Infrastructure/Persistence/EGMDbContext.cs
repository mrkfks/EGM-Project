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
        public DbSet<OperasyonelFaaliyet> OperasyonelFaaliyetler { get; set; } = null!;
        public DbSet<KatilimciGrup> KatilimciGruplar { get; set; } = null!;
        public DbSet<Supheli> Supheliler { get; set; } = null!;
        public DbSet<Sehit> Sehitler { get; set; } = null!;
        public DbSet<Olu> Oluler { get; set; } = null!;
        public DbSet<SosyalMedyaOlay> SosyalMedyaOlaylar { get; set; } = null!;
        public DbSet<YuruyusRota> YuruyusRotasi { get; set; } = null!;

        // Organizator ve Konu
        public DbSet<Organizator> Organizatorler { get; set; } = null!;
        public DbSet<KategoriOrganizator> KategoriOrganizatorler { get; set; } = null!;
        public DbSet<Konu> Konular { get; set; } = null!;

        // Seçim
        public DbSet<SandikOlay> SandikOlaylar { get; set; } = null!;
        public DbSet<SecimSonucu> SecimSonuclari { get; set; } = null!;
        public DbSet<Aday> Adaylar { get; set; } = null!;
        public DbSet<Parti> Partiler { get; set; } = null!;
        public DbSet<SecimKaynak> SecimKaynaklar { get; set; } = null!;

        // VIP Ziyaret ve Güvenlik
        public DbSet<VIPZiyaret> VIPZiyaretler { get; set; } = null!;
        public DbSet<GuvenlikPlani> GuvenlikPlanlari { get; set; } = null!;
        public DbSet<Ekip> Ekipler { get; set; } = null!;

        // Audit
        public DbSet<AuditLog> AuditLoglar { get; set; } = null!;

        // Bildirimler
        public DbSet<Notification> Bildirimler { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Global soft-delete query filter ─────────────────────────
            // BaseEntity'den türeyen tüm entity'lerde IsDeleted=true kayıtlar
            // otomatik olarak sorgu dışında bırakılır.
            modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Olay>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<OperasyonelFaaliyet>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<KatilimciGrup>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Supheli>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Sehit>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Olu>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<SosyalMedyaOlay>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<YuruyusRota>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Organizator>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<KategoriOrganizator>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Konu>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<SandikOlay>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<SecimSonucu>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Aday>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Parti>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<SecimKaynak>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<VIPZiyaret>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<GuvenlikPlani>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Ekip>().HasQueryFilter(e => !e.IsDeleted);
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

            // ── OperasyonelFaaliyet ──────────────────────────────────────
            modelBuilder.Entity<OperasyonelFaaliyet>()
                .HasIndex(f => f.OlayId);

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

            // ── Supheli ─────────────────────────────────────────────────
            modelBuilder.Entity<Supheli>()
                .Property(s => s.TcKimlikNo)
                .HasMaxLength(256);

            modelBuilder.Entity<Supheli>()
                .HasOne(s => s.OperasyonelFaaliyet)
                .WithMany(f => f.Supheliler)
                .HasForeignKey(s => s.OperasyonelFaaliyetId);

            // ── Sehit ───────────────────────────────────────────────────
            modelBuilder.Entity<Sehit>()
                .Property(s => s.TcKimlikNo)
                .HasMaxLength(256);

            modelBuilder.Entity<Sehit>()
                .HasOne(s => s.OperasyonelFaaliyet)
                .WithMany(f => f.Sehitler)
                .HasForeignKey(s => s.OperasyonelFaaliyetId);

            // ── Olu ─────────────────────────────────────────────────────
            modelBuilder.Entity<Olu>()
                .Property(o => o.TcKimlikNo)
                .HasMaxLength(256);

            modelBuilder.Entity<Olu>()
                .HasOne(o => o.OperasyonelFaaliyet)
                .WithMany(f => f.Oluler)
                .HasForeignKey(o => o.OperasyonelFaaliyetId);

            // ── KatilimciGrup ───────────────────────────────────────────
            modelBuilder.Entity<KatilimciGrup>()
                .HasOne(k => k.OperasyonelFaaliyet)
                .WithMany(f => f.KatilimciGruplar)
                .HasForeignKey(k => k.OperasyonelFaaliyetId);

            // ── OperasyonelFaaliyet - Olay ──────────────────────────────
            modelBuilder.Entity<OperasyonelFaaliyet>()
                .HasOne(f => f.Olay)
                .WithMany(o => o.OperasyonelFaaliyetler)
                .HasForeignKey(f => f.OlayId);

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

            // ── SecimSonucu ilişkileri ───────────────────────────────────
            modelBuilder.Entity<SecimSonucu>()
                .HasOne(s => s.Aday)
                .WithMany()
                .HasForeignKey(s => s.AdayId);

            modelBuilder.Entity<SecimSonucu>()
                .HasOne(s => s.Parti)
                .WithMany()
                .HasForeignKey(s => s.PartiId);

            modelBuilder.Entity<SecimSonucu>()
                .HasOne(s => s.Kaynak)
                .WithMany()
                .HasForeignKey(s => s.KaynakId);

            // ── VIPZiyaret - GuvenlikPlani ───────────────────────────────
            modelBuilder.Entity<GuvenlikPlani>()
                .HasOne(g => g.VIPZiyaret)
                .WithMany(v => v.GuvenlikPlanlari)
                .HasForeignKey(g => g.VIPZiyaretId);

            // ── VIPZiyaret - Ekip ───────────────────────────────────────
            modelBuilder.Entity<Ekip>()
                .HasOne(e => e.VIPZiyaret)
                .WithMany(v => v.EkipAtamasi)
                .HasForeignKey(e => e.VIPZiyaretId);

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
        }
    }
}

