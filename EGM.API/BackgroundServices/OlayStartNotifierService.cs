using EGM.Domain.Enums;
using EGM.Domain.Interfaces;
using EGM.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EGM.API.BackgroundServices
{
    /// <summary>
    /// Her dakika çalışır; başlangıç saati gelmiş ancak bildirim gönderilmemiş
    /// olayları tespit ederek ilgili personele "Olay Başladı" bildirimi gönderir.
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

            var now = DateTime.Now;

            // Başlangıç saati belirtilmiş, henüz bildirilmemiş ve iptal edilmemiş olaylar
            var bekleyenOlaylar = await context.Olaylar
                .Where(o =>
                    !o.IsDeleted &&
                    !o.BaslangicBildirimiGonderildi &&
                    o.BaslangicSaati != null &&
                    o.Durum != OlayDurum.Iptal)
                .ToListAsync(ct);

            // Bellek içinde başlangıç zamanı hesaplaması (SQLite TimeSpan uyumsuzlukları için)
            var gecmisOlaylar = bekleyenOlaylar
                .Where(o => o.Tarih.Date.Add(o.BaslangicSaati!.Value) <= now)
                .ToList();

            if (gecmisOlaylar.Count == 0) return;

            _logger.LogInformation(
                "{Count} olay için başlangıç bildirimi gönderilecek.",
                gecmisOlaylar.Count);

            foreach (var olay in gecmisOlaylar)
            {
                try
                {
                    await notifier.NotifyOlayBasladiAsync(olay);
                    olay.BaslangicBildirimiGonderildi = true;
                    // Durumu otomatik olarak "Gerceklesti" yap
                    if (olay.Durum == EGM.Domain.Enums.OlayDurum.Planlandi)
                    {
                        olay.Durum = EGM.Domain.Enums.OlayDurum.Gerceklesti;
                        _logger.LogInformation(
                            "Olay {TakipNo} durumu otomatik olarak Gerceklesti olarak güncellendi.",
                            olay.TakipNo ?? olay.Id.ToString());
                    }
                    _logger.LogInformation(
                        "Olay {TakipNo} için başlangıç bildirimi gönderildi.",
                        olay.TakipNo ?? olay.Id.ToString());
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Olay {TakipNo} için bildirim gönderilirken hata oluştu.",
                        olay.TakipNo ?? olay.Id.ToString());
                    // Hata durumunda bu olay için flag'i güncelleme, bir sonraki döngüde yeniden dene
                }
            }

            await context.SaveChangesAsync(ct);
        }
    }
}
