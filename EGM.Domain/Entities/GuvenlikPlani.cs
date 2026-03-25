using System;

namespace EGM.Domain.Entities
{
    public class GuvenlikPlani : BaseEntity
    {
        public string? Ad { get; set; }
        public string? Aciklama { get; set; }

        public Guid VIPZiyaretId { get; set; }
        public VIPZiyaret? VIPZiyaret { get; set; }
    }
}
