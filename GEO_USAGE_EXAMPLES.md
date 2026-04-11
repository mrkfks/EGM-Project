/**
 * GeoPackage Entegrasyonu - Kullanım Örnekleri
 * 
 * Bu dosya, TurkiyeRehber GeoPackage'ından coğrafi verileri sorgulayan
 * API endpoint'lerinin örnek kullanımlarını göstermektedir.
 */

// ========================================
// Örnek 1: Tekil Alan Sorgulama
// ========================================

// Request:
// GET /api/geo/find-area?latitude=41.0082&longitude=28.9784

// Response (Başarılı):
{
  "id": 42,
  "osmId": 1234567890,
  "name": "İstanbul",
  "adminLevel": 4,
  "adminLevelName": "İl"
}

// ========================================
// Örnek 2: Hiyerarşik Alan Sorgulama (Mahalle, İlçe, İl)
// ========================================

// Request:
// GET /api/geo/find-hierarchy?latitude=41.0082&longitude=28.9784

// Response:
[
  {
    "id": 1,
    "osmId": 3141234,
    "name": "İstanbul",
    "adminLevel": 4,
    "adminLevelName": "İl"
  },
  {
    "id": 12,
    "osmId": 3141567,
    "name": "Fatih",
    "adminLevel": 6,
    "adminLevelName": "İlçe"
  },
  {
    "id": 234,
    "osmId": 3141891,
    "name": "Sultanahmet Mahallesi",
    "adminLevel": 10,
    "adminLevelName": "Mahalle/Sokak"
  }
]

// ========================================
// Örnek 3: İlleri Listeleme
// ========================================

// Request:
// GET /api/geo/provinces

// Response:
[
  {
    "id": 1,
    "osmId": 67890123,
    "name": "Adana",
    "adminLevel": 4
  },
  {
    "id": 2,
    "osmId": 67890124,
    "name": "Adıyaman",
    "adminLevel": 4
  },
  ...
]

// ========================================
// Örnek 4: İlçeleri Listeleme
// ========================================

// Request:
// GET /api/geo/districts?provinceName=Istanbul

// Response:
[
  {
    "id": 12,
    "osmId": 3141567,
    "name": "Fatih",
    "adminLevel": 6
  },
  {
    "id": 13,
    "osmId": 3141568,
    "name": "Beyoğlu",
    "adminLevel": 6
  },
  ...
]

// ========================================
// Örnek 5: C# Kodunda Doğrudan Kullanım
// ========================================

public class EventService
{
    private readonly IGeoAreaService _geoAreaService;

    public EventService(IGeoAreaService geoAreaService)
    {
        _geoAreaService = geoAreaService;
    }

    // Etkinliğin mekansal bilgisini otomatik olarak belirle
    public async Task<string> DetermineEventLocationAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default)
    {
        // Verilen koordinata en yakın poligonu bul
        var area = await _geoAreaService.FindAdminAreaByCoordinatesAsync(
            latitude, longitude, adminLevel: 6, // İlçe seviyesinde ara
            cancellationToken);

        if (area != null)
        {
            return $"Etkinlik {area.Name} ilçesinde gerçekleşiyor.";
        }

        return "Konum belirlenemedi.";
    }

    // Hiyerarşik konum bilgisi al (Mahalle -> İlçe -> İl)
    public async Task<LocationHierarchy> GetLocationHierarchyAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default)
    {
        var areas = await _geoAreaService.FindAllAdminAreasByCoordinatesAsync(
            latitude, longitude, cancellationToken);

        var province = areas.FirstOrDefault(a => a.AdminLevel == 4)?.Name ?? "Bilinmiyor";
        var district = areas.FirstOrDefault(a => a.AdminLevel == 6)?.Name ?? "Bilinmiyor";
        var neighborhood = areas.FirstOrDefault(a => a.AdminLevel >= 8)?.Name ?? "Bilinmiyor";

        return new LocationHierarchy
        {
            Province = province,
            District = district,
            Neighborhood = neighborhood
        };
    }
}

// ========================================
// Örnek 6: Performans Tüyoları
// ========================================

/*
 * 1. Admin Level Filtresi Kullanın
 *    - FindAdminAreaByCoordinatesAsync(lat, lon, adminLevel: 4)
 *    - Sadece illeri kontrol etmek istiyorsanız, adminLevel=4 geçin
 *    - Varsayılan olarak tüm seviyeler taranır (yavaş)
 *
 * 2. Hiyerarşik Sorgularda Sınırlandırma
 *    - Eğer sadece ilçeyi ihtiyacınız varsa, adminLevel filtresi kullanın
 *
 * 3. Caching
 *    - Sık sorgulanan koordinatları cache'leyebilirsiniz
 *    - Örneğin: 41.0082, 28.9784 -> "İstanbul" sonucu bir saat boyunca cache'ley
 *
 * 4. Batch Sorgular (100+ koordinat)
 *    - Async/Await kullanarak parallel işleyin:
 *      var tasks = coordinates.Select(c => 
 *          _geoAreaService.FindAdminAreaByCoordinatesAsync(c.Lat, c.Lon)
 *      );
 *      var results = await Task.WhenAll(tasks);
 */

// ========================================
// Örnek 7: Pool İçinde Kullanım (ASP.NET Core)
// ========================================

/*
 * Program.cs'de:
 * 
 * builder.Services.AddDbContext<GeoContext>(options =>
 * {
 *     var geoConnection = builder.Configuration.GetConnectionString("GeoConnection");
 *     options.UseSqlite(
 *         geoConnection,
 *         x => x.UseNetTopologySuite()
 *     );
 * });
 * 
 * builder.Services.AddScoped<IGeoAreaService, GeoAreaService>();
 * 
 * // Sonra her controller'da inject edin:
 * [ApiController]
 * [Route("api/[controller]")]
 * public class EventController : ControllerBase
 * {
 *     private readonly IGeoAreaService _geoAreaService;
 *
 *     public EventController(IGeoAreaService geoAreaService)
 *     {
 *         _geoAreaService = geoAreaService;
 *     }
 * 
 *     [HttpPost("create-with-location")]
 *     public async Task<IActionResult> CreateEventWithLocation(
 *         double latitude,
 *         double longitude,
 *         string eventName)
 *     {
 *         var location = await _geoAreaService.FindAdminAreaByCoordinatesAsync(
 *             latitude, longitude);
 * 
 *         if (location == null)
 *             return BadRequest("Konum Türkiye sınırları dışında.");
 * 
 *         // Etkinliği oluştur...
 *         return Ok(new { eventName, location = location.Name });
 *     }
 * }
 */
