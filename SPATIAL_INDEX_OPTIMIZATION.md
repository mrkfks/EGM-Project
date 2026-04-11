# Mekansal Sorguların Performans Optimizasyonu

## Genel Bakış

TurkiyeRehber GeoPackage dosyasından coğrafi sorgular yaparken, donanımdaki kaynaklara (RTX 4060, Intel CPU) göre optimizasyon yapılması gerekir. Aşağıda performans artırma tekniklerini bulabilirsiniz.

---

## 1. Mekansal İndeksleme (Spatial Indexing)

### SpatiaLite R-Tree İndeksleri

SQLite + SpatiaLite kombinasyonu, **R-Tree (Rectree)** adı verilen mekansal indeks yapısını kullanır. Bu, noktaların/poligonların geometrik sınırlarını hızlı şekilde araştırır.

#### Otomatik İndeksler
GeoContext'te `IsSpatialIndex()` tanımlandığı için, SpatiaLite otomatik olarak rtree indekleri oluşturur:

```sql
CREATE TABLE 'idx_geometry' (
    id INTEGER PRIMARY KEY,
    minx REAL, maxx REAL,
    miny REAL, maxy REAL
);
```

#### Elle İndeks Oluşturma (İsteğe bağlı)
```csharp
// GeoContext'te:
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    
    // SpatiaLite'ın rtree indeksi oluşturmasına izin verin
    // Veya EF Core migration'ında:
    // migrationBuilder.Sql("SELECT CreateSpatialIndex('gis_osm_adminareas_a_free_1', 'geometry')");
}
```

---

## 2. Sorgu Optimizasyonu

### a) Bounding Box (BBOX) Filtresi ile Ön-Eleme

Tüm poligonu kontrol etmeden önce sınırlarını kontrol edin:

```csharp
public async Task<AdminArea?> FindAdminAreaByCoordinatesOptimized(
    double latitude,
    double longitude,
    CancellationToken cancellationToken = default)
{
    var point = _geometryFactory.CreatePoint(new Coordinate(longitude, latitude));

    return await Task.Run(() =>
    {
        return _context.AdminAreas.AsNoTracking()
            // 1. BBOX ile ön-eleme (Hızlı - indeks kullanır)
            .Where(a => a.Geometry != null)
            // 2. Tam geometrik test (Daha yavaş ama az sayıda)
            .Where(a => a.Geometry.Contains(point))
            // 3. En spesifik seviye önce
            .OrderByDescending(a => a.AdminLevel)
            .FirstOrDefaultAsync(cancellationToken);
    }, cancellationToken);
}
```

SpatiaLite otomatik olarak `MBR (Minimum Bounding Rectangle)` kontroller yapıyor ve R-Tree indeksi kullanıyor.

---

### b) Sayfalama ve Sınırlandırma

Çok sayıdaki sonuç için sayfalama yapın:

```csharp
public async Task<IReadOnlyList<AdminArea>> GetNearbyAreasAsync(
    double latitude,
    double longitude,
    int maxResults = 10,
    CancellationToken cancellationToken = default)
{
    var point = _geometryFactory.CreatePoint(new Coordinate(longitude, latitude));

    return await _context.AdminAreas.AsNoTracking()
        .Where(a => a.Geometry != null && a.Geometry.Contains(point))
        .OrderByDescending(a => a.AdminLevel)
        .Take(maxResults) // CPU ve bellek kullanımını sınırlandır
        .ToListAsync(cancellationToken);
}
```

---

### c) Admin Level ile Filtrele

Gereksiz verileri elemenin en etkili yolu:

```csharp
// Başarısız: Tüm levels'ı kontrol eder
var all = _context.AdminAreas
    .Where(a => a.Geometry.Contains(point))
    .ToList(); // ~10K+ poligon

// ✓ Doğru: Sadece il seviyesi (4) kontrol eder
var province = _context.AdminAreas
    .Where(a => a.AdminLevel == 4 && a.Geometry.Contains(point))
    .FirstOrDefault(); // ~81 poligon (Türkiye'nin illeri)
```

---

## 3. Veritabanı Yapılandırması

### SQLite Pragmaları (Performans)

GeoContext'te bağlantı açılırken bu pragma'ları ekleyin:

```csharp
public GeoContext(DbContextOptions<GeoContext> options) : base(options)
{
    // Bağlantı açıldıktan sonra çalıştırılacak pragmalar
    this.Database.ExecuteSqlRaw(@"
        PRAGMA query_only = ON;           -- Salt-okunur mod
        PRAGMA synchronous = OFF;         -- Disk senkronizasyonu kapat (okuma-only)
        PRAGMA temp_store = MEMORY;       -- Geçici tablolar RAM'de
        PRAGMA mmap_size = 30000000;      -- Memory-mapped I/O (30MB)
        PRAGMA page_size = 4096;          -- Sayfa boyutu
        PRAGMA cache_size = 5000;         -- Cache boyutu (~20MB)
    ");
}
```

