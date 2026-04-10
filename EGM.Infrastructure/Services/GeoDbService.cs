using EGM.Domain.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace EGM.Infrastructure.Services;

/// <summary>
/// TurkiyeRehber.sqlite (SpatiaLite) veritabanını sadece-okuma modunda sorgular.
/// EF Core kullanılmaz; bağlantı Microsoft.Data.Sqlite üzerinden açılır.
/// </summary>
public sealed class GeoDbService : IGeoService
{
    private readonly string _connectionString;

    // SpatiaLite binary header boyutu (39 byte: srid + endian + mbr + marker)
    private const int SpatialiteHeaderSize = 39;

    public GeoDbService(IConfiguration configuration)
    {
        var cs = configuration.GetConnectionString("GeoConnection");
        if (string.IsNullOrWhiteSpace(cs))
            throw new InvalidOperationException("GeoConnection bağlantı dizesi yapılandırılmamış.");
        _connectionString = cs;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<GeoProvinceDto>> GetProvincesAsync(CancellationToken ct = default)
    {
        var results = new List<GeoProvinceDto>();

        await using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync(ct);

        // SpatiaLite eklentisini yükle
        await LoadSpatiaLiteAsync(conn, ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT ogc_fid, osm_id, name, admin_level, boundary
            FROM   turkey260409osm__multipolygons
            ORDER  BY name
            """;

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            results.Add(new GeoProvinceDto(
                OgcFid:     reader.GetInt32(0),
                OsmId:      reader.IsDBNull(1) ? null : reader.GetString(1),
                Name:       reader.IsDBNull(2) ? null : reader.GetString(2),
                AdminLevel: reader.IsDBNull(3) ? null : reader.GetString(3),
                Boundary:   reader.IsDBNull(4) ? null : reader.GetString(4)
            ));
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task<string?> GetGeometryGeoJsonAsync(int ogcFid, CancellationToken ct = default)
    {
        await using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync(ct);

        // SpatiaLite eklentisini yükle
        await LoadSpatiaLiteAsync(conn, ct);

        // SpatiaLite GEOMETRY blob'u raw bayt olarak okunur.
        // İlk 39 bayt SpatiaLite başlığıdır; geri kalanı standart WKB'dir.
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT GEOMETRY FROM turkey260409osm__multipolygons WHERE ogc_fid = $id";
        cmd.Parameters.AddWithValue("$id", ogcFid);

        var rawObj = await cmd.ExecuteScalarAsync(ct);
        if (rawObj is null or DBNull)
            return null;

        var blob = (byte[])rawObj;
        if (blob.Length <= SpatialiteHeaderSize)
            return null;

        // Standart WKB kısmını çıkar (39. bayttan sona kadar; son bayt 0xFE marker'ı hariç)
        var wkbBytes = blob[SpatialiteHeaderSize..^1];

        return ParseWkbToGeoJson(wkbBytes);
    }

    // SpatiaLite DLL yükleyici
    private static async Task LoadSpatiaLiteAsync(SqliteConnection conn, CancellationToken ct)
    {
        try
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT load_extension('mod_spatialite')";
            await cmd.ExecuteNonQueryAsync(ct);
        }
        catch (Exception ex)
        {
            // Yüklenemezse logla ama uygulamayı durdurma
            Console.WriteLine($"SpatiaLite yüklenemedi: {ex.Message}");
        }
    }

    // ── WKB → GeoJSON ────────────────────────────────────────────────────

    /// <summary>
    /// Standart WKB baytlarını minimal GeoJSON string'e dönüştürür.
    /// Sadece POLYGON / MULTIPOLYGON tiplerini destekler.
    /// </summary>
    private static string? ParseWkbToGeoJson(byte[] wkb)
    {
        try
        {
            int pos = 0;

            byte byteOrder = wkb[pos++];
            bool isLe = byteOrder == 1; // 1 = little-endian

            uint geomType = ReadUInt32(wkb, ref pos, isLe);

            return geomType switch
            {
                3  => PolygonGeoJson(wkb, ref pos, isLe),
                6  => MultiPolygonGeoJson(wkb, ref pos, isLe),
                _  => null   // desteklenmeyen tip
            };
        }
        catch
        {
            return null;
        }
    }

    private static string PolygonGeoJson(byte[] wkb, ref int pos, bool le)
    {
        var sb = new StringBuilder("{\"type\":\"Polygon\",\"coordinates\":[");
        uint ringCount = ReadUInt32(wkb, ref pos, le);
        for (uint r = 0; r < ringCount; r++)
        {
            if (r > 0) sb.Append(',');
            sb.Append(ReadLinearRing(wkb, ref pos, le));
        }
        sb.Append("]}");
        return sb.ToString();
    }

    private static string MultiPolygonGeoJson(byte[] wkb, ref int pos, bool le)
    {
        var sb = new StringBuilder("{\"type\":\"MultiPolygon\",\"coordinates\":[");
        uint polyCount = ReadUInt32(wkb, ref pos, le);
        for (uint p = 0; p < polyCount; p++)
        {
            if (p > 0) sb.Append(',');
            sb.Append('[');

            // Her polygon kendi WKB başlığıyla gelir
            pos++;              // byteOrder
            ReadUInt32(wkb, ref pos, le); // geomType (3 = Polygon)

            uint ringCount = ReadUInt32(wkb, ref pos, le);
            for (uint r = 0; r < ringCount; r++)
            {
                if (r > 0) sb.Append(',');
                sb.Append(ReadLinearRing(wkb, ref pos, le));
            }
            sb.Append(']');
        }
        sb.Append("]}");
        return sb.ToString();
    }

    private static string ReadLinearRing(byte[] wkb, ref int pos, bool le)
    {
        var sb = new StringBuilder("[");
        uint ptCount = ReadUInt32(wkb, ref pos, le);
        for (uint i = 0; i < ptCount; i++)
        {
            if (i > 0) sb.Append(',');
            double x = ReadDouble(wkb, ref pos, le);
            double y = ReadDouble(wkb, ref pos, le);
            sb.Append($"[{x.ToString("G10", System.Globalization.CultureInfo.InvariantCulture)},{y.ToString("G10", System.Globalization.CultureInfo.InvariantCulture)}]");
        }
        sb.Append(']');
        return sb.ToString();
    }

    private static uint ReadUInt32(byte[] buf, ref int pos, bool le)
    {
        var bytes = buf[pos..(pos + 4)];
        pos += 4;
        if (le != BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        return BitConverter.ToUInt32(bytes, 0);
    }

    private static double ReadDouble(byte[] buf, ref int pos, bool le)
    {
        var bytes = buf[pos..(pos + 8)];
        pos += 8;
        if (le != BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        return BitConverter.ToDouble(bytes, 0);
    }
}
