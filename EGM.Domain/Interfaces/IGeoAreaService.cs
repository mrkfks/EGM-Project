using EGM.Domain.Entities;
using NetTopologySuite.Geometries;

namespace EGM.Domain.Interfaces;

/// <summary>
/// Coğrafi alan sorgulamaları için arayüz.
/// </summary>
public interface IGeoAreaService
{
    /// <summary>
    /// Verilen latitude ve longitude koordinatlarının hangi idari alanın içinde olduğunu bulur.
    /// </summary>
    /// <param name="latitude">Enlem (Latitude)</param>
    /// <param name="longitude">Boylam (Longitude)</param>
    /// <param name="adminLevel">Filtrelemek için admin level (isteğe bağlı). NULL = tüm seviyeler</param>
    /// <param name="cancellationToken">İptal tokeni</param>
    /// <returns>Bulunduğu AdminArea, bulunamazsa null</returns>
    Task<AdminArea?> FindAdminAreaByCoordinatesAsync(
        double latitude, 
        double longitude, 
        int? adminLevel = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verilen noktanın içinde olduğu tüm idari alanları bulur.
    /// (Örnekse, mahalle, ilçe, il gibi hiyerarşik sonuçlar)
    /// </summary>
    /// <param name="latitude">Enlem</param>
    /// <param name="longitude">Boylam</param>
    /// <param name="cancellationToken">İptal tokeni</param>
    /// <returns>İçinde bulunan tüm AdminArea listesi</returns>
    Task<IReadOnlyList<AdminArea>> FindAllAdminAreasByCoordinatesAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// İllerin listesini döner (admin_level = 4)
    /// </summary>
    Task<IReadOnlyList<AdminArea>> GetProvincesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Verilen ilin ilçelerini döner
    /// </summary>
    Task<IReadOnlyList<AdminArea>> GetDistrictsByProvinceAsync(
        string provinceName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verilen ilçenin mahallelerini döner
    /// </summary>
    Task<IReadOnlyList<AdminArea>> GetNeighborhoodsByDistrictAsync(
        string districtName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// İl, ilçe veya mahalle adından geometri merkezinin (centroid) koordinatlarını döner.
    /// </summary>
    /// <param name="provinceName">İl adı (opsiyonel)</param>
    /// <param name="districtName">İlçe adı (opsiyonel)</param>
    /// <param name="adminLevel">Admin level (opsiyonel)</param>
    /// <param name="cancellationToken">İptal tokeni</param>
    /// <returns>(latitude, longitude) tuple veya null</returns>
    Task<(double latitude, double longitude)?> GetCoordinatesByAdminAreaAsync(
        string? provinceName = null,
        string? districtName = null,
        string? neighborhoodName = null,
        int? adminLevel = null,
        CancellationToken cancellationToken = default);
}
