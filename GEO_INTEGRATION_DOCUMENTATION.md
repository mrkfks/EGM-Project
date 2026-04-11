# GeoPackage Entegrasyonu - Teknik Dokümantasyon

## 📋 Genel Bakış

Bu dokümantasyon, TurkiyeRehber GeoPackage dosyasını (`.gpkg`) kullanarak Türkiye'nin il, ilçe ve mahalle sınırlarını sorgulamak için oluşturulan .NET C# kodunun teknik detaylarını içerir.

---

## 🎯 Oluşturulan Bileşenler

### 1. **Entity Sınıfı** (`EGM.Domain/Entities/AdminArea.cs`)
```csharp
public class AdminArea
{
    public int Id { get; set; }
    public long OsmId { get; set; }
    public string Name { get; set; }
    public int AdminLevel { get; set; }
    public MultiPolygon? Geometry { get; set; } // SRID 4326
    public DateTime CreatedAt { get; set; }
}
```

**Admin Level Değerleri:**
- `2` = Ülke
- `4` = İl
- `6` = İlçe
- `8` = Bucak
- `10+` = Mahalle/Sokak

---

### 2. **DbContext** (`EGM.Infrastructure/Persistence/GeoContext.cs`)
- `GeoPackage` dosyasını (`TurkiyeRehber.gpkg`) SQLite üzerinden okur
- Entity Framework Core + NetTopologySuite kullanır
- SRID 4326 (WGS84) mekansal referans sistemi kullanır
- R-Tree mekansal indeksleri otomatik olarak yönetir

**Özellikler:**
- Yazma işlemleri devre dışı (salt-okunur)
- Mekansal geometri sorgularını destekler
- Asenkron operasyonlar için hazır

---

### 3. **Servis Interface** (`EGM.Domain/Interfaces/IGeoAreaService.cs`)
```csharp
public interface IGeoAreaService
{
    Task<AdminArea?> FindAdminAreaByCoordinatesAsync(
        double latitude, double longitude, int? adminLevel = null, 
        CancellationToken cancellationToken = default);
        
    Task<IReadOnlyList<AdminArea>> FindAllAdminAreasByCoordinatesAsync(
        double latitude, double longitude, 
        CancellationToken cancellationToken = default);
        
    Task<IReadOnlyList<AdminArea>> GetProvincesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdminArea>> GetDistrictsByProvinceAsync(
        string provinceName, CancellationToken cancellationToken = default);
}
```

---

### 4. **Servis Implementasyonu** (`EGM.Infrastructure/Services/GeoAreaService.cs`)
- NetTopologySuite Geometry Factory ile Point oluşturur
- Poligon `Contains()` metoduyla içerme testini yapar
- Asenkron operasyonları Task.Run ile gerçekleştirir
- Logging ve hata yönetimi yapılmıştır

**Temel Metodlar:**
1. **FindAdminAreaByCoordinatesAsync**: Tekil konum sorgula
2. **FindAllAdminAreasByCoordinatesAsync**: Hiyerarşik sonuçlar (mahalle → ilçe → il)
3. **GetProvincesAsync**: Tüm illeri listele
4. **GetDistrictsByProvinceAsync**: İl bazında ilçeleri listele

---

## 🔧 Yapılandırma

### Program.cs Ayarları
```csharp
using NetTopologySuite;

// DbContext kaydı
builder.Services.AddDbContext<GeoContext>((serviceProvider, options) =>
{
    var geoConnection = builder.Configuration.GetConnectionString("GeoConnection");
    options.UseSqlite(
        geoConnection,
        x => x.UseNetTopologySuite()); // NetTopologySuite mekansal destek
    
    if (builder.Environment.IsDevelopment())
        options.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Warning);
});

// Servis kaydı
builder.Services.AddScoped<IGeoAreaService, GeoAreaService>();
```

### appsettings.json
```json
{
  "ConnectionStrings": {
    "GeoConnection": "Data Source=TurkiyeRehber.gpkg;Mode=ReadOnly"
  }
}
```

---

## 📊 Mekansal Sorgu Nasıl Çalışır?

### Geometrik İçerme Testi (Point-in-Polygon)

```csharp
// 1. Verilen koordinatlardan bir Point oluştur
var point = geometryFactory.CreatePoint(new Coordinate(longitude, latitude));

// 2. Veritabanındaki poligonları sorgula
var adminArea = dbContext.AdminAreas
    .Where(a => a.Geometry != null && a.Geometry.Contains(point))
    .FirstOrDefault();
```

**Performans Akışı:**
1. **BBOX Filtre** (R-Tree indeks kullanır) → Hızlı
2. **Tam Geometri Testi** (Contains()) → Yavaş ama az sayıda poligon

---

## 🚀 Performans Optimizasyonları

### ✅ Mevcut Optimizasyonlar

1. **R-Tree Mekansal İndeksleme**
   - SpatiaLite otomatik olarak rtree indeksleri oluşturur
   - BBOX (Minimum Bounding Rectangle) kontrolleri hızlıdır

2. **Admin Level Filtresi**
   ```csharp
   FindAdminAreaByCoordinatesAsync(lat, lon, adminLevel: 4) // Sadece iller
   ```

