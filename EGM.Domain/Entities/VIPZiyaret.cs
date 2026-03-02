using System.Collections.Generic;
using EGM.Domain.Enums;

namespace EGM.Domain.Entities
{
    public class VIPZiyaret
    {
        public int Id { get; set; }
        public string? ZiyaretEdenAdSoyad { get; set; }
        public string? Unvan { get; set; }
        public DateTime BaslangicTarihi { get; set; }
        public DateTime BitisTarihi { get; set; }
        public string? Il { get; set; }
        public string? Mekan { get; set; }
        public Hassasiyet Hassasiyet { get; set; }
        public string? GuvenlikSeviyesi { get; set; }

        public ICollection<GuvenlikPlani> GuvenlikPlanlari { get; set; } = new List<GuvenlikPlani>();
        public ICollection<Ekip> EkipAtamasi { get; set; } = new List<Ekip>();
        public string? GozlemNoktalari { get; set; }
    }
}
