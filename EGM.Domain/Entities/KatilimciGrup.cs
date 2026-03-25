using System;

namespace EGM.Domain.Entities
{
    public class KatilimciGrup : BaseEntity
    {
        public Guid OperasyonelFaaliyetId { get; set; }
        public OperasyonelFaaliyet? OperasyonelFaaliyet { get; set; }

        public string? GrupAdi { get; set; }
        public int? GrupKatilimciSayisi { get; set; }
    }
}
