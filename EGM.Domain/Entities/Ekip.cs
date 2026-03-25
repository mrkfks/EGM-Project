using System;

namespace EGM.Domain.Entities
{
    public class Ekip : BaseEntity
    {
        public string? Ad { get; set; }

        public Guid VIPZiyaretId { get; set; }
        public VIPZiyaret? VIPZiyaret { get; set; }
    }
}
