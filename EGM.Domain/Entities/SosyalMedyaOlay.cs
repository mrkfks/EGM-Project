using System;
using EGM.Domain.Enums;

namespace EGM.Domain.Entities
{
    public class SosyalMedyaOlay : BaseEntity
    {
        public Guid OlayId { get; set; }
        public Olay? Olay { get; set; }

        public string? Platform { get; set; }
        public string? PaylasimLinki { get; set; }
        public DateTime PaylasimTarihi { get; set; }
        public string? IcerikOzeti { get; set; }
        public string? IlgiliKisiKurum { get; set; }

        public Hassasiyet Hassasiyet { get; set; }
        public double SosyalSignalSkoru { get; set; }
    }
}
