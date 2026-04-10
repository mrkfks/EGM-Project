namespace EGM.Domain.Interfaces;

public interface IGeoService
{
    /// <summary>Tüm il/bölge kaydı listesini döndürür (geometri hariç).</summary>
    Task<IReadOnlyList<GeoProvinceDto>> GetProvincesAsync(CancellationToken ct = default);

    /// <summary>Belirli bir kaydın GeoJSON geometrisini döndürür.</summary>
    Task<string?> GetGeometryGeoJsonAsync(int ogcFid, CancellationToken ct = default);
}

/// <summary>Geometri içermeyen il/bölge özet verisi.</summary>
public record GeoProvinceDto(
    int    OgcFid,
    string? OsmId,
    string? Name,
    string? AdminLevel,
    string? Boundary
);
