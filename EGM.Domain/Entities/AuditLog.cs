using System;
using EGM.Domain.Enums;

namespace EGM.Domain.Entities
{
    /// <summary>
    /// Sistem üzerindeki tüm Create/Update/Delete işlemlerinin denetim kaydı.
    /// AuditLog soft-delete'e tabi değildir; kayıtlar kalıcı saklanır.
    /// </summary>
    public class AuditLog : BaseEntity
    {
        public string EntityName { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public AuditAction Action { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Changes { get; set; } = string.Empty;
    }
}
