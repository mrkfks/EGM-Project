using EGM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EGM.Infrastructure
{
    public class EGMDbContext : DbContext
    {
        public EGMDbContext(DbContextOptions<EGMDbContext> options) : base(options)
        {
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── User ────────────────────────────────────────────────────
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Sicil)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

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
            modelBuilder.Entity<AuditLog>()
                .Property(a => a.Changes)
                .HasColumnType("TEXT");
        }
    }
}

