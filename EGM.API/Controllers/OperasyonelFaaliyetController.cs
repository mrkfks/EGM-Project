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
    public class OperasyonelFaaliyetController : ControllerBase
    {
        private readonly OperasyonelFaaliyetService _service;

        public OperasyonelFaaliyetController(OperasyonelFaaliyetService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _service.GetAllAsync();
            return Ok(list.Select(MapToResponse));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(MapToResponse(item));
        }

        [HttpGet("olay/{olayId}")]
        public async Task<IActionResult> GetByOlay(int olayId)
        {
            var list = await _service.GetByOlayAsync(olayId);
            return Ok(list.Select(MapToResponse));
        }

        [HttpGet("{id}/gruplar")]
        public async Task<IActionResult> GetGruplar(int id)
        {
            var gruplar = await _service.GetGruplarAsync(id);
            return Ok(gruplar);
        }

        [HttpGet("{id}/toplam-katilimci")]
        public async Task<IActionResult> GetToplamKatilimci(int id)
        {
            var sayi = await _service.GetToplamKatilimciSayisiAsync(id);
            return Ok(new { ToplamKatilimci = sayi });
        }

        [HttpPost]
        [Authorize(Roles = $"{Roles.IlAdmin},{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> Create([FromBody] OperasyonelFaaliyetCreateDto dto)
        {
            var entity = new OperasyonelFaaliyet
            {
                OlayId = dto.OlayId,
                Aciklama = dto.Aciklama
            };
            var created = await _service.CreateAsync(entity);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToResponse(created));
        }

        [HttpPost("{id}/grup")]
        [Authorize(Roles = $"{Roles.IlAdmin},{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> AddGrup(int id, [FromBody] KatilimciGrupCreateDto dto)
        {
            var grup = await _service.AddKatilimciGrupAsync(id, dto.GrupAdi!, dto.KatilimciSayisi);
            return Ok(grup);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{Roles.IlAdmin},{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> Update(int id, [FromBody] OperasyonelFaaliyetCreateDto dto)
        {
            var updated = new OperasyonelFaaliyet { OlayId = dto.OlayId, Aciklama = dto.Aciklama };
            var ok = await _service.UpdateAsync(id, updated);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }

        private static OperasyonelFaaliyetResponseDto MapToResponse(OperasyonelFaaliyet f) => new()
        {
            Id = f.Id,
            OlayId = f.OlayId,
            Aciklama = f.Aciklama,
            ToplamGrupSayisi = f.ToplamGrupSayisi,
            SupheliSayisi = f.SupheliSayisi,
            GozaltiSayisi = f.GozaltiSayisi,
            SehitSayisi = f.SehitSayisi,
            OluSayisi = f.OluSayisi
        };
    }
}
