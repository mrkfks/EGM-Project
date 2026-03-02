using System;

namespace EGM.Domain.Entities
{
    public class SecimSonucu
    {
        public int Id { get; set; }
        public string? SecimTuru { get; set; }
        public DateTime Tarih { get; set; }
        public string? BolgeTipi { get; set; }
        public int BolgeId { get; set; }

        public int AdayId { get; set; }
        public Aday? Aday { get; set; }

        public int PartiId { get; set; }
        public Parti? Parti { get; set; }

        public int OySayisi { get; set; }
        public double OyOrani { get; set; }

        public int KaynakId { get; set; }
        public SecimKaynak? Kaynak { get; set; }
        public bool KaynakOnayDurumu { get; set; }
    }

    public class Aday
    {
        public int Id { get; set; }
        public string? AdSoyad { get; set; }
        public string? PartiAdi { get; set; }
    }

    public class Parti
    {
        public int Id { get; set; }
        public string? Ad { get; set; }
    }

    public class SecimKaynak
    {
        public int Id { get; set; }
        public string? KaynakAdi { get; set; }
    }
}
