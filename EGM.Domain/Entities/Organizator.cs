using System;
using System.Collections.Generic;

namespace EGM.Domain.Entities
{
    public class Organizator : BaseEntity
    {
        public string? Ad { get; set; }
        public DateTime KurulusTarihi { get; set; }
        public string? FaaliyetAlani { get; set; }
        public string? Iletisim { get; set; }

        public ICollection<Olay> Olaylar { get; set; } = new List<Olay>();
        public ICollection<KategoriOrganizator> Kategoriler { get; set; } = new List<KategoriOrganizator>();
    }
}
