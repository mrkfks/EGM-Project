using EGM.Domain.Enums;

namespace EGM.Domain.Entities
{
    public class Notification : BaseEntity
    {
        /// <summary>Bildirimin hedeflendiği kullanıcının sicil numarası (string).</summary>
        public string UserId { get; set; } = string.Empty;

        public string Title   { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        /// <summary>Tetikleyen olayın risk puanı.</summary>
        public double RiskScore { get; set; }

        public bool IsRead { get; set; } = false;

        public NotificationType Type { get; set; }
    }
}
