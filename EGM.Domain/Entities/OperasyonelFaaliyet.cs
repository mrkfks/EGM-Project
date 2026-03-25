using System;
using System.Collections.Generic;
using System.Linq;

namespace EGM.Domain.Entities
{
    public class OperasyonelFaaliyet : BaseEntity
    {
        public Guid OlayId { get; set; }
        public Olay? Olay { get; set; }

        public string? Aciklama { get; set; }

        // Katılımcı grupları
        public ICollection<KatilimciGrup> KatilimciGruplar { get; set; } = new List<KatilimciGrup>();
        public int ToplamGrupSayisi => KatilimciGruplar.Count;

        // Şüpheliler
        public ICollection<Supheli> Supheliler { get; set; } = new List<Supheli>();

        // Şehitler ve ölüler
        public ICollection<Sehit> Sehitler { get; set; } = new List<Sehit>();
        public ICollection<Olu> Oluler { get; set; } = new List<Olu>();

        // Convenience properties
        public int SupheliSayisi => Supheliler.Count;
        public int GozaltiSayisi => Supheliler.Count(s => s.Gozaltinda);
        public int SehitSayisi => Sehitler.Count;
        public int OluSayisi => Oluler.Count;
    }
}
