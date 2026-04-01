using System.Collections.Generic;

namespace EGM.Domain.Entities
{
    public class OlayTuru : BaseEntity
    {
        public string Name { get; set; } = null!;
        public ICollection<GerceklesmeSekli> GerceklesmeSekilleri { get; set; } = new List<GerceklesmeSekli>();
    }
}