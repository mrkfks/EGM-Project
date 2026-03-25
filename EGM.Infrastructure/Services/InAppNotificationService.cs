using EGM.Domain.Constants;
using EGM.Domain.Entities;
using EGM.Domain.Enums;
using EGM.Domain.Interfaces;
using EGM.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace EGM.Infrastructure.Services
{
    public class InAppNotificationService : IInAppNotificationService
    {
        // Kritik risk eşiği: Hassasiyet.Kritik (20 puan) veya katılımcı > 1000 (10 puan)
        // RiskPuani > 0 → şehir yöneticileri; RiskPuani >= 20 → merkez yöneticileri de
        private const double CriticalRiskThreshold = 20.0;

        private readonly EGMDbContext             _context;
        private readonly IHubContext<NotificationHub> _hub;

        public InAppNotificationService(
            EGMDbContext context,
            IHubContext<NotificationHub> hub)
        {
            _context = context;
            _hub     = hub;
        }

        // ─── Olay Risk Bildirimi ─────────────────────────────────────────
        public async Task NotifyOlayRiskAsync(Olay olay, bool isSelfCorrection = false)
        {
            if (olay.RiskPuani <= 0 && !isSelfCorrection) return;

            var isCritical = olay.RiskPuani >= CriticalRiskThreshold;

            var title = isSelfCorrection
                ? $"[Düzeltme] Olay Güncellendi: {olay.Baslik}"
                : $"[Risk] Yüksek Riskli Olay: {olay.Baslik}";

            var message =
                $"Risk Puanı: {olay.RiskPuani} | " +
                $"Hassasiyet: {olay.Hassasiyet} | " +
                $"Tarih: {olay.Tarih:dd.MM.yyyy}" +
                (isSelfCorrection ? " | Kayıt sahibi tarafından düzeltildi." : string.Empty);

            var type = NotificationType.Risk;

            // ── Hedef kullanıcıları bul ───────────────────────────────
            var ilYoneticileri = await _context.Users
                .Where(u => u.Role == Roles.IlYoneticisi && !u.IsDeleted)
                .Where(u => !olay.CityId.HasValue || u.CityId == olay.CityId)
                .ToListAsync();

            var notifications = ilYoneticileri
                .Select(u => BuildNotification(u.Sicil.ToString(), title, message, olay.RiskPuani, type))
                .ToList();

            if (isCritical)
            {
                var hqManagers = await _context.Users
                    .Where(u => u.Role == Roles.BaskanlikYoneticisi && !u.IsDeleted)
                    .ToListAsync();

                notifications.AddRange(
                    hqManagers.Select(u => BuildNotification(u.Sicil.ToString(), title, message, olay.RiskPuani, type)));
            }

            if (notifications.Count == 0) return;

            // ── Bildirimleri kaydet ────────────────────────────────────
            await _context.Bildirimler.AddRangeAsync(notifications);

            var auditLogs = notifications.Select(n => new AuditLog
            {
                EntityName = nameof(Notification),
                EntityId   = n.Id,
                Action     = AuditAction.Create,
                UserId     = "system",
                Timestamp  = DateTime.UtcNow,
                Changes    = "In-App Notification Dispatched"
            }).ToList();

            await _context.AuditLoglar.AddRangeAsync(auditLogs);
            await _context.SaveChangesAsync();

            // ── SignalR üzerinden gönder ───────────────────────────────
            var payload = new
            {
                title,
                message,
                riskPuani = olay.RiskPuani,
                type = type.ToString(),
                olayId = olay.Id
            };

            if (olay.CityId.HasValue)
            {
                await _hub.Clients
                    .Group(NotificationGroupNames.CityManagers(olay.CityId.Value))
                    .SendAsync("ReceiveNotification", payload);
            }

            if (isCritical)
            {
                await _hub.Clients
                    .Group(NotificationGroupNames.HQManagers)
                    .SendAsync("ReceiveNotification", payload);
            }
        }

        // ─── Okundu İşaretle ─────────────────────────────────────────────
        public async Task MarkAsReadAsync(Guid notificationId, string userId)
        {
            var notification = await _context.Bildirimler
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId && !n.IsDeleted);

            if (notification == null) return;

            notification.IsRead = true;
            await _context.SaveChangesAsync();
        }

        // ─── Kullanıcı Bildirimleri ───────────────────────────────────────
        public async Task<IReadOnlyList<Notification>> GetUserNotificationsAsync(string userId)
        {
            return await _context.Bildirimler
                .Where(n => n.UserId == userId && !n.IsDeleted)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        // ─── Yardımcı ─────────────────────────────────────────────────────
        private static Notification BuildNotification(
            string userId, string title, string message, double riskScore, NotificationType type)
        {
            return new Notification
            {
                UserId    = userId,
                Title     = title,
                Message   = message,
                RiskScore = riskScore,
                Type      = type
            };
        }
    }
}
