using EGM.Application.Services;
using EGM.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EGM.API.Controllers;

/// <summary>
/// Coğrafi veri (TurkiyeRehber.sqlite) erişim uç noktaları.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class GeoController : ControllerBase
{
    private readonly IGeoService _geoService;
    private readonly IGeoAreaService _geoAreaService;
    private readonly OlayService _olayService;
    private readonly VIPZiyaretService _vipService;
    private readonly ILogger<GeoController> _logger;

    public GeoController(
        IGeoService geoService,
        IGeoAreaService geoAreaService,
        OlayService olayService,
        VIPZiyaretService vipService,
        ILogger<GeoController> logger)
    {
        _geoService = geoService;
        _geoAreaService = geoAreaService;
        _olayService = olayService;
        _vipService = vipService;
        _logger = logger;
    }

    /// <summary>
    /// Tüm il/bölge kayıtlarını listeler (geometri hariç).
    /// GET /api/geo/provinces
    /// </summary>
    [HttpGet("provinces")]
    public async Task<IActionResult> GetProvinces(CancellationToken ct)
    {
        var list = await _geoService.GetProvincesAsync(ct);
        return Ok(list);
    }

    /// <summary>
    /// TurkeyRehber GeoPackage'dan tüm illeri döner (İçerik = il adları).
    /// GET /api/geo/provinces-geopackage
    /// </summary>
    [AllowAnonymous]
    [HttpGet("provinces-geopackage")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProvincesFromGeoPackage(CancellationToken cancellationToken = default)
    {
        try
        {
            var provinces = await _geoAreaService.GetProvincesAsync(cancellationToken);
            
            var result = provinces
                .Select(p => new { name = p.Name, osmId = p.OsmId })
                .OrderBy(p => p.name)
                .ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetProvincesFromGeoPackage sırasında hata");
            return StatusCode(500, new { error = "İlleri getirme başarısız." });
        }
    }

    /// <summary>
    /// Verilen ile ait ilçeleri TurkeyRehber GeoPackage'dan döner.
    /// GET /api/geo/districts-geopackage?province=Istanbul
    /// </summary>
    [AllowAnonymous]
    [HttpGet("districts-geopackage")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetDistrictsFromGeoPackage(
        [FromQuery] string? province = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(province))
        {
            return BadRequest(new { error = "province parametresi gereklidir." });
        }

        try
        {
            var districts = await _geoAreaService.GetDistrictsByProvinceAsync(province, cancellationToken);
            
            var result = districts
                .Select(d => new { name = d.Name, osmId = d.OsmId })
                .OrderBy(d => d.name)
                .ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetDistrictsFromGeoPackage sırasında hata: {Province}", province);
            return StatusCode(500, new { error = "İlçeleri getirme başarısız." });
        }
    }

    /// <summary>
    /// Verilen ilçeye ait mahalleri TurkeyRehber GeoPackage'dan döner.
    /// GET /api/geo/neighborhoods-geopackage?district=Fatih
    /// </summary>
    [AllowAnonymous]
    [HttpGet("neighborhoods-geopackage")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetNeighborhoodsFromGeoPackage(
        [FromQuery] string? district = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(district))
        {
            return BadRequest(new { error = "district parametresi gereklidir." });
        }

        try
        {
            var neighborhoods = await _geoAreaService.GetNeighborhoodsByDistrictAsync(district, cancellationToken);
            
            var result = neighborhoods
                .Select(n => new { name = n.Name, osmId = n.OsmId })
                .OrderBy(n => n.name)
                .ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetNeighborhoodsFromGeoPackage sırasında hata: {District}", district);
            return StatusCode(500, new { error = "Mahalleri getirme başarısız." });
        }
    }

    /// <summary>
    /// Belirtilen kaydın sınır geometrisini GeoJSON olarak döndürür.
    /// GET /api/geo/provinces/{id}/geometry
    /// </summary>
    [HttpGet("provinces/{id:int}/geometry")]
    public async Task<IActionResult> GetGeometry(int id, CancellationToken ct)
    {
        var geoJson = await _geoService.GetGeometryGeoJsonAsync(id, ct);
        if (geoJson is null)
            return NotFound();

        // GeoJSON doğrudan JSON olarak dönülür (string sarmalanmaz)
        return Content(geoJson, "application/geo+json");
    }

    /// <summary>
    /// İl, ilçe, mahalle adından koordinat döner (geometri merkezi).
    /// GET /api/geo/get-coordinates?provinceName=Istanbul&districtName=Fatih
    /// </summary>
    [AllowAnonymous]
    [HttpGet("get-coordinates")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCoordinates(
        [FromQuery] string? provinceName = null,
        [FromQuery] string? districtName = null,
        [FromQuery] int? adminLevel = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(provinceName) && 
            string.IsNullOrWhiteSpace(districtName) && 
            !adminLevel.HasValue)
        {
            return BadRequest(new { error = "provinceName, districtName veya adminLevel gereklidir." });
        }

        try
        {
            var coordinates = await _geoAreaService.GetCoordinatesByAdminAreaAsync(
                provinceName, districtName, adminLevel, cancellationToken);

            if (coordinates == null)
                return NotFound(new { error = "Belirtilen alan bulunamadı." });

            return Ok(new
            {
                latitude = coordinates.Value.latitude,
                longitude = coordinates.Value.longitude
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetCoordinates sırasında hata: Province={Province}, District={District}",
                provinceName, districtName);
            return StatusCode(500, new { error = "Koordinat sorgulaması başarısız." });
        }
    }

    /// <summary>
    /// Verilen koordinatların hangi il/ilçe'de olduğunu bulur (reverse lookup).
    /// GET /api/geo/resolve-location?latitude=41.0082&longitude=28.9784
    /// </summary>
    [AllowAnonymous]
    [HttpGet("resolve-location")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResolveLocation(
        [FromQuery] double? latitude = null,
        [FromQuery] double? longitude = null,
        CancellationToken cancellationToken = default)
    {
        if (!latitude.HasValue || !longitude.HasValue)
        {
            return BadRequest(new { error = "latitude ve longitude parametreleri gereklidir." });
        }

        // Koordinat aralığını kontrol et
        if (latitude < -90 || latitude > 90 || longitude < -180 || longitude > 180)
        {
            return BadRequest(new { error = "Geçersiz koordinat değerleri. Enlem: -90 ~ +90, Boylam: -180 ~ +180" });
        }

        try
        {
            // Verilen koordinata göre tüm idari alanları bul (hiyerarşik)
            var areas = await _geoAreaService.FindAllAdminAreasByCoordinatesAsync(
                latitude.Value, longitude.Value, cancellationToken);

            if (areas == null || areas.Count == 0)
                return NotFound(new { error = "Belirtilen koordinata ait alan bulunamadı, Türkiye sınırları dışında." });

            // Hiyerarşik sonuçlar: admin_level 4 (il), 6 (ilçe), 8+ (mahalle)
            var province = areas.FirstOrDefault(a => a.AdminLevel == 4);
            var district = areas.FirstOrDefault(a => a.AdminLevel == 6);
            var neighborhood = areas.FirstOrDefault(a => a.AdminLevel >= 8);

            return Ok(new
            {
                province = province?.Name,
                district = district?.Name,
                neighborhood = neighborhood?.Name,
                allAreas = areas.OrderBy(a => a.AdminLevel).Select(a => new
                {
                    name = a.Name,
                    adminLevel = a.AdminLevel
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ResolveLocation sırasında hata: ({Latitude}, {Longitude})", 
                latitude, longitude);
            return StatusCode(500, new { error = "Konum çözümleme başarısız." });
        }
    }

    /// <summary>
    /// Harita için tüm olayları GeoJSON FeatureCollection olarak döndürür.
    /// İçerisinde: id, takipNo, olay türü, il/ilçe, tarih vs.
    /// GET /api/geo/olaylar?il=Istanbul&olay-turu=Gösteri
    /// </summary>
    [HttpGet("olaylar")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOlaylarForMap(
        [FromQuery] string? il = null,
        [FromQuery] string? olay_turu = null,
        [FromQuery] int? yil = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var features = new List<object>();

            // Tüm olayları al (sayfalı)
            var pagedOlaylar = await _olayService.GetAllAsync(
                page: 1,
                pageSize: 5000,  // Tüm olayları al
                durum: null,
                tarihBaslangic: yil.HasValue ? new DateTime(yil.Value, 1, 1) : null,
                tarihBitis: yil.HasValue ? new DateTime(yil.Value, 12, 31) : null
            );

            foreach (var olay in pagedOlaylar.Items)
            {
                // Filtreleme
                if (!string.IsNullOrWhiteSpace(il) && olay.Il != il)
                    continue;

                // Koordinatları kontrol et
                if (!olay.Latitude.HasValue || !olay.Longitude.HasValue)
                    continue;

                // GeoJSON Feature oluştur
                var feature = new
                {
                    type = "Feature",
                    id = olay.Id,
                    properties = new
                    {
                        takipNo = olay.TakipNo,
                        il = olay.Il,
                        ilce = olay.Ilce,
                        mekan = olay.Mekan,
                        tarih = olay.Tarih,
                        durum = olay.Durum.ToString(),
                        hassasiyet = olay.Hassasiyet.ToString(),
                        olayTuru = olay.OlayTuru,
                        aciklama = olay.Aciklama
                    },
                    geometry = new
                    {
                        type = "Point",
                        coordinates = new[] { olay.Longitude.Value, olay.Latitude.Value }
                    }
                };

                features.Add(feature);
            }

            var geoJsonCollection = new
            {
                type = "FeatureCollection",
                features = features
            };

            return Ok(geoJsonCollection);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetOlaylarForMap sırasında hata");
            return StatusCode(500, new { error = "Olayları getirme başarısız." });
        }
    }

    /// <summary>
    /// Harita için tüm VIP ziyaretlerini GeoJSON FeatureCollection olarak döndürür.
    /// GET /api/geo/vipziyaretler?il=Ankara
    /// </summary>
    [HttpGet("vipziyaretler")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVIPZiyaretlerForMap(
        [FromQuery] string? il = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var features = new List<object>();

            // Tüm VIP ziyaretlerini al
            var vipZiyaretler = await _vipService.GetAllAsync();

            foreach (var vip in vipZiyaretler.Where(v => !v.IsDeleted))
            {
                // Filtreleme
                if (!string.IsNullOrWhiteSpace(il) && vip.Il != il)
                    continue;

                // Koordinatları kontrol et
                if (!vip.Latitude.HasValue || !vip.Longitude.HasValue)
                    continue;

                // GeoJSON Feature oluştur
                var feature = new
                {
                    type = "Feature",
                    id = vip.Id,
                    properties = new
                    {
                        takipNo = vip.TakipNo,
                        ziyaretEdenAdSoyad = vip.ZiyaretEdenAdSoyad,
                        unvan = vip.Unvan,
                        il = vip.Il,
                        mekan = vip.Mekan,
                        baslangicTarihi = vip.BaslangicTarihi,
                        bitisTarihi = vip.BitisTarihi,
                        durum = vip.ZiyaretDurumu.ToString(),
                        hassasiyet = vip.Hassasiyet.ToString(),
                        guvenlikSeviyesi = vip.GuvenlikSeviyesi
                    },
                    geometry = new
                    {
                        type = "Point",
                        coordinates = new[] { vip.Longitude.Value, vip.Latitude.Value }
                    }
                };

                features.Add(feature);
            }

            var geoJsonCollection = new
            {
                type = "FeatureCollection",
                features = features
            };

            return Ok(geoJsonCollection);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetVIPZiyaretlerForMap sırasında hata");
            return StatusCode(500, new { error = "VIP ziyaretlerini getirme başarısız." });
        }
    }
}

