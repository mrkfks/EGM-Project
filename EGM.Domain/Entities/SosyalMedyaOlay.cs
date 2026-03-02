using System;

namespace EGM.Domain.Entities
{
    public class SosyalMedyaOlay
    {
        public int Id { get; set; }
        public int OlayId { get; set; }
        public Olay? Olay { get; set; }

        public string? Platform { get; set; }
        public string? PaylasimLinki { get; set; }
        public DateTime PaylasimTarihi { get; set; }
        public string? IcerikOzeti { get; set; }
        public string? IlgiliKisiKurum { get; set; }

        public EGM.Domain.Enums.Hassasiyet Hassasiyet { get; set; }
        public double SosyalSignalSkoru { get; set; }
    }
}
