using EGM.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EGM.API.Controllers;

/// <summary>
/// Coğrafi veri (TurkiyeRehber.sqlite) erişim uç noktaları.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GeoController : ControllerBase
{
    private readonly IGeoService _geoService;

    public GeoController(IGeoService geoService)
    {
        _geoService = geoService;
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
}
