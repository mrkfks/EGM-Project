using System;
using EGM.Domain.Attributes;

namespace EGM.Domain.Entities
{
    public class Sehit : BaseEntity
    {
        public Guid OperasyonelFaaliyetId { get; set; }
        public OperasyonelFaaliyet? OperasyonelFaaliyet { get; set; }

        public string? Ad { get; set; }
        public string? Soyad { get; set; }

        [Encrypted]
        public string? TcKimlikNo { get; set; }

        public DateTime DogumTarihi { get; set; }
        public string? Gorev { get; set; }
    }
}
