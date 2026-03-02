using System.Collections.Generic;

namespace EGM.Domain.Entities
{
    public class KategoriOrganizator
    {
        public int Id { get; set; }
        public string? Ad { get; set; }

        public ICollection<Organizator> Organizatorler { get; set; } = new List<Organizator>();
    }
}
