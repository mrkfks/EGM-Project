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
        // Hassasiyet >= Yüksek (2) → şehir yöneticileri; Hassasiyet == Kritik (3) → merkez yöneticileri de

        // Aynı olay için 30 saniye içinde tekrar bildirim gönderilmesini engelle
        private static readonly Dictionary<string, DateTime> _rateLimitMap = new();
        private static readonly TimeSpan _rateLimitWindow = TimeSpan.FromSeconds(30);

        private readonly EGMDbContext                 _context;
        private readonly IHubContext<NotificationHub> _hub;
        private readonly ICurrentUserService          _currentUser;

        public InAppNotificationService(
            EGMDbContext context,
            IHubContext<NotificationHub> hub,
            ICurrentUserService currentUser)
        {
            _context     = context;
            _hub         = hub;
            _currentUser = currentUser;
        }

        // ─── Olay Risk Bildirimi ─────────────────────────────────────────
        public async Task NotifyOlayRiskAsync(Olay olay, bool isSelfCorrection = false)
        {
            if (olay.Hassasiyet == Hassasiyet.Dusuk && !isSelfCorrection) return;

            // Rate limiting: aynı olay için 30 sn içinde tekrar bildirim gönderme
            var rateLimitKey = $"{olay.Id}_{(isSelfCorrection ? "correction" : "risk")}";
            lock (_rateLimitMap)
            {
                if (_rateLimitMap.TryGetValue(rateLimitKey, out var lastSent)
                    && DateTime.UtcNow - lastSent < _rateLimitWindow)
                    return;
                _rateLimitMap[rateLimitKey] = DateTime.UtcNow;
            }

            var isCritical = olay.Hassasiyet == Hassasiyet.Kritik;

            var title = isSelfCorrection
                ? $"[Düzeltme] Olay Güncellendi: {olay.OlayTuru ?? olay.Il ?? "Olay"}"
                : $"[Uyarı] Yüksek Hassasiyetli Olay: {olay.OlayTuru ?? olay.Il ?? "Olay"}";

            var message =
                $"Hassasiyet: {olay.Hassasiyet} | " +
                $"Tarih: {olay.Tarih:dd.MM.yyyy}" +
                (isSelfCorrection ? " | Kayıt sahibi tarafından düzeltildi." : string.Empty);

            var type = NotificationType.Risk;

            // ── Hedef kullanıcıları bul ───────────────────────────────
            // İl personeli + il yöneticileri (şehir bazlı)
            var ilKullanicilari = await _context.Users
                .Where(u => (u.Role == Roles.IlPersoneli || u.Role == Roles.IlYoneticisi) && !u.IsDeleted)
                .Where(u => !olay.CityId.HasValue || u.CityId == olay.CityId)
                .ToListAsync();

            var notifications = ilKullanicilari
                .Select(u => BuildNotification(u.Sicil.ToString(), title, message, (double)olay.Hassasiyet * 10, type))
                .ToList();

            // Kritik olaylar → tüm başkanlık personeline de gönder
            if (isCritical)
            {
                var hqKullanicilari = await _context.Users
                    .Where(u => (u.Role == Roles.BaskanlikPersoneli || u.Role == Roles.BaskanlikYoneticisi) && !u.IsDeleted)
                    .ToListAsync();

                notifications.AddRange(
                    hqKullanicilari.Select(u => BuildNotification(u.Sicil.ToString(), title, message, (double)olay.Hassasiyet * 10, type)));
            }

            if (notifications.Count == 0) return;

            // ── Bildirimleri kaydet ────────────────────────────────────
            await _context.Bildirimler.AddRangeAsync(notifications);

            // Tetikleyen kullanıcı kimliğini ve hedef grupları audit log'a yaz
            var triggeredBy = _currentUser.IsAuthenticated ? _currentUser.UserId : "system";
            var targetGroups = olay.CityId.HasValue
                ? $"city_{olay.CityId}" + (isCritical ? ", hq" : string.Empty)
                : isCritical ? "hq" : "broadcast";

            var auditLogs = notifications.Select(n => new AuditLog
            {
                EntityName = nameof(Notification),
                EntityId   = n.Id,
                Action     = AuditAction.Create,
                UserId     = triggeredBy,
                Timestamp  = DateTime.UtcNow,
                Changes    = $"Bildirim gönderildi → Gruplar: [{targetGroups}] | Hassasiyet: {olay.Hassasiyet}"
            }).ToList();

            await _context.AuditLoglar.AddRangeAsync(auditLogs);
            await _context.SaveChangesAsync();

            // ── SignalR üzerinden gönder ───────────────────────────────
            // Zengin payload: harita güncellemesi için koordinat + hassasiyet dahil
            var payload = new
            {
                title,
                message,
                hassasiyet = (int)olay.Hassasiyet,
                type       = type.ToString(),
                olayId     = olay.Id,
                latitude   = olay.Latitude,
                longitude  = olay.Longitude,
                cityId     = olay.CityId
            };

            if (olay.CityId.HasValue)
                await _hub.Clients
                    .Group(NotificationGroupNames.City(olay.CityId.Value))
                    .SendAsync("ReceiveNotification", payload);

            if (isCritical)
                await _hub.Clients
                    .Group(NotificationGroupNames.HQ)
                    .SendAsync("ReceiveNotification", payload);
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
                public async Task MarkAllAsReadAsync(string userId)
                {
                    var unread = await _context.Bildirimler
                        .Where(n => n.UserId == userId && !n.IsRead && !n.IsDeleted)
                        .ToListAsync();

                    if (unread.Count == 0) return;

                    foreach (var n in unread) n.IsRead = true;
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
