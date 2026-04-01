namespace EGM.Domain.Entities
{
    public class GerceklesmeSekli : BaseEntity
    {
        public string Name { get; set; } = null!;
        public Guid OlayTuruId { get; set; }
        public OlayTuru? OlayTuru { get; set; }
    }
}