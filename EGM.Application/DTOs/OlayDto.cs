namespace EGM.Application.DTOs
{
    public class OlayDto
    {
        public Guid Id { get; set; } // Id alanını geri ekliyorum.
        public string OlayTuru { get; set; } = string.Empty;
        public DateTime Tarih { get; set; }
        public string Il { get; set; } = string.Empty;
        public string Ilce { get; set; } = string.Empty;
        public string Mahalle { get; set; } = string.Empty;
        public int Hassasiyet { get; set; }
        public string Aciklama { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int? KatilimciSayisi { get; set; }
        public int? GozaltiSayisi { get; set; }
        public int? SehitOluSayisi { get; set; }
        public string? EvrakNumarasi { get; set; }
        public string? TakipNo { get; set; }
        public int? CityId { get; set; }
    }
}