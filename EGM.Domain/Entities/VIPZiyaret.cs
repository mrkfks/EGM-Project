using System;
using EGM.Domain.Enums;

namespace EGM.Domain.Entities
{
    public class VIPZiyaret : BaseEntity
    {
        public string? ZiyaretEdenAdSoyad { get; set; }
        public string? Unvan { get; set; }
        public DateTime BaslangicTarihi { get; set; }
        public DateTime BitisTarihi { get; set; }
        public string? Il { get; set; }
        public string? Mekan { get; set; }
        public Hassasiyet Hassasiyet { get; set; }
        public string? GuvenlikSeviyesi { get; set; }
        public string? GozlemNoktalari { get; set; }
        public ZiyaretDurumu ZiyaretDurumu { get; set; } = ZiyaretDurumu.Planlandi;
    }
}
