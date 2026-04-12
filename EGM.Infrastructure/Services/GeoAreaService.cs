using EGM.Domain.Entities;
using EGM.Domain.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EGM.Infrastructure.Services;

/// <summary>
/// Coğrafi alan sorgulamalarını yönetir.
/// Raw SQLite + R-tree bounding box sorguları kullanır (GeoPackage formatı).
/// </summary>
public class GeoAreaService : IGeoAreaService
{
    private readonly string _connectionString;
    private readonly ILogger<GeoAreaService> _logger;

    // Tablo ve R-tree isimleri
    private const string Table = "gis_osm_adminareas_a_free_1";
    private const string Rtree = "rtree_gis_osm_adminareas_a_free_1_geom";

    // code = 1200 + admin_level
    private const int ProvinceCode    = 1204; // admin_level 4
    private const int DistrictCode    = 1206; // admin_level 6
    private const int NeighborhoodMin = 1208; // admin_level 8+

    public GeoAreaService(IConfiguration configuration, ILogger<GeoAreaService> logger)
    {
        var cs = configuration.GetConnectionString("GeoConnection");
        if (string.IsNullOrWhiteSpace(cs))
            throw new InvalidOperationException("GeoConnection bağlantı dizesi yapılandırılmamış.");
        _connectionString = cs;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ── Yardımcı: satırdan AdminArea oluştur ─────────────────────────────
    private static AdminArea ReadArea(SqliteDataReader r)
    {
        var code = r.GetInt32(2);
        return new AdminArea
        {
            Id         = r.GetInt32(0),
            OsmId      = r.IsDBNull(1) ? 0 : (long.TryParse(r.GetString(1), out var v) ? v : 0),
            Name       = r.IsDBNull(3) ? string.Empty : r.GetString(3),
            AdminLevel = code - 1200,  // 1204→4, 1206→6, 1208→8
            Geometry   = null          // Geometry okunmuyor — SIGSEGV'den kaçınma
        };
    }

    /// <summary>
    /// Verilen lat/lon noktasının hangi idari alanın içinde olduğunu bulur (bounding-box).
    /// </summary>
    public async Task<AdminArea?> FindAdminAreaByCoordinatesAsync(
        double latitude,
        double longitude,
        int? adminLevel = null,
        CancellationToken cancellationToken = default)
    {
        var all = await FindAllAdminAreasByCoordinatesAsync(latitude, longitude, cancellationToken);
        if (adminLevel.HasValue)
            return all.FirstOrDefault(a => a.AdminLevel == adminLevel.Value);
        return all.FirstOrDefault();
    }

    /// <summary>
    /// Verilen noktanın bounding-box'ı içinde olan tüm idari alanları bulur.
    /// </summary>
    public async Task<IReadOnlyList<AdminArea>> FindAllAdminAreasByCoordinatesAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var results = new List<AdminArea>();
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = $"""
                SELECT a.fid, a.osm_id, a.code, a.name
                FROM   {Table} a
                JOIN   {Rtree} rt ON rt.id = a.fid
                WHERE  rt.minx <= $lon AND rt.maxx >= $lon
                  AND  rt.miny <= $lat AND rt.maxy >= $lat
                  AND  a.code IN ({ProvinceCode},{DistrictCode},{NeighborhoodMin})
                ORDER  BY a.code DESC
                """;
            cmd.Parameters.AddWithValue("$lon", longitude);
            cmd.Parameters.AddWithValue("$lat", latitude);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                results.Add(ReadArea(reader));

            _logger.LogInformation("({Lat}, {Lon}) için {Count} idari alan bulundu.",
                latitude, longitude, results.Count);
            return results.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FindAllAdminAreasByCoordinatesAsync sırasında hata");
            throw;
        }
    }

    /// <summary>
    /// İllerin listesini döner (code = 1204).
    /// </summary>
    public async Task<IReadOnlyList<AdminArea>> GetProvincesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var results = new List<AdminArea>();
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = $"""
                SELECT fid, osm_id, code, name
                FROM   {Table}
                WHERE  code = {ProvinceCode}
                ORDER  BY name
                """;

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                results.Add(ReadArea(reader));

            return results.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetProvincesAsync sırasında hata");
            throw;
        }
    }

    /// <summary>
    /// Verilen ilin bounding-box'ı içindeki ilçeleri döner (code = 1206).
    /// </summary>
    public async Task<IReadOnlyList<AdminArea>> GetDistrictsByProvinceAsync(
        string provinceName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            // 1) İlin bounding-box'ını R-tree'den al
            double pMinX, pMaxX, pMinY, pMaxY;
            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"""
                    SELECT rt.minx, rt.maxx, rt.miny, rt.maxy
                    FROM   {Table} a
                    JOIN   {Rtree} rt ON rt.id = a.fid
                    WHERE  a.code = {ProvinceCode}
                      AND  lower(a.name) = lower($name)
                    LIMIT  1
                    """;
                cmd.Parameters.AddWithValue("$name", provinceName);

                await using var r = await cmd.ExecuteReaderAsync(cancellationToken);
                if (!await r.ReadAsync(cancellationToken))
                    return Array.Empty<AdminArea>();

                pMinX = r.GetDouble(0); pMaxX = r.GetDouble(1);
                pMinY = r.GetDouble(2); pMaxY = r.GetDouble(3);
            }

            // 2) İlin bounding-box'ı içindeki ilçeleri bul
            var results = new List<AdminArea>();
            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"""
                    SELECT a.fid, a.osm_id, a.code, a.name
                    FROM   {Table} a
                    JOIN   {Rtree} rt ON rt.id = a.fid
                    WHERE  a.code = {DistrictCode}
                      AND  rt.minx >= $minx AND rt.maxx <= $maxx
                      AND  rt.miny >= $miny AND rt.maxy <= $maxy
                    ORDER  BY a.name
                    """;
                cmd.Parameters.AddWithValue("$minx", pMinX);
                cmd.Parameters.AddWithValue("$maxx", pMaxX);
                cmd.Parameters.AddWithValue("$miny", pMinY);
                cmd.Parameters.AddWithValue("$maxy", pMaxY);

                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                    results.Add(ReadArea(reader));
            }

            return results.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetDistrictsByProvinceAsync sırasında hata: {ProvinceName}", provinceName);
            throw;
        }
    }

    /// <summary>
    /// Verilen ilçenin bounding-box'ı içindeki mahalleleri döner (code >= 1208).
    /// </summary>
    public async Task<IReadOnlyList<AdminArea>> GetNeighborhoodsByDistrictAsync(
        string districtName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            // 1) İlçenin bounding-box'ını R-tree'den al
            double dMinX, dMaxX, dMinY, dMaxY;
            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"""
                    SELECT rt.minx, rt.maxx, rt.miny, rt.maxy
                    FROM   {Table} a
                    JOIN   {Rtree} rt ON rt.id = a.fid
                    WHERE  a.code = {DistrictCode}
                      AND  lower(a.name) = lower($name)
                    LIMIT  1
                    """;
                cmd.Parameters.AddWithValue("$name", districtName);

                await using var r = await cmd.ExecuteReaderAsync(cancellationToken);
                if (!await r.ReadAsync(cancellationToken))
                    return Array.Empty<AdminArea>();

                dMinX = r.GetDouble(0); dMaxX = r.GetDouble(1);
                dMinY = r.GetDouble(2); dMaxY = r.GetDouble(3);
            }

            // 2) İlçenin bounding-box'ı içindeki mahalleleri bul
            var results = new List<AdminArea>();
            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"""
                    SELECT a.fid, a.osm_id, a.code, a.name
                    FROM   {Table} a
                    JOIN   {Rtree} rt ON rt.id = a.fid
                    WHERE  a.code >= {NeighborhoodMin}
                      AND  rt.minx >= $minx AND rt.maxx <= $maxx
                      AND  rt.miny >= $miny AND rt.maxy <= $maxy
                    ORDER  BY a.name
                    """;
                cmd.Parameters.AddWithValue("$minx", dMinX);
                cmd.Parameters.AddWithValue("$maxx", dMaxX);
                cmd.Parameters.AddWithValue("$miny", dMinY);
                cmd.Parameters.AddWithValue("$maxy", dMaxY);

                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                    results.Add(ReadArea(reader));
            }

            return results.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetNeighborhoodsByDistrictAsync sırasında hata: {DistrictName}", districtName);
            throw;
        }
    }

    /// <summary>
    /// İl veya ilçe adından bounding-box merkez koordinatını döner.
    /// </summary>
    public async Task<(double latitude, double longitude)?> GetCoordinatesByAdminAreaAsync(
        string? provinceName = null,
        string? districtName = null,
        string? neighborhoodName = null,
        int? adminLevel = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(provinceName) &&
                string.IsNullOrWhiteSpace(districtName) &&
                string.IsNullOrWhiteSpace(neighborhoodName) &&
                !adminLevel.HasValue)
                return null;

            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            await using var cmd = conn.CreateCommand();

            if (!string.IsNullOrWhiteSpace(neighborhoodName))
            {
                cmd.CommandText = $"""
                    SELECT rt.minx, rt.maxx, rt.miny, rt.maxy
                    FROM   {Table} a
                    JOIN   {Rtree} rt ON rt.id = a.fid
                    WHERE  a.code >= {NeighborhoodMin}
                      AND  lower(a.name) = lower($name)
                    LIMIT  1
                    """;
                cmd.Parameters.AddWithValue("$name", neighborhoodName);
            }
            else if (!string.IsNullOrWhiteSpace(districtName))
            {
                cmd.CommandText = $"""
                    SELECT rt.minx, rt.maxx, rt.miny, rt.maxy
                    FROM   {Table} a
                    JOIN   {Rtree} rt ON rt.id = a.fid
                    WHERE  a.code = {DistrictCode}
                      AND  lower(a.name) = lower($name)
                    LIMIT  1
                    """;
                cmd.Parameters.AddWithValue("$name", districtName);
            }
            else if (!string.IsNullOrWhiteSpace(provinceName))
            {
                cmd.CommandText = $"""
                    SELECT rt.minx, rt.maxx, rt.miny, rt.maxy
                    FROM   {Table} a
                    JOIN   {Rtree} rt ON rt.id = a.fid
                    WHERE  a.code = {ProvinceCode}
                      AND  lower(a.name) = lower($name)
                    LIMIT  1
                    """;
                cmd.Parameters.AddWithValue("$name", provinceName);
            }
            else
            {
                int code = 1200 + adminLevel!.Value;
                cmd.CommandText = $"""
                    SELECT rt.minx, rt.maxx, rt.miny, rt.maxy
                    FROM   {Table} a
                    JOIN   {Rtree} rt ON rt.id = a.fid
                    WHERE  a.code = $code
                    LIMIT  1
                    """;
                cmd.Parameters.AddWithValue("$code", code);
            }

            await using var r = await cmd.ExecuteReaderAsync(cancellationToken);
            if (!await r.ReadAsync(cancellationToken))
                return null;

            double minx = r.GetDouble(0), maxx = r.GetDouble(1);
            double miny = r.GetDouble(2), maxy = r.GetDouble(3);

            double centerLon = (minx + maxx) / 2.0;
            double centerLat = (miny + maxy) / 2.0;

            _logger.LogInformation(
                "Koordinat bulundu: Province={Province}, District={District}, Neighborhood={Neighborhood} -> ({Lat:F6}, {Lon:F6})",
                provinceName, districtName, neighborhoodName, centerLat, centerLon);

            return (centerLat, centerLon);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "GetCoordinatesByAdminAreaAsync sırasında hata: Province={Province}, District={District}, Neighborhood={Neighborhood}",
                provinceName, districtName, neighborhoodName);
            throw;
        }
    }
}
