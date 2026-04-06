using System;

namespace EGM.Domain.Entities
{
    public class SandikOlay : BaseEntity
    {
        public string? MusahitAdi { get; set; }
        public string? Il { get; set; }
        public string? Ilce { get; set; }
        public string? Mahalle { get; set; }
        public string? Okul { get; set; }
        public string? Konu { get; set; }
        public int SandikNo { get; set; }
        public string? OlayKategorisi { get; set; }
        public TimeSpan OlaySaati { get; set; }
        public string? Aciklama { get; set; }
        public string? KanitDosyasi { get; set; }
        public DateTime Tarih { get; set; }
        public int KatilimciSayisi { get; set; }
        public int SehitSayisi { get; set; }
        public int OluSayisi { get; set; }
        public int GozaltiSayisi { get; set; }
    }
}
