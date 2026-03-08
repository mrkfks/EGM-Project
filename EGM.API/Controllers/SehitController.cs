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
    public class SehitController : ControllerBase
    {
        private readonly SehitService _service;
        public SehitController(SehitService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok((await _service.GetAllAsync()).Select(MapToResponse));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            return item == null ? NotFound() : Ok(MapToResponse(item));
        }

        [HttpGet("faaliyet/{faaliyetId}")]
        public async Task<IActionResult> GetByFaaliyet(int faaliyetId)
            => Ok((await _service.GetByOperasyonelFaaliyetAsync(faaliyetId)).Select(MapToResponse));

        [HttpGet("gorev/{gorev}")]
        public async Task<IActionResult> GetByGorev(string gorev)
            => Ok((await _service.GetByGorevAsync(gorev)).Select(MapToResponse));

        [HttpPost]
        [Authorize(Roles = $"{Roles.IlAdmin},{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> Create([FromBody] SehitCreateDto dto)
        {
            var entity = new Sehit
            {
                OperasyonelFaaliyetId = dto.OperasyonelFaaliyetId,
                Ad = dto.Ad, Soyad = dto.Soyad,
                TcKimlikNo = dto.TcKimlikNo,
                DogumTarihi = dto.DogumTarihi,
                Gorev = dto.Gorev
            };
            var created = await _service.CreateAsync(entity);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToResponse(created));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{Roles.IlAdmin},{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> Update(int id, [FromBody] SehitCreateDto dto)
        {
            var updated = new Sehit
            {
                OperasyonelFaaliyetId = dto.OperasyonelFaaliyetId,
                Ad = dto.Ad, Soyad = dto.Soyad,
                TcKimlikNo = dto.TcKimlikNo,
                DogumTarihi = dto.DogumTarihi,
                Gorev = dto.Gorev
            };
            return await _service.UpdateAsync(id, updated) ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> Delete(int id)
            => await _service.DeleteAsync(id) ? NoContent() : NotFound();

        private static SehitResponseDto MapToResponse(Sehit s) => new()
        {
            Id = s.Id, OperasyonelFaaliyetId = s.OperasyonelFaaliyetId,
            Ad = s.Ad, Soyad = s.Soyad,
            DogumTarihi = s.DogumTarihi, Gorev = s.Gorev
        };
    }
}
