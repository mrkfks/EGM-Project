using EGM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace EGM.Infrastructure.Persistence;

/// <summary>
/// TurkiyeRehber GeoPackage dosyasından coğrafi verileri okumak için DbContext.
/// SQLite + SpatiaLite ile çalışır.
/// </summary>
public class GeoContext : DbContext
{
    public DbSet<AdminArea> AdminAreas { get; set; } = null!;

    public GeoContext(DbContextOptions<GeoContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // AdminArea tablosu yapılandırması
        modelBuilder.Entity<AdminArea>(entity =>
        {
            entity.ToTable("gis_osm_adminareas_a_free_1");
            entity.HasKey(e => e.Id);

            // OsmId - benzersiz ve indeksli olmalı
            entity.Property(e => e.OsmId)
                .HasColumnName("osm_id")
                .IsRequired();

            entity.HasIndex(e => e.OsmId)
                .IsUnique()
                .HasDatabaseName("idx_osm_id");

            // Name
            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(255)
                .IsRequired();

            entity.HasIndex(e => e.Name)
                .HasDatabaseName("idx_name");

            // AdminLevel
            entity.Property(e => e.AdminLevel)
                .HasColumnName("admin_level")
                .IsRequired();

            entity.HasIndex(e => e.AdminLevel)
                .HasDatabaseName("idx_admin_level");

            // Geometry - Mekansal sütun
            entity.Property(e => e.Geometry)
                .HasColumnName("geometry")
                .HasSrid(4326); // WGS84

            // SpatiaLite otomatik olarak rtree indeksleri oluşturur
            // Indeksi elle oluşturmak gerekmez, SpatiaLite bunu kendisi yönetir

            // CreatedAt
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}
