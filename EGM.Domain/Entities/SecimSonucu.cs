using System;

namespace EGM.Domain.Entities
{
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
