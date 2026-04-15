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

        public async Task NotifyOlayRiskAsync(Olay olay, bool isSelfCorrection = false)
        {
            var hassasiyet = olay.EventDetail?.Hassasiyet ?? Hassasiyet.Dusuk;
            if (hassasiyet == Hassasiyet.Dusuk && !isSelfCorrection) return;

            var rateLimitKey = $"{olay.Id}_{(isSelfCorrection ? "correction" : "risk")}";
            lock (_rateLimitMap)
            {
                if (_rateLimitMap.TryGetValue(rateLimitKey, out var lastSent)
                    && DateTime.UtcNow - lastSent < _rateLimitWindow)
                    return;
                _rateLimitMap[rateLimitKey] = DateTime.UtcNow;
            }

            var isCritical = hassasiyet == Hassasiyet.Kritik;
            var il = olay.Locations?.FirstOrDefault()?.Il ?? "Bilinmiyor";
            var tur = olay.Tur?.Name ?? "Olay";

            var title = isSelfCorrection
                ? $"[Düzeltme] Olay Güncellendi: {tur}"
                : $"[Uyarı] Yüksek Hassasiyetli Olay: {tur}";

            var message =
                $"Hassasiyet: {hassasiyet} | " +
                $"Tarih: {olay.BaslangicTarihi:dd.MM.yyyy}" +
                (isSelfCorrection ? " | Kayıt sahibi tarafından düzeltildi." : string.Empty);

            var type = NotificationType.Risk;

            var ilKullanicilari = await _context.Users
                .Where(u => (u.Role == Roles.IlPersoneli || u.Role == Roles.IlYoneticisi) && !u.IsDeleted)
                .Where(u => !olay.CityId.HasValue || u.CityId == olay.CityId)
                .ToListAsync();

            var notifications = ilKullanicilari
                .Select(u => BuildNotification(u.Sicil.ToString(), title, message, (double)hassasiyet * 10, type))
                .ToList();

            if (isCritical)
            {
                var hqKullanicilari = await _context.Users
                    .Where(u => (u.Role == Roles.BaskanlikPersoneli || u.Role == Roles.BaskanlikYoneticisi) && !u.IsDeleted)
                    .ToListAsync();

                notifications.AddRange(
                    hqKullanicilari.Select(u => BuildNotification(u.Sicil.ToString(), title, message, (double)hassasiyet * 10, type)));
            }

            if (notifications.Count == 0) return;

            await _context.Bildirimler.AddRangeAsync(notifications);

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
                Changes    = $"Bildirim gönderildi → Gruplar: [{targetGroups}] | Hassasiyet: {hassasiyet}"
            }).ToList();

            await _context.AuditLoglar.AddRangeAsync(auditLogs);
            await _context.SaveChangesAsync();

            var lat = olay.Locations?.FirstOrDefault()?.Latitude ?? 0;
            var lon = olay.Locations?.FirstOrDefault()?.Longitude ?? 0;

            var payload = new
            {
                title,
                message,
                hassasiyet = (int)hassasiyet,
                type       = type.ToString(),
                olayId     = olay.Id,
                latitude   = lat,
                longitude  = lon,
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

        public async Task NotifyOlayBasladiAsync(Olay olay)
        {
            var saatStr = olay.BaslangicTarihi.ToString("HH:mm");
            var tur = olay.Tur?.Name ?? "Olay";
            var il = olay.Locations?.FirstOrDefault()?.Il ?? "Bilinmiyor";
            var hassasiyet = olay.EventDetail?.Hassasiyet ?? Hassasiyet.Dusuk;

            var title   = $"[Başladı] {olay.OlayNo ?? tur} — {il}";
            var message = $"Olay başlangıç saatine ulaşıldı. " +
                          $"Tarih: {olay.BaslangicTarihi:dd.MM.yyyy} | Saat: {saatStr} | " +
                          $"Tür: {tur} | Hassasiyet: {hassasiyet}";
            var type    = NotificationType.Operasyonel;

            var ilKullanicilari = await _context.Users
                .Where(u => (u.Role == Roles.IlPersoneli || u.Role == Roles.IlYoneticisi) && !u.IsDeleted)
                .Where(u => !olay.CityId.HasValue || u.CityId == olay.CityId)
                .ToListAsync();

            var hqKullanicilari = await _context.Users
                .Where(u => (u.Role == Roles.BaskanlikPersoneli || u.Role == Roles.BaskanlikYoneticisi) && !u.IsDeleted)
                .ToListAsync();

            var notifications = ilKullanicilari
                .Concat(hqKullanicilari)
                .Select(u => BuildNotification(u.Sicil.ToString(), title, message, (double)hassasiyet * 10, type))
                .ToList();

            if (notifications.Count == 0) return;

            await _context.Bildirimler.AddRangeAsync(notifications);
            await _context.SaveChangesAsync();

            var lat = olay.Locations?.FirstOrDefault()?.Latitude ?? 0;
            var lon = olay.Locations?.FirstOrDefault()?.Longitude ?? 0;

            var payload = new
            {
                title,
                message,
                hassasiyet = (int)hassasiyet,
                type       = type.ToString(),
                olayId     = olay.Id,
                latitude   = lat,
                longitude  = lon,
                cityId     = olay.CityId
            };

            if (olay.CityId.HasValue)
                await _hub.Clients
                    .Group(NotificationGroupNames.City(olay.CityId.Value))
                    .SendAsync("ReceiveNotification", payload);

            await _hub.Clients
                .Group(NotificationGroupNames.HQ)
                .SendAsync("ReceiveNotification", payload);
        }

        public async Task MarkAsReadAsync(Guid notificationId, string userId)
        {
            var notification = await _context.Bildirimler
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId && !n.IsDeleted);

            if (notification == null) return;
            notification.IsRead = true;
            await _context.SaveChangesAsync();
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            var unread = await _context.Bildirimler
                .Where(n => n.UserId == userId && !n.IsRead && !n.IsDeleted)
                .ToListAsync();

            if (unread.Count == 0) return;
            foreach (var n in unread) n.IsRead = true;
            await _context.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<Notification>> GetUserNotificationsAsync(string userId)
        {
            return await _context.Bildirimler
                .Where(n => n.UserId == userId && !n.IsDeleted)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

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
