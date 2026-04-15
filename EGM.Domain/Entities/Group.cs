using System.Collections.Generic;

namespace EGM.Domain.Entities
{
    public class Group : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        
        public ICollection<Olay> Events { get; set; } = new List<Olay>();
    }
}
