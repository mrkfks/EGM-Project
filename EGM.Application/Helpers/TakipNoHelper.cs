namespace EGM.Application.Helpers
{
    /// <summary>
    /// Olay Numarası üretir.
    /// Format: yyyy.aa.gg.İlPlaka.Sıra
    /// Örnek: 2026.04.15.06.001 (2026-04-15, Ankara, günün 1. olayı)
    /// </summary>
    public static class TakipNoHelper
    {
        /// <summary>
        /// Olay numarasını üretir.
        /// </summary>
        /// <param name="tarih">Olayın başlangıç tarihi</param>
        /// <param name="plakaKodu">İl plaka kodu (1-81). Bilinmiyorsa 00.</param>
        /// <param name="siraNo">Aynı gün ve ildeki sıra numarası (1'den başlar).</param>
        public static string GenerateOlayNo(DateTime tarih, int plakaKodu, int siraNo)
            => $"{tarih:yyyy.MM.dd}.{plakaKodu:D2}.{siraNo:D3}";

        public const string SosyalMedya = "SM";
        public const string VipZiyaret = "VIP";
        public const string SecimOlay = "SEC";

        public static string Generate(string prefix, DateTime tarih, int plakaKodu, int siraNo)
        {
            return $"{prefix}-{tarih:yyyyMMdd}-{plakaKodu:D2}-{siraNo:D3}";
        }
    }
}

