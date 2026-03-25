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
    public class OluController : ControllerBase
    {
        private readonly OluService _service;
        public OluController(OluService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok((await _service.GetAllAsync()).Select(MapToResponse));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _service.GetByIdAsync(id);
            return item == null ? NotFound() : Ok(MapToResponse(item));
        }

        [HttpGet("faaliyet/{faaliyetId}")]
        public async Task<IActionResult> GetByFaaliyet(Guid faaliyetId)
            => Ok((await _service.GetByOperasyonelFaaliyetAsync(faaliyetId)).Select(MapToResponse));

        [HttpGet("durum/{durum}")]
        public async Task<IActionResult> GetByDurum(string durum)
            => Ok((await _service.GetByKatilimciDurumuAsync(durum)).Select(MapToResponse));

        [HttpPost]
        [Authorize(Roles = $"{Roles.IlAdmin},{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> Create([FromBody] OluCreateDto dto)
        {
            var entity = new Olu
            {
                OperasyonelFaaliyetId = dto.OperasyonelFaaliyetId,
                Ad = dto.Ad, Soyad = dto.Soyad,
                TcKimlikNo = dto.TcKimlikNo,
                DogumTarihi = dto.DogumTarihi,
                KatilimciDurumu = dto.KatilimciDurumu
            };
            var created = await _service.CreateAsync(entity);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToResponse(created));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{Roles.IlAdmin},{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] OluCreateDto dto)
        {
            var updated = new Olu
            {
                OperasyonelFaaliyetId = dto.OperasyonelFaaliyetId,
                Ad = dto.Ad, Soyad = dto.Soyad,
                TcKimlikNo = dto.TcKimlikNo,
                DogumTarihi = dto.DogumTarihi,
                KatilimciDurumu = dto.KatilimciDurumu
            };
            return await _service.UpdateAsync(id, updated) ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> Delete(Guid id)
            => await _service.DeleteAsync(id) ? NoContent() : NotFound();

        private static OluResponseDto MapToResponse(Olu o) => new()
        {
            Id = o.Id, OperasyonelFaaliyetId = o.OperasyonelFaaliyetId,
            Ad = o.Ad, Soyad = o.Soyad,
            DogumTarihi = o.DogumTarihi, KatilimciDurumu = o.KatilimciDurumu
        };
    }
}
