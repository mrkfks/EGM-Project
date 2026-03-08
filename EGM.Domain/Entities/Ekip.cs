namespace EGM.Domain.Entities
{
    public class Ekip
    {
        public int Id { get; set; }
        public string? Ad { get; set; }

        public int VIPZiyaretId { get; set; }
        public VIPZiyaret? VIPZiyaret { get; set; }
    }
}
