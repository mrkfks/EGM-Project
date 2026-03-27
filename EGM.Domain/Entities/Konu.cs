using System.Collections.Generic;

namespace EGM.Domain.Entities
{
    public class Konu : BaseEntity
    {
        public string? Ad { get; set; }
        public string? Aciklama { get; set; }

        // Hiyerarşik yapı: Ana konu / alt konu
        public string? Tur { get; set; }            // örn: Ana Konu, İşçi Hakları, Emekli Hakları...
        public Guid? UstKonuId { get; set; }
        public Konu? UstKonu { get; set; }
        public ICollection<Konu> AltKonular { get; set; } = new List<Konu>();

        public ICollection<Olay> Olaylar { get; set; } = new List<Olay>();
    }
}