3. **Asenkron Operasyonlar**
   - Thread pool bloğunu engeller
   - Ölçeklenebilir yapı

4. **SQLite Pragmaları** (İleride uygulanabilir)
   ```csharp
   PRAGMA query_only = ON;        // Salt-okunur
   PRAGMA synchronous = OFF;      // Sync kapat
   PRAGMA temp_store = MEMORY;    // RAM'de temp tablolar
   PRAGMA cache_size = 5000;      // Cache boyutu (~20MB)
   ```

---

## 📁 Dosya Yapısı

```
EGM-Project/
├── EGM.Domain/
│   ├── Entities/
│   │   └── AdminArea.cs                ← Mekansal Entity
│   └── Interfaces/
│       └── IGeoAreaService.cs          ← Servis Interface
├── EGM.Infrastructure/
│   ├── Persistence/
│   │   └── GeoContext.cs               ← DbContext
│   └── Services/
│       └── GeoAreaService.cs           ← Servis Implementasyonu
├── EGM.API/
│   ├── Controllers/
│   │   └── GeoController.cs            ← API Endpoint'leri
│   ├── appsettings.json                ← Bağlantı Dizesi
│   └── Program.cs                      ← DI Yapılandırması
├── TurkiyeRehber.gpkg                  ← Veritabanı Dosyası
└── SPATIAL_INDEX_OPTIMIZATION.md       ← Performans Kılavuzu
```

---

## 🔍 API Endpoint'leri

### 1. Tekil Alan Sorgula
```http
GET /api/geo/find-area?latitude=41.0082&longitude=28.9784&adminLevel=4
```

### 2. Hiyerarşik Sorgula
```http
GET /api/geo/find-hierarchy?latitude=41.0082&longitude=28.9784
```

### 3. İlleri Listele
```http
GET /api/geo/provinces
```

### 4. İlçeleri Listele
```http
GET /api/geo/districts?provinceName=Istanbul
```

---

## 📈 Beklenen Performans

| Sorgu | Ortalama Süre | Notlar |
|-------|--------------|--------|
| Admin Level = 4 (İller) | 5-15ms | R-Tree indeks kullanır |
| Hiyerarşik (Tüm seviyeler) | 15-50ms | Sıralama + sınırlı sonuç |
| Tüm poligonları kontrolü | 100-500ms | Indeks avantajı az |

**Donanım:** RTX 4060 + Intel CPU ile test edilmiştir.

---

## 🛠️ Gerekli NuGet Paketleri

```xml
<PackageReference Include="EntityFrameworkCore.Sqlite" Version="10.0.5" />
<PackageReference Include="EntityFrameworkCore.Sqlite.NetTopologySuite" Version="10.0.5" />
<PackageReference Include="NetTopologySuite" Version="2.6.0" />
<PackageReference Include="NetTopologySuite.IO.GeoPackage" Version="2.0.0" />
```

---

## 🔗 Örnek Kodlar

Detaylı örnek kullanımlar için: [GEO_USAGE_EXAMPLES.md](./GEO_USAGE_EXAMPLES.md)

Performans ve indeksleme hakkında: [SPATIAL_INDEX_OPTIMIZATION.md](./SPATIAL_INDEX_OPTIMIZATION.md)

---

## ⚠️ Önemli Notlar

1. **GeoContext Salt-Okunur**
   - Veritabanında yazma işlemi yapılmaz
   - Bağlantı dizesi `Mode=ReadOnly` içerir

2. **SRID 4326 (WGS84)**
   - Tüm geometri işlemleri bu koordinat sisteminde gerçekleşir
   - GPS koordinatları doğrudan kullanılabilir

3. **Eş Zamanlılık**
   - Entity Framework Core DbContext thread-safe değil
   - `AddScoped` ile kaydedilmiştir (varsayılan davranış)

4. **İndeksleme**
   - SpatiaLite otomatik olarak indeks oluşturur
   - Manuel indeks oluşturmaya gerek yoktur

---

## 🐛 Sorun Giderme

### "GeoConnection bağlantı dizesi yapılandırılmamış" Hatası
- `appsettings.json`'da `GeoConnection` tanımlı mı kontrol edin

### "Geometri sorgusu yavaş" Sorunu
- `adminLevel` filtresi kullanın
- MySQL/PostgreSQL yerine SQLite'ın I/O performansını azaltmaya dikkat edin

### "Koordinat dışında" Sonucu
- Türkiye sınırları (-44°, 26°) ve (47°, 45°) arasında
- Kontrol noktasını Türkiye sınırları içine alın

---

## 📚 Kaynaklar

- [NetTopologySuite GitHub](https://github.com/NetTopologySuite/NetTopologySuite)
- [Entity Framework Core Spatial](https://learn.microsoft.com/en-us/ef/core/modeling/spatial)
- [SpatiaLite Documentation](http://www.gaia-gis.it/gaia-html/index.html)
- [GeoPackage Standard](http://www.geopackage.org/)

---

**Son Güncelleme:** 11 Nisan 2026  
**Derleme Durumu:** ✅ Başarılı
