namespace EGM.Domain.Entities
{
    public class GuvenlikPlani
    {
        public int Id { get; set; }
        public string? Ad { get; set; }
        public string? Aciklama { get; set; }

        public int VIPZiyaretId { get; set; }
        public VIPZiyaret? VIPZiyaret { get; set; }
    }
}
