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

        // â”€â”€ Sandik Olay â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
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
                Okul = dto.Okul,
                Konu = dto.Konu,
                SandikNo = dto.SandikNo,
                OlayKategorisi = dto.OlayKategorisi,
                OlaySaati = dto.OlaySaati,
                Aciklama = dto.Aciklama,
                KanitDosyasi = dto.KanitDosyasi,
                Tarih = dto.Tarih,
                KatilimciSayisi = dto.KatilimciSayisi,
                SehitSayisi = dto.SehitSayisi,
                OluSayisi = dto.OluSayisi,
                GozaltiSayisi = dto.GozaltiSayisi
            };
            var created = await _service.CreateSandikOlayAsync(entity);
            return CreatedAtAction(nameof(GetSandikOlayById), new { id = created.Id }, MapSandikToResponse(created));
        }

        [HttpPut("sandik-olay/{id}")]
        [Authorize(Policy = "CityStaffOrAbove")]
        public async Task<IActionResult> UpdateSandikOlay(Guid id, [FromBody] SandikOlayCreateDto dto)
        {
            var updated = new SandikOlay
            {
                MusahitAdi = dto.MusahitAdi, Il = dto.Il, Ilce = dto.Ilce,
                Mahalle = dto.Mahalle, Okul = dto.Okul, Konu = dto.Konu,
                SandikNo = dto.SandikNo, OlayKategorisi = dto.OlayKategorisi,
                OlaySaati = dto.OlaySaati, Aciklama = dto.Aciklama,
                Tarih = dto.Tarih, KatilimciSayisi = dto.KatilimciSayisi,
                SehitSayisi = dto.SehitSayisi, OluSayisi = dto.OluSayisi,
                GozaltiSayisi = dto.GozaltiSayisi
            };
            return await _service.UpdateSandikOlayAsync(id, updated) ? NoContent() : NotFound();
        }

        [HttpDelete("sandik-olay/{id}")]
        [Authorize(Policy = "CityManagerOrAbove")]
        public async Task<IActionResult> DeleteSandikOlay(Guid id)
            => await _service.DeleteSandikOlayAsync(id) ? NoContent() : NotFound();

        private static SandikOlayResponseDto MapSandikToResponse(SandikOlay s) => new()
        {
            Id = s.Id, MusahitAdi = s.MusahitAdi, Il = s.Il, Ilce = s.Ilce,
            Mahalle = s.Mahalle, Okul = s.Okul, Konu = s.Konu, SandikNo = s.SandikNo, OlayKategorisi = s.OlayKategorisi,
            OlaySaati = s.OlaySaati, Aciklama = s.Aciklama, KanitDosyasi = s.KanitDosyasi,
            Tarih = s.Tarih, CreatedAt = s.CreatedAt, CreatedByUserId = s.CreatedByUserId,
            KatilimciSayisi = s.KatilimciSayisi, SehitSayisi = s.SehitSayisi,
            OluSayisi = s.OluSayisi, GozaltiSayisi = s.GozaltiSayisi
        };
    }
}
