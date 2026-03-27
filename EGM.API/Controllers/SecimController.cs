using EGM.Application.DTOs;
using EGM.Application.Services;
using EGM.Domain.Constants;
using EGM.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EGM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SecimController : ControllerBase
    {
        private readonly SecimService _service;
        public SecimController(SecimService service) => _service = service;

        // ── Seçim Sonucu ─────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok((await _service.GetAllAsync()).Select(MapToResponse));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _service.GetByIdAsync(id);
            return item == null ? NotFound() : Ok(MapToResponse(item));
        }

        [HttpGet("tur/{tur}")]
        public async Task<IActionResult> GetByTur(string tur)
            => Ok((await _service.GetBySecimTuruAsync(tur)).Select(MapToResponse));

        [HttpGet("bolge")]
        public async Task<IActionResult> GetByBolge([FromQuery] string bolgeTipi, [FromQuery] int bolgeId)
            => Ok((await _service.GetByBolgeAsync(bolgeTipi, bolgeId)).Select(MapToResponse));

        [HttpGet("onayli")]
        public async Task<IActionResult> GetOnaylilar()
            => Ok((await _service.GetOnayliSonuclarAsync()).Select(MapToResponse));

        [HttpGet("parti/{partiId}/toplam-oy")]
        public async Task<IActionResult> GetToplamOy(Guid partiId)
            => Ok(new { PartiId = partiId, ToplamOy = await _service.GetToplamOyByPartiAsync(partiId) });

        [HttpPost]
        [Authorize(Roles = $"{Roles.BaskanlikPersonel},{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> Create([FromBody] SecimSonucuCreateDto dto)
        {
            var entity = new SecimSonucu
            {
                SecimTuru = dto.SecimTuru, Tarih = dto.Tarih, BolgeTipi = dto.BolgeTipi,
                BolgeId = dto.BolgeId, AdayId = dto.AdayId, PartiId = dto.PartiId,
                OySayisi = dto.OySayisi, OyOrani = dto.OyOrani,
                KaynakId = dto.KaynakId, KaynakOnayDurumu = dto.KaynakOnayDurumu
            };
            var created = await _service.CreateAsync(entity);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToResponse(created));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SecimSonucuCreateDto dto)
        {
            var updated = new SecimSonucu
            {
                SecimTuru = dto.SecimTuru, Tarih = dto.Tarih, BolgeTipi = dto.BolgeTipi,
                BolgeId = dto.BolgeId, AdayId = dto.AdayId, PartiId = dto.PartiId,
                OySayisi = dto.OySayisi, OyOrani = dto.OyOrani,
                KaynakId = dto.KaynakId, KaynakOnayDurumu = dto.KaynakOnayDurumu
            };
            return await _service.UpdateAsync(id, updated) ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{Roles.Yonetici}")]
        public async Task<IActionResult> Delete(Guid id)
            => await _service.DeleteAsync(id) ? NoContent() : NotFound();

        // ── Aday ─────────────────────────────────────────────────────────
        [HttpGet("aday")]
        public async Task<IActionResult> GetAllAday()
            => Ok(await _service.GetAllAdayAsync());

        [HttpPost("aday")]
        [Authorize(Roles = $"{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> CreateAday([FromBody] AdayCreateDto dto)
            => Ok(await _service.CreateAdayAsync(dto.AdSoyad!, dto.PartiAdi!));

        [HttpDelete("aday/{id}")]
        [Authorize(Roles = $"{Roles.Yonetici}")]
        public async Task<IActionResult> DeleteAday(Guid id)
            => await _service.DeleteAdayAsync(id) ? NoContent() : NotFound();

        // ── Parti ─────────────────────────────────────────────────────────
        [HttpGet("parti")]
        public async Task<IActionResult> GetAllParti()
            => Ok(await _service.GetAllPartiAsync());

        [HttpPost("parti")]
        [Authorize(Roles = $"{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> CreateParti([FromBody] PartiCreateDto dto)
            => Ok(await _service.CreatePartiAsync(dto.Ad!));

        [HttpDelete("parti/{id}")]
        [Authorize(Roles = $"{Roles.Yonetici}")]
        public async Task<IActionResult> DeleteParti(Guid id)
            => await _service.DeletePartiAsync(id) ? NoContent() : NotFound();

        // ── Kaynak ───────────────────────────────────────────────────────
        [HttpGet("kaynak")]
        public async Task<IActionResult> GetAllKaynak()
            => Ok(await _service.GetAllKaynakAsync());

        [HttpPost("kaynak")]
        [Authorize(Roles = $"{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> CreateKaynak([FromBody] string kaynakAdi)
            => Ok(await _service.CreateKaynakAsync(kaynakAdi));

        [HttpDelete("kaynak/{id}")]
        [Authorize(Roles = $"{Roles.Yonetici}")]
        public async Task<IActionResult> DeleteKaynak(Guid id)
            => await _service.DeleteKaynakAsync(id) ? NoContent() : NotFound();

        // ── Sandik Olay ──────────────────────────────────────────────────
        [HttpGet("sandik-olay")]
        public async Task<IActionResult> GetAllSandikOlay()
            => Ok((await _service.GetAllSandikOlayAsync()).Select(MapSandikToResponse));

        [HttpGet("sandik-olay/{id}")]
        public async Task<IActionResult> GetSandikOlayById(Guid id)
        {
            var item = await _service.GetSandikOlayByIdAsync(id);
            return item == null ? NotFound() : Ok(MapSandikToResponse(item));
        }

        [HttpPost("sandik-olay")]
        [Authorize(Policy = "CityStaffOrAbove")]
        public async Task<IActionResult> CreateSandikOlay([FromBody] SandikOlayCreateDto dto)
        {
            var entity = new SandikOlay
            {
                MusahitAdi = dto.MusahitAdi,
                Il = dto.Il,
                Ilce = dto.Ilce,
                Mahalle = dto.Mahalle,
                SandikNo = dto.SandikNo,
                OlayKategorisi = dto.OlayKategorisi,
                OlaySaati = dto.OlaySaati,
                Aciklama = dto.Aciklama,
                KanitDosyasi = dto.KanitDosyasi,
                Tarih = dto.Tarih
            };
            var created = await _service.CreateSandikOlayAsync(entity);
            return CreatedAtAction(nameof(GetSandikOlayById), new { id = created.Id }, MapSandikToResponse(created));
        }

        [HttpDelete("sandik-olay/{id}")]
        [Authorize(Policy = "CityManagerOrAbove")]
        public async Task<IActionResult> DeleteSandikOlay(Guid id)
            => await _service.DeleteSandikOlayAsync(id) ? NoContent() : NotFound();

        private static SecimSonucuResponseDto MapToResponse(SecimSonucu s) => new()
        {
            Id = s.Id, SecimTuru = s.SecimTuru, Tarih = s.Tarih, BolgeTipi = s.BolgeTipi,
            BolgeId = s.BolgeId, AdayId = s.AdayId, PartiId = s.PartiId,
            OySayisi = s.OySayisi, OyOrani = s.OyOrani, KaynakOnayDurumu = s.KaynakOnayDurumu
        };

        private static SandikOlayResponseDto MapSandikToResponse(SandikOlay s) => new()
        {
            Id = s.Id, MusahitAdi = s.MusahitAdi, Il = s.Il, Ilce = s.Ilce,
            Mahalle = s.Mahalle, SandikNo = s.SandikNo, OlayKategorisi = s.OlayKategorisi,
            OlaySaati = s.OlaySaati, Aciklama = s.Aciklama, KanitDosyasi = s.KanitDosyasi,
            Tarih = s.Tarih, CreatedAt = s.CreatedAt
        };
    }
}
