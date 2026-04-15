using System;
using EGM.Domain.Enums;

namespace EGM.Domain.Entities
{
    public class EventDetail : BaseEntity
    {
        public Guid OlayId { get; set; }
        public Olay? Olay { get; set; }
        
        public Hassasiyet Hassasiyet { get; set; }
        public int KatilimciSayisi { get; set; }
        public int SupheliSayisi { get; set; }
        public int GozaltiSayisi { get; set; }
        public int SehitSayisi { get; set; }
        public int OluSayisi { get; set; }
    }
}
