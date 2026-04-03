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

    public class SecimSonucu : BaseEntity
    {
        public string? SecimTuru { get; set; }
        public DateTime Tarih { get; set; }
        public string? BolgeTipi { get; set; }
        public int BolgeId { get; set; }

        public Guid AdayId { get; set; }
        public Aday? Aday { get; set; }

        public Guid PartiId { get; set; }
        public Parti? Parti { get; set; }

        public int OySayisi { get; set; }
        public double OyOrani { get; set; }

        public Guid KaynakId { get; set; }
        public SecimKaynak? Kaynak { get; set; }
        public bool KaynakOnayDurumu { get; set; }
    }

    public class Aday : BaseEntity
    {
        public string? AdSoyad { get; set; }
        public string? PartiAdi { get; set; }
    }

    public class Parti : BaseEntity
    {
        public string? Ad { get; set; }
    }

    public class SecimKaynak : BaseEntity
    {
        public string? KaynakAdi { get; set; }
    }
}
