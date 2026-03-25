using System;

namespace EGM.Domain.Entities
{
    public class YuruyusRota : BaseEntity
    {
        public Guid OlayId { get; set; }
        public Olay? Olay { get; set; }

        public string? NoktaAdi { get; set; } // örn: "Kızılay Meydanı", "Tandoğan"
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int SiraNo { get; set; } // 1 = başlangıç, n = bitiş
    }
}
