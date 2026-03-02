using System;

namespace EGM.Domain.Entities
{
    public class Sehit
    {
        public int Id { get; set; }
        public int OperasyonelFaaliyetId { get; set; }
        public OperasyonelFaaliyet? OperasyonelFaaliyet { get; set; }

        public string? Ad { get; set; }
        public string? Soyad { get; set; }
        public string? TcKimlikNo { get; set; } // encrypted
        public DateTime DogumTarihi { get; set; }
        public string? Gorev { get; set; } // örn: polis, asker
    }
}
