using EGM.Application.DTOs;
using EGM.Domain.Enums;
using EGM.Domain.Interfaces;
using System.Linq;

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

            var res1 = await _olayRepository.GetFilteredPagedAsync(
                OlayDurum.Gerceklesen, gunBaslangic, gunBitis, null, 1, 500);
            var gerceklesen = res1.Items;

            var res2 = await _olayRepository.GetFilteredPagedAsync(
                OlayDurum.Planlanan, sonrakiGun, sonrakiGunBitis, null, 1, 500);
            var beklenen = res2.Items;

            var icmal = gerceklesen
                .GroupBy(o => o.Tur?.Name ?? "DİĞER")
                .OrderBy(g => g.Key)
                .Select((g, i) => new IcmalVeriDto
                {
                    Tur           = g.Key.ToUpperInvariant(),
                    EylemSayisi   = g.Count(),
                    KatilimSayisi = g.Sum(o => o.EventDetail?.KatilimciSayisi ?? 0),
                    GozaltiSayisi = g.Sum(o => o.EventDetail?.GozaltiSayisi ?? 0),
                    OluSayisi     = g.Sum(o => (o.EventDetail?.SehitSayisi ?? 0) + (o.EventDetail?.OluSayisi ?? 0))
                })
                .ToList();

            var gerceklesenDetay = gerceklesen
                .OrderBy(o => o.BaslangicTarihi)
                .Select((o, i) => new GerceklesenDetayDto
                {
                    Sn            = i + 1,
                    Il            = o.Locations?.FirstOrDefault()?.Il ?? string.Empty,
                    EylemEtkinlik = o.Tur?.Name ?? string.Empty,
                    Saat          = o.BaslangicTarihi.ToString(@"HH\:mm"),
                    OrganizeEden  = o.Organizator?.Ad ?? string.Empty,
                    Aciklama      = o.Aciklama ?? string.Empty,
                    KatilimSayisi = o.EventDetail?.KatilimciSayisi ?? 0
                })
                .ToList();

            var beklenenDetay = beklenen
                .OrderBy(o => o.BaslangicTarihi)
                .Select((o, i) => new BeklenenDetayDto
                {
                    Sn            = i + 1,
                    Il            = o.Locations?.FirstOrDefault()?.Il ?? string.Empty,
                    Yer           = o.Locations?.FirstOrDefault()?.Mekan ?? string.Empty,
                    EylemEtkinlik = o.Tur?.Name ?? string.Empty,
                    Saat          = o.BaslangicTarihi.ToString(@"HH\:mm"),
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
