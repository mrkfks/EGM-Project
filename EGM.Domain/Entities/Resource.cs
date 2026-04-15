using System;

namespace EGM.Domain.Entities
{
    public class Resource : BaseEntity
    {
        public Guid OlayId { get; set; }
        public Olay? Olay { get; set; }
        
        public string? Platform { get; set; }
        public string? KullaniciAdi { get; set; }
        public string? Link { get; set; }
        public string? GorselPath { get; set; }
        public string? Aciklama { get; set; }
    }
}
