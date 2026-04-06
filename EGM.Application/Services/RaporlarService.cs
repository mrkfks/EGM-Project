using EGM.Application.DTOs;
using EGM.Domain.Enums;
using EGM.Domain.Interfaces;

namespace EGM.Application.Services
{
    public class RaporlarService
    {
        private readonly IOlayRepository _olayRepository;

        public RaporlarService(IOlayRepository olayRepository)
        {
            _olayRepository = olayRepository;
        }

        public async Task<GunlukBultenDto> GetGunlukBultenAsync(DateTime tarih)
        {
            var gunBaslangic = tarih.Date;
            var gunBitis     = gunBaslangic.AddDays(1).AddTicks(-1);
            var sonrakiGun   = gunBaslangic.AddDays(1);
            var sonrakiGunBitis = sonrakiGun.AddDays(1).AddTicks(-1);

            // GerÃ§ekleÅŸen olaylar
            var (gerceklesen, _) = await _olayRepository.GetFilteredPagedAsync(
                OlayDurum.Gerceklesti, gunBaslangic, gunBitis, null, 1, 500);

            // Beklenen olaylar (ertesi gÃ¼n planlanmÄ±ÅŸ)
            var (beklenen, _) = await _olayRepository.GetFilteredPagedAsync(
                OlayDurum.Planlandi, sonrakiGun, sonrakiGunBitis, null, 1, 500);

            // Ä°cmal: olay tÃ¼rÃ¼ne gÃ¶re grupla
            var icmal = gerceklesen
                .GroupBy(o => o.OlayTuru ?? "DÄ°ÄER")
                .OrderBy(g => g.Key)
                .Select((g, i) => new IcmalVeriDto
                {
                    Tur           = g.Key.ToUpperInvariant(),
                    EylemSayisi   = g.Count(),
                    KatilimSayisi = g.Sum(o => o.KatilimciSayisi ?? 0),
                    GozaltiSayisi = g.Sum(o => o.GozaltiSayisi ?? 0),
                    OluSayisi     = g.Sum(o => o.SehitOluSayisi ?? 0)
                })
                .ToList();

            // GerÃ§ekleÅŸen detaylar
            var gerceklesenDetay = gerceklesen
                .OrderBy(o => o.BaslangicSaati)
                .Select((o, i) => new GerceklesenDetayDto
                {
                    Sn            = i + 1,
                    Il            = o.Il ?? string.Empty,
                    EylemEtkinlik = o.OlayTuru ?? string.Empty,
                    Saat          = o.BaslangicSaati.HasValue
                                        ? o.BaslangicSaati.Value.ToString(@"HH\:mm")
                                        : string.Empty,
                    OrganizeEden  = o.Organizator?.Ad ?? string.Empty,
                    Aciklama      = o.Aciklama ?? string.Empty,
                    KatilimSayisi = o.KatilimciSayisi ?? 0
                })
                .ToList();

            // Beklenen detaylar
            var beklenenDetay = beklenen
                .OrderBy(o => o.BaslangicSaati)
                .Select((o, i) => new BeklenenDetayDto
                {
                    Sn            = i + 1,
                    Il            = o.Il ?? string.Empty,
                    Yer           = o.Mekan ?? string.Empty,
                    EylemEtkinlik = o.OlayTuru ?? string.Empty,
                    Saat          = o.BaslangicSaati.HasValue
                                        ? o.BaslangicSaati.Value.ToString(@"HH\:mm")
                                        : string.Empty,
                    OrganizeEden  = o.Organizator?.Ad ?? string.Empty,
                    Aciklama      = o.Aciklama ?? string.Empty
                })
                .ToList();

            return new GunlukBultenDto
            {
                Tarih           = tarih.ToString("dd MMMM yyyy dddd", new System.Globalization.CultureInfo("tr-TR")),
                SonrakiGunTarih = sonrakiGun.ToString("dd MMMM yyyy dddd", new System.Globalization.CultureInfo("tr-TR")),
                IcmalVerileri       = icmal,
                GerceklesenDetaylar = gerceklesenDetay,
                BeklenenDetaylar    = beklenenDetay,
            };
        }
    }
}
