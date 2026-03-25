using System.Collections.Generic;

namespace EGM.Domain.Entities
{
    public class Konu : BaseEntity
    {
        public string? Ad { get; set; }
        public string? Aciklama { get; set; }

        public ICollection<Olay> Olaylar { get; set; } = new List<Olay>();
    }
}
