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

            // Supheli TcKimlikNo için max length
            modelBuilder.Entity<Supheli>()
                .Property(s => s.TcKimlikNo)
                .HasMaxLength(256);

            // KatilimciGrup ilişkisi
            modelBuilder.Entity<KatilimciGrup>()
                .HasOne(k => k.OperasyonelFaaliyet)
                .WithMany(f => f.KatilimciGruplar)
                .HasForeignKey(k => k.OperasyonelFaaliyetId);

            // Olay - Organizator ilişkisi
            modelBuilder.Entity<Olay>()
                .HasOne(o => o.Organizator)
                .WithMany(org => org.Olaylar)
                .HasForeignKey(o => o.OrganizatorId);

            // Olay - Konu ilişkisi
            modelBuilder.Entity<Olay>()
                .HasOne(o => o.Konu)
                .WithMany(k => k.Olaylar)
                .HasForeignKey(o => o.KonuId);

            // Olay - YuruyusRota ilişkisi
            modelBuilder.Entity<YuruyusRota>()
                .HasOne(r => r.Olay)
                .WithMany(o => o.YuruyusRotasi)
                .HasForeignKey(r => r.OlayId);

            modelBuilder.Entity<AuditLog>()
            .Property(a => a.Changes)
            .HasColumnType("nvarchar(max)");


        }
    }
}

