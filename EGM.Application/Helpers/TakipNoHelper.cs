namespace EGM.Application.Helpers
{
    /// <summary>
    /// Olay tiplerine göre benzersiz takip numarası üretir.
    /// Format: [TipKodu]-[YYYYMMDD][PlakaKodu(2 hane)]-[SıraNo(3 hane)]
    ///
    /// Tip kodları:
    ///   SO = Sokak Olayı
    ///   SM = Sosyal Medya Olayı
    ///   SC = Seçim (Sandık) Olayı
    ///   VZ = VIP Ziyaret
    ///
    /// Örnek: SO-2026040806-001 (2026-04-08, Ankara, günün 1. sokak olayı)
    /// </summary>
    public static class TakipNoHelper
    {
        public const string SokakOlay      = "SO";
        public const string SosyalMedya    = "SM";
        public const string SecimOlay      = "SC";
        public const string VipZiyaret     = "VZ";

        /// <summary>
        /// Takip numarasını üretir.
        /// </summary>
        /// <param name="tipKodu">SO / SM / SC / VZ</param>
        /// <param name="tarih">Olayın tarihi</param>
        /// <param name="plakaKodu">İl plaka kodu (1-81). Bilinmiyorsa 0.</param>
        /// <param name="siraNo">Aynı gün ve ildeki sıra numarası (1'den başlar).</param>
        public static string Generate(string tipKodu, DateTime tarih, int plakaKodu, int siraNo)
            => $"{tipKodu}-{tarih:yyyyMMdd}{plakaKodu:D2}-{siraNo:D3}";
    }
}
