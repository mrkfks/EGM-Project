namespace EGM.Domain.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }

        public string Entity { get; set; } = string.Empty;   // Hangi entity üzerinde işlem yapıldı
        public int EntityId { get; set; }                   // Entity’nin primary key değeri
        public string Action { get; set; } = string.Empty;  // Create / Update / Delete
        public string UserId { get; set; } = string.Empty;  // İşlemi yapan kullanıcı
        public DateTime Timestamp { get; set; }             // İşlem zamanı

        public string Changes { get; set; } = string.Empty; // JSON formatında değişiklikler
    }
}
