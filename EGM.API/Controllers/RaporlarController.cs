using EGM.Application.Services;
using EGM.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EGM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RaporlarController : ControllerBase
    {
        private readonly RaporlarService _raporlarService;
        private readonly EGMDbContext    _context;

        public RaporlarController(RaporlarService raporlarService, EGMDbContext context)
        {
            _raporlarService = raporlarService;
            _context         = context;
        }

        [HttpGet("gunluk-bulten")]
        public async Task<IActionResult> GetGunlukBulten([FromQuery] DateTime? tarih)
        {
            var raporTarihi = tarih.HasValue ? tarih.Value.Date : DateTime.Today;
            var bulten = await _raporlarService.GetGunlukBultenAsync(raporTarihi);
            return Ok(bulten);
        }

        [HttpGet("veri-girisler")]
        [Authorize(Policy = "CityManagerOrAbove")]
        public async Task<IActionResult> GetVeriGirisler([FromQuery] int limit = 500)
        {
            // (UserId, Tarih, Konu, Faaliyet, Kaynak)
            var rows = new List<(string? UserId, DateTime Tarih, string? Konu, string? Faaliyet, string Kaynak)>();

            // ── Sokak Olayları ────────────────────────────────────────────
            rows.AddRange((await _context.Olaylar
                .OrderByDescending(o => o.CreatedAt).Take(limit)
                .Select(o => new { o.CreatedByUserId, o.CreatedAt, o.OlayTuru })
                .ToListAsync())
                .Select(o => ((string?)o.CreatedByUserId, o.CreatedAt, o.OlayTuru, o.OlayTuru, "Sokak Olayı")));

            // ── Sosyal Medya Olayları ─────────────────────────────────────
            rows.AddRange((await _context.SosyalMedyaOlaylar
                .OrderByDescending(o => o.CreatedAt).Take(limit)
                .Select(o => new { o.CreatedByUserId, o.CreatedAt, o.Konu, o.Platform })
                .ToListAsync())
                .Select(o => ((string?)o.CreatedByUserId, o.CreatedAt, o.Konu, o.Platform, "Sosyal Medya Olayı")));

            // ── VIP Ziyaret Olayları ──────────────────────────────────────
            rows.AddRange((await _context.VIPZiyaretler
                .OrderByDescending(o => o.CreatedAt).Take(limit)
                .Select(o => new { o.CreatedByUserId, o.CreatedAt, o.ZiyaretEdenAdSoyad, o.Unvan })
                .ToListAsync())
                .Select(o => ((string?)o.CreatedByUserId, o.CreatedAt, o.ZiyaretEdenAdSoyad, o.Unvan, "VIP Ziyaret")));

            // ── Sandık Olayları ───────────────────────────────────────────
            rows.AddRange((await _context.SandikOlaylar
                .OrderByDescending(o => o.CreatedAt).Take(limit)
                .Select(o => new { o.CreatedByUserId, o.CreatedAt, o.Konu, o.OlayKategorisi })
                .ToListAsync())
                .Select(o => ((string?)o.CreatedByUserId, o.CreatedAt, o.Konu, o.OlayKategorisi, "Sandık Olayı")));

            // ── Konu Tanımları ────────────────────────────────────────────
            rows.AddRange((await _context.Konular
                .OrderByDescending(k => k.CreatedAt).Take(limit)
                .Select(k => new { k.CreatedByUserId, k.CreatedAt, k.Ad, k.Tur })
                .ToListAsync())
                .Select(k => ((string?)k.CreatedByUserId, k.CreatedAt, (string?)k.Ad, k.Tur, "Konu Tanımı")));

            // ── Gerçekleşme Şekilleri ─────────────────────────────────────
            rows.AddRange((await _context.GerceklesmeSekilleri
                .Include(g => g.OlayTuru)
                .OrderByDescending(g => g.CreatedAt).Take(limit)
                .ToListAsync())
                .Select(g => ((string?)g.CreatedByUserId, g.CreatedAt,
                    (string?)(g.OlayTuru != null ? g.OlayTuru.Name : "-"), (string?)g.Name, "Gerçekleşme Şekli")));

            // ── Kullanıcı haritası ────────────────────────────────────────
            var sicilSet = rows.Select(r => r.UserId)
                .Where(s => !string.IsNullOrEmpty(s)).ToHashSet();

            var userMap = await _context.Users
                .Where(u => sicilSet.Contains(u.Sicil.ToString()))
                .Select(u => new { Key = u.Sicil.ToString(), u.FullName, u.Birim })
                .ToDictionaryAsync(u => u.Key, u => new { u.FullName, u.Birim });

            var result = rows
                .OrderByDescending(r => r.Tarih)
                .Take(limit)
                .Select(r =>
                {
                    userMap.TryGetValue(r.UserId ?? "", out var u);
                    return new
                    {
                        sicil    = int.TryParse(r.UserId, out var s) ? s : 0,
                        adSoyad  = u?.FullName ?? "-",
                        birim    = u?.Birim    ?? "-",
                        tarih    = r.Tarih,
                        kaynak   = r.Kaynak,
                        konu     = r.Konu     ?? "-",
                        faaliyet = r.Faaliyet ?? "-",
                    };
                });

            return Ok(result);
        }
    }
}
