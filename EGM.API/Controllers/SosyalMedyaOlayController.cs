using EGM.Application.DTOs;
using EGM.Application.Services;
using EGM.Domain.Constants;
using EGM.Domain.Entities;
using EGM.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EGM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SosyalMedyaOlayController : ControllerBase
    {
        private readonly SosyalMedyaOlayService _service;
        public SosyalMedyaOlayController(SosyalMedyaOlayService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok((await _service.GetAllAsync()).Select(MapToResponse));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            return item == null ? NotFound() : Ok(MapToResponse(item));
        }

        [HttpGet("olay/{olayId}")]
        public async Task<IActionResult> GetByOlay(int olayId)
            => Ok((await _service.GetByOlayAsync(olayId)).Select(MapToResponse));

        [HttpGet("platform/{platform}")]
        public async Task<IActionResult> GetByPlatform(string platform)
            => Ok((await _service.GetByPlatformAsync(platform)).Select(MapToResponse));

        [HttpGet("yuksek-signal")]
        public async Task<IActionResult> GetHighSignal([FromQuery] double minSkor = 50)
            => Ok((await _service.GetHighSignalAsync(minSkor)).Select(MapToResponse));

        [HttpGet("hassasiyet/{hassasiyet}")]
        public async Task<IActionResult> GetByHassasiyet(Hassasiyet hassasiyet)
            => Ok((await _service.GetByHassasiyetAsync(hassasiyet)).Select(MapToResponse));

        [HttpPost]
        [Authorize(Roles = $"{Roles.IlPersonel},{Roles.IlAdmin},{Roles.BaskanlikPersonel},{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> Create([FromBody] SosyalMedyaOlayCreateDto dto)
        {
            var entity = new SosyalMedyaOlay
            {
                OlayId = dto.OlayId,
                Platform = dto.Platform,
                PaylasimLinki = dto.PaylasimLinki,
                PaylasimTarihi = dto.PaylasimTarihi,
                IcerikOzeti = dto.IcerikOzeti,
                IlgiliKisiKurum = dto.IlgiliKisiKurum,
                Hassasiyet = dto.Hassasiyet,
                SosyalSignalSkoru = dto.SosyalSignalSkoru
            };
            var created = await _service.CreateAsync(entity);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToResponse(created));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{Roles.IlAdmin},{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> Update(int id, [FromBody] SosyalMedyaOlayCreateDto dto)
        {
            var updated = new SosyalMedyaOlay
            {
                OlayId = dto.OlayId, Platform = dto.Platform,
                PaylasimLinki = dto.PaylasimLinki, PaylasimTarihi = dto.PaylasimTarihi,
                IcerikOzeti = dto.IcerikOzeti, IlgiliKisiKurum = dto.IlgiliKisiKurum,
                Hassasiyet = dto.Hassasiyet, SosyalSignalSkoru = dto.SosyalSignalSkoru
            };
            return await _service.UpdateAsync(id, updated) ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> Delete(int id)
            => await _service.DeleteAsync(id) ? NoContent() : NotFound();

        private static SosyalMedyaOlayResponseDto MapToResponse(SosyalMedyaOlay s) => new()
        {
            Id = s.Id, OlayId = s.OlayId, Platform = s.Platform,
            PaylasimLinki = s.PaylasimLinki, PaylasimTarihi = s.PaylasimTarihi,
            IcerikOzeti = s.IcerikOzeti, IlgiliKisiKurum = s.IlgiliKisiKurum,
            Hassasiyet = s.Hassasiyet, SosyalSignalSkoru = s.SosyalSignalSkoru
        };
    }
}
