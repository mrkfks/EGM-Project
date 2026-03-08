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
    public class SupheliController : ControllerBase
    {
        private readonly SupheliService _service;

        public SupheliController(SupheliService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize(Roles = $"{Roles.IlPersonel},{Roles.IlAdmin},{Roles.BaskanlikPersonel},{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
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

        [HttpGet("faaliyet/{faaliyetId}")]
        public async Task<IActionResult> GetByFaaliyet(int faaliyetId)
        {
            var list = await _service.GetByOperasyonelFaaliyetAsync(faaliyetId);
            return Ok(list.Select(MapToResponse));
        }

        [HttpGet("gozaltinda")]
        public async Task<IActionResult> GetGozaltindakiler()
        {
            var list = await _service.GetGozaltindakileriAsync();
            return Ok(list.Select(MapToResponse));
        }

        [HttpPost]
        [Authorize(Roles = $"{Roles.IlAdmin},{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> Create([FromBody] SupheliCreateDto dto)
        {
            var entity = new Supheli
            {
                OperasyonelFaaliyetId = dto.OperasyonelFaaliyetId,
                Ad = dto.Ad,
                Soyad = dto.Soyad,
                TcKimlikNo = dto.TcKimlikNo,
                DogumTarihi = dto.DogumTarihi,
                Gozaltinda = dto.Gozaltinda
            };
            var created = await _service.CreateAsync(entity);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToResponse(created));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{Roles.IlAdmin},{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> Update(int id, [FromBody] SupheliCreateDto dto)
        {
            var updated = new Supheli
            {
                OperasyonelFaaliyetId = dto.OperasyonelFaaliyetId,
                Ad = dto.Ad, Soyad = dto.Soyad,
                TcKimlikNo = dto.TcKimlikNo,
                DogumTarihi = dto.DogumTarihi,
                Gozaltinda = dto.Gozaltinda
            };
            var ok = await _service.UpdateAsync(id, updated);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpPut("{id}/gozaltiya-al")]
        [Authorize(Roles = $"{Roles.IlAdmin},{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> GozaltiyaAl(int id)
        {
            var ok = await _service.GozaltiyaAlAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpPut("{id}/serbest-birak")]
        [Authorize(Roles = $"{Roles.IlAdmin},{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> SerbetBirak(int id)
        {
            var ok = await _service.SerbetBirakAsync(id);
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

        private static SupheliResponseDto MapToResponse(Supheli s) => new()
        {
            Id = s.Id,
            OperasyonelFaaliyetId = s.OperasyonelFaaliyetId,
            Ad = s.Ad,
            Soyad = s.Soyad,
            DogumTarihi = s.DogumTarihi,
            Gozaltinda = s.Gozaltinda
        };
    }
}