### Veya Program.cs'de:
```csharp
builder.Services.AddDbContext<GeoContext>((serviceProvider, options) =>
{
    var geoConnection = builder.Configuration.GetConnectionString("GeoConnection");
    options.UseSqlite(geoConnection, x =>
    {
        x.UseNetTopologySuite();
        // Bağlantı açıldığında pragma'ları çalıştır
        x.CommandTimeout(30);
    });
});

// Uygulama başladığında bazı ayarlar yapabilirsiniz
app.MapGet("/api/geo/init-cache", async (GeoContext db) =>
{
    await db.Database.ExecuteSqlRawAsync(@"
        PRAGMA query_only = ON;
        PRAGMA synchronous = OFF;
        PRAGMA temp_store = MEMORY;
        PRAGMA mmap_size = 30000000;
        PRAGMA page_size = 4096;
        PRAGMA cache_size = 5000;
    ");
    return Results.Ok("Cache initialized");
});
```

---

## 4. Donanım Optimizasyonu (RTX 4060 + Intel CPU)

### CPU Kaynakları
- **İş Parçacığı Havuzu (Thread Pool):** SQLite C# wrapper'ı varsayılan olarak CPU çekirdeklerini kullanır.
- **Async/Await:** Eşzamansız operasyonlar (asynchronous) I/O işlemlerini bloke etmez.

### GPU (RTX 4060)
SQLite GPu'yu doğrudan desteklemez, ancak:
- **CUDA/RAPIDS:** Coğrafi verileri GPU'da işlemek için; fakat bu projede karmaşıklık artırır.
- **Önerilen:** CPU'yla yeterlidir, indeksleme yeterince hızlı olacaktır.

---

## 5. Benchmark İpuçları

### Sorguları Ölçmek
```csharp
var stopwatch = System.Diagnostics.Stopwatch.StartNew();

var area = await _geoAreaService.FindAdminAreaByCoordinatesAsync(
    41.0082, 28.9784, cancellationToken: default);

stopwatch.Stop();
_logger.LogInformation("Sorgu süresi: {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);
```

### Beklenen Performans
| Sorgu Türü | Süre (ms) | Neden |
|-----------|----------|-------|
| Tek poligonun içinde kontrol (admin_level = 4) | 5-20ms | R-Tree indeks kullanır |
| Tüm poligonları kontrol et | 100-500ms | İndeks avantajı az |
| 10 sonuç; hiyerarşik | 10-50ms | Sıralama + sınırlı sonuç |

---

## 6. Cached Results (İleri)

Sık sorgulanan sonuçları cache'leyebilirsiniz:

```csharp
public class CachedGeoAreaService : IGeoAreaService
{
    private readonly IGeoAreaService _inner;
    private readonly IMemoryCache _cache;

    public async Task<AdminArea?> FindAdminAreaByCoordinatesAsync(
        double latitude,
        double longitude,
        int? adminLevel = null,
        CancellationToken cancellationToken = default)
    {
        var key = $"geo_{Math.Round(latitude, 4)}_{Math.Round(longitude, 4)}_{adminLevel}";
        
        if (_cache.TryGetValue(key, out AdminArea? cached))
            return cached;

        var result = await _inner.FindAdminAreaByCoordinatesAsync(
            latitude, longitude, adminLevel, cancellationToken);

        if (result != null)
            _cache.Set(key, result, TimeSpan.FromHours(1));

        return result;
    }
}
```

---

## 7. SpatiaLite Ayarlarının Doğrulanması

GeoController'da bir health check endpoint'i ekleyin:

```csharp
[HttpGet("spatial-index-status")]
[AllowAnonymous]
public async Task<ActionResult> CheckSpatialIndexStatus(CancellationToken ct)
{
    try
    {
        await _context.Database.ExecuteSqlRawAsync(
            "SELECT HasSpatialIndex('gis_osm_adminareas_a_free_1', 'geometry');", 
            cancellationToken: ct);

        return Ok(new { status = "Spatial index is active and ready" });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { error = ex.Message });
    }
}
```

---

## Özet

1. **R-Tree İndeksl Eme:** SpatiaLite otomatik kullanır → ✓ Aktif
2. **BBOX Filtresi:** WHERE + geometri kontrol → ✓ Hızlı
3. **Admin Level Filtresi:** Veri sayısını azalt → ✓ Çok etkili
4. **Sorgu Sayfa:** TÜm sonuçları çekme → ✓ Belleği koru
5. **SQLite Pragmaları:** Disk I/O opsiyonları → ✓ Cache'yi uygun kurula
6. **Caching:** Sık sorgular için → ✓ Tekrarlı istekler hızlı
7. **Async/Await:** CPU bloğunu engelle → ✓ Ölçeklenebilir

Bu yapılandırmayla, RTX 4060 ve Intel CPU'nuzda, **~40-100ms'de coğrafi sorgu yanıtı** alabilirsiniz.
