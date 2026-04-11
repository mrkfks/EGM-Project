using NetTopologySuite.Geometries;

namespace EGM.Domain.Entities;

/// <summary>
/// TurkiyeRehber GeoPackage dosyasından okunan idari alan verilerini temsil eder.
/// Türkiye'nin il, ilçe ve mahalle sınırlarını içerir.
/// </summary>
public class AdminArea
{
    /// <summary>
    /// Benzersiz tanımlayıcı
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// OpenStreetMap ID
    /// </summary>
    public long OsmId { get; set; }

    /// <summary>
    /// İdari alanın adı (il, ilçe, mahalle adı)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// OSM admin_level: 
    /// 2 = Country, 4 = Province (İl), 6 = District (İlçe), 8+ = Smaller divisions
    /// </summary>
    public int AdminLevel { get; set; }

    /// <summary>
    /// Çokgen geometrisi (SRID 4326 - WGS84)
    /// </summary>
    public MultiPolygon? Geometry { get; set; }

    /// <summary>
    /// Oluşturma tarihi
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
