using System;

namespace EGM.Domain.Entities
{
    public class Location : BaseEntity
    {
        public Guid OlayId { get; set; }
        public Olay? Olay { get; set; }
        
        public string? Il { get; set; }
        public string? Ilce { get; set; }
        public string? Mahalle { get; set; }
        public string? Mekan { get; set; }
        
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
