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
    public class VIPZiyaretController : ControllerBase
    {
        private readonly VIPZiyaretService _service;
        public VIPZiyaretController(VIPZiyaretService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok((await _service.GetAllAsync()).Select(MapToResponse));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _service.GetByIdAsync(id);
            return item == null ? NotFound() : Ok(MapToResponse(item));
        }

        [HttpGet("aktif")]
        public async Task<IActionResult> GetAktif()
            => Ok((await _service.GetAktifZiyaretlerAsync()).Select(MapToResponse));

        [HttpGet("il/{il}")]
        public async Task<IActionResult> GetByIl(string il)
            => Ok((await _service.GetByIlAsync(il)).Select(MapToResponse));

        [HttpGet("hassasiyet/{hassasiyet}")]
        public async Task<IActionResult> GetByHassasiyet(Hassasiyet hassasiyet)
            => Ok((await _service.GetByHassasiyetAsync(hassasiyet)).Select(MapToResponse));

        [HttpPost]
        [Authorize(Roles = $"{Roles.IlAdmin},{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> Create([FromBody] VIPZiyaretCreateDto dto)
        {
            var entity = new VIPZiyaret
            {
                ZiyaretEdenAdSoyad = dto.ZiyaretEdenAdSoyad, Unvan = dto.Unvan,
                BaslangicTarihi = dto.BaslangicTarihi, BitisTarihi = dto.BitisTarihi,
                Il = dto.Il, Mekan = dto.Mekan,
                Hassasiyet = dto.Hassasiyet, GuvenlikSeviyesi = dto.GuvenlikSeviyesi,
                GozlemNoktalari = dto.GozlemNoktalari,
                ZiyaretDurumu = dto.ZiyaretDurumu
            };
            var created = await _service.CreateAsync(entity);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToResponse(created));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{Roles.IlAdmin},{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] VIPZiyaretCreateDto dto)
        {
            var updated = new VIPZiyaret
            {
                ZiyaretEdenAdSoyad = dto.ZiyaretEdenAdSoyad, Unvan = dto.Unvan,
                BaslangicTarihi = dto.BaslangicTarihi, BitisTarihi = dto.BitisTarihi,
                Il = dto.Il, Mekan = dto.Mekan,
                Hassasiyet = dto.Hassasiyet, GuvenlikSeviyesi = dto.GuvenlikSeviyesi,
                GozlemNoktalari = dto.GozlemNoktalari,
                ZiyaretDurumu = dto.ZiyaretDurumu
            };
            return await _service.UpdateAsync(id, updated) ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> Delete(Guid id)
            => await _service.DeleteAsync(id) ? NoContent() : NotFound();

        private static VIPZiyaretResponseDto MapToResponse(VIPZiyaret v) => new()
        {
            Id = v.Id, ZiyaretEdenAdSoyad = v.ZiyaretEdenAdSoyad, Unvan = v.Unvan,
            BaslangicTarihi = v.BaslangicTarihi, BitisTarihi = v.BitisTarihi,
            Il = v.Il, Mekan = v.Mekan,
            Hassasiyet = v.Hassasiyet, GuvenlikSeviyesi = v.GuvenlikSeviyesi,
            GozlemNoktalari = v.GozlemNoktalari, ZiyaretDurumu = v.ZiyaretDurumu,
            CreatedByUserId = v.CreatedByUserId, CreatedAt = v.CreatedAt
        };
    }
}
