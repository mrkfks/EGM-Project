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

        // ── VIP Ziyaret ──────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok((await _service.GetAllAsync()).Select(MapToResponse));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
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
                GozlemNoktalari = dto.GozlemNoktalari
            };
            var created = await _service.CreateAsync(entity);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToResponse(created));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{Roles.IlAdmin},{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> Update(int id, [FromBody] VIPZiyaretCreateDto dto)
        {
            var updated = new VIPZiyaret
            {
                ZiyaretEdenAdSoyad = dto.ZiyaretEdenAdSoyad, Unvan = dto.Unvan,
                BaslangicTarihi = dto.BaslangicTarihi, BitisTarihi = dto.BitisTarihi,
                Il = dto.Il, Mekan = dto.Mekan,
                Hassasiyet = dto.Hassasiyet, GuvenlikSeviyesi = dto.GuvenlikSeviyesi,
                GozlemNoktalari = dto.GozlemNoktalari
            };
            return await _service.UpdateAsync(id, updated) ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> Delete(int id)
            => await _service.DeleteAsync(id) ? NoContent() : NotFound();

        // ── Güvenlik Planı ───────────────────────────────────────────────
        [HttpGet("guvenlik-plani")]
        public async Task<IActionResult> GetAllGuvenlikPlani()
            => Ok(await _service.GetAllGuvenlikPlaniAsync());

        [HttpPost("guvenlik-plani")]
        [Authorize(Roles = $"{Roles.IlAdmin},{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> CreateGuvenlikPlani([FromBody] GuvenlikPlaniCreateDto dto)
            => Ok(await _service.CreateGuvenlikPlaniAsync(dto.VIPZiyaretId, dto.Ad!, dto.Aciklama));

        [HttpPut("guvenlik-plani/{id}")]
        [Authorize(Roles = $"{Roles.IlAdmin},{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> UpdateGuvenlikPlani(int id, [FromBody] GuvenlikPlaniCreateDto dto)
            => await _service.UpdateGuvenlikPlaniAsync(id, dto.Ad!, dto.Aciklama) ? NoContent() : NotFound();

        [HttpDelete("guvenlik-plani/{id}")]
        [Authorize(Roles = $"{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> DeleteGuvenlikPlani(int id)
            => await _service.DeleteGuvenlikPlaniAsync(id) ? NoContent() : NotFound();

        // ── Ekip ─────────────────────────────────────────────────────────
        [HttpGet("ekip")]
        public async Task<IActionResult> GetAllEkip()
            => Ok(await _service.GetAllEkipAsync());

        [HttpPost("ekip")]
        [Authorize(Roles = $"{Roles.IlAdmin},{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> CreateEkip([FromBody] EkipCreateDto dto)
            => Ok(await _service.CreateEkipAsync(dto.VIPZiyaretId, dto.Ad!));

        [HttpPut("ekip/{id}")]
        [Authorize(Roles = $"{Roles.IlAdmin},{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> UpdateEkip(int id, [FromBody] EkipCreateDto dto)
            => await _service.UpdateEkipAsync(id, dto.Ad!) ? NoContent() : NotFound();

        [HttpDelete("ekip/{id}")]
        [Authorize(Roles = $"{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> DeleteEkip(int id)
            => await _service.DeleteEkipAsync(id) ? NoContent() : NotFound();

        private static VIPZiyaretResponseDto MapToResponse(VIPZiyaret v) => new()
        {
            Id = v.Id, ZiyaretEdenAdSoyad = v.ZiyaretEdenAdSoyad, Unvan = v.Unvan,
            BaslangicTarihi = v.BaslangicTarihi, BitisTarihi = v.BitisTarihi,
            Il = v.Il, Mekan = v.Mekan,
            Hassasiyet = v.Hassasiyet, GuvenlikSeviyesi = v.GuvenlikSeviyesi,
            GozlemNoktalari = v.GozlemNoktalari
        };
    }
}
