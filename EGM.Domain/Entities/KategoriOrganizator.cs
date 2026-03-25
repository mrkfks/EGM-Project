using System.Collections.Generic;

namespace EGM.Domain.Entities
{
    public class KategoriOrganizator : BaseEntity
    {
        public string? Ad { get; set; }

        public ICollection<Organizator> Organizatorler { get; set; } = new List<Organizator>();
    }
}
