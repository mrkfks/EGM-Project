using EGM.Domain.Entities;

namespace EGM.Domain.Interfaces
{
    public interface IInAppNotificationService
    {
        /// <summary>
        /// Bir olay oluşturulduğunda veya güncellendiğinde tetiklenir.
        /// RiskPuani > 0 ise risk bildirimi; isSelfCorrection = true ise
        /// "Self-Correction" başlığıyla İl Yöneticilerine gönderilir.
        /// </summary>
        Task NotifyOlayRiskAsync(Olay olay, bool isSelfCorrection = false);

        /// <summary>Bildirimi okundu olarak işaretler (yalnızca sahibi değiştirebilir).</summary>
        Task MarkAsReadAsync(Guid notificationId, string userId);

        /// <summary>Kullanıcıya ait okunmamış / tüm bildirimleri döner.</summary>
        Task MarkAllAsReadAsync(string userId);
        Task<IReadOnlyList<Notification>> GetUserNotificationsAsync(string userId);
    }
}
