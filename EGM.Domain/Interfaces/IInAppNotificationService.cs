using EGM.Domain.Entities;

namespace EGM.Domain.Interfaces
{
    public interface IInAppNotificationService
    {
        /// <summary>
        /// Bir olay oluşturulduğunda veya güncellendiğinde tetiklenir.
        /// Hassasiyet >= Orta ise bildirim; isSelfCorrection = true ise
        /// "Self-Correction" başlığıyla İl Yöneticilerine gönderilir.
        /// </summary>
        Task NotifyOlayRiskAsync(Olay olay, bool isSelfCorrection = false);

        /// <summary>
        /// Arka plan servisi tarafından olay başlangıç saati geldiğinde tetiklenir.
        /// İlgili il personeli ve tüm başkanlık personeline "Olay Başladı" bildirimi gönderir.
        /// </summary>
        Task NotifyOlayBasladiAsync(Olay olay);

        /// <summary>Bildirimi okundu olarak işaretler (yalnızca sahibi değiştirebilir).</summary>
        Task MarkAsReadAsync(Guid notificationId, string userId);

        /// <summary>Kullanıcıya ait okunmamış / tüm bildirimleri döner.</summary>
        Task MarkAllAsReadAsync(string userId);
        Task<IReadOnlyList<Notification>> GetUserNotificationsAsync(string userId);
    }
}
