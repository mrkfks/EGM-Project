using EGM.Domain.Enums;
using EGM.Domain.Interfaces;
using EGM.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EGM.API.BackgroundServices
{
    /// <summary>
    /// Her dakika çalışır; başlangıç zamanı gelmiş ancak bildirim gönderilmemiş
    /// olayları tespit ederek otomatik olarak "Devam Eden" statüsüne çeker ve bildirim gönderir.
    /// </summary>
    public class OlayStartNotifierService : BackgroundService
    {
        private static readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OlayStartNotifierService> _logger;

        public OlayStartNotifierService(
            IServiceScopeFactory scopeFactory,
            ILogger<OlayStartNotifierService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger       = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OlayStartNotifierService başlatıldı.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingNotificationsAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "OlayStartNotifierService beklenmeyen hata.");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }

        private async Task ProcessPendingNotificationsAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var context     = scope.ServiceProvider.GetRequiredService<EGMDbContext>();
            var notifier    = scope.ServiceProvider.GetRequiredService<IInAppNotificationService>();

            var now = DateTime.UtcNow;

            // Başlangıç tarihi gelmiş, henüz bildirilmemiş ve Planlanan durumundaki olaylar
            var bekleyenOlaylar = await context.Olaylar
                .Where(o =>
                    !o.IsDeleted &&
                    !o.BaslangicBildirimiGonderildi &&
                    o.BaslangicTarihi <= now &&
                    o.Durum == OlayDurum.Planlanan)
                .ToListAsync(ct);

            if (bekleyenOlaylar.Count == 0) return;

            _logger.LogInformation(
                "{Count} olay için başlangıç bildirimi gönderilecek ve statü güncellenecek.",
                bekleyenOlaylar.Count);

            foreach (var olay in bekleyenOlaylar)
            {
                try
                {
                    await notifier.NotifyOlayBasladiAsync(olay);
                    olay.BaslangicBildirimiGonderildi = true;
                    
                    // Durumu otomatik olarak "Devam Eden" yap
                    olay.Durum = OlayDurum.DevamEden;
                    
                    _logger.LogInformation(
                        "Olay {OlayNo} durumu otomatik olarak Devam Eden olarak güncellendi.",
                        olay.OlayNo ?? olay.Id.ToString());
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Olay {OlayNo} için işlem yapılırken hata oluştu.",
                        olay.OlayNo ?? olay.Id.ToString());
                }
            }

            await context.SaveChangesAsync(ct);
        }
    }
}

