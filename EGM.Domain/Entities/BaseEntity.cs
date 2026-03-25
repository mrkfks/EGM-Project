namespace EGM.Domain.Entities
{
    /// <summary>
    /// Tüm domain entity'leri için ortak taban sınıf.
    /// Id (Guid), CreatedAt, UpdatedAt ve soft-delete desteği sağlar.
    /// </summary>
    public abstract class BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Kaydı oluşturan kullanıcının sicil numarası (string).
        /// Servis katmanı tarafından otomatik doldurulur.
        /// </summary>
        public string CreatedByUserId { get; set; } = string.Empty;
    }
}
