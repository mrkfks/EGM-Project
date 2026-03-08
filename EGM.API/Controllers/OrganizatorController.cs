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
    public class OrganizatorController : ControllerBase
    {
        private readonly OrganizatorService _service;
        public OrganizatorController(OrganizatorService service) => _service = service;

        // ── Organizatör ──────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok((await _service.GetAllAsync()).Select(MapToResponse));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            return item == null ? NotFound() : Ok(MapToResponse(item));
        }

        [HttpGet("alan/{alan}")]
        public async Task<IActionResult> GetByFaaliyetAlani(string alan)
            => Ok((await _service.GetByFaaliyetAlaniAsync(alan)).Select(MapToResponse));

        [HttpPost]
        [Authorize(Roles = $"{Roles.IlAdmin},{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> Create([FromBody] OrganizatorCreateDto dto)
        {
            var entity = new Organizator
            {
                Ad = dto.Ad, KurulusTarihi = dto.KurulusTarihi,
                FaaliyetAlani = dto.FaaliyetAlani, Iletisim = dto.Iletisim
            };
            var created = await _service.CreateAsync(entity);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToResponse(created));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{Roles.IlAdmin},{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> Update(int id, [FromBody] OrganizatorCreateDto dto)
        {
            var updated = new Organizator
            {
                Ad = dto.Ad, KurulusTarihi = dto.KurulusTarihi,
                FaaliyetAlani = dto.FaaliyetAlani, Iletisim = dto.Iletisim
            };
            return await _service.UpdateAsync(id, updated) ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> Delete(int id)
            => await _service.DeleteAsync(id) ? NoContent() : NotFound();

        // ── Kategori ─────────────────────────────────────────────────────
        [HttpGet("kategori")]
        public async Task<IActionResult> GetAllKategori()
            => Ok(await _service.GetAllKategoriAsync());

        [HttpPost("kategori")]
        [Authorize(Roles = $"{Roles.IlAdmin},{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> CreateKategori([FromBody] string ad)
            => Ok(await _service.CreateKategoriAsync(ad));

        [HttpDelete("kategori/{id}")]
        [Authorize(Roles = $"{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> DeleteKategori(int id)
            => await _service.DeleteKategoriAsync(id) ? NoContent() : NotFound();

        // ── Konu ─────────────────────────────────────────────────────────
        [HttpGet("konu")]
        public async Task<IActionResult> GetAllKonu()
            => Ok(await _service.GetAllKonuAsync());

        [HttpGet("konu/{id}")]
        public async Task<IActionResult> GetKonuById(int id)
        {
            var item = await _service.GetKonuByIdAsync(id);
            return item == null ? NotFound() : Ok(item);
        }

        [HttpPost("konu")]
        [Authorize(Roles = $"{Roles.IlAdmin},{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> CreateKonu([FromBody] KonuCreateDto dto)
            => Ok(await _service.CreateKonuAsync(dto.Ad!, dto.Aciklama));

        [HttpPut("konu/{id}")]
        [Authorize(Roles = $"{Roles.IlAdmin},{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> UpdateKonu(int id, [FromBody] KonuCreateDto dto)
            => await _service.UpdateKonuAsync(id, dto.Ad!, dto.Aciklama) ? NoContent() : NotFound();

        [HttpDelete("konu/{id}")]
        [Authorize(Roles = $"{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> DeleteKonu(int id)
            => await _service.DeleteKonuAsync(id) ? NoContent() : NotFound();

        private static OrganizatorResponseDto MapToResponse(Organizator o) => new()
        {
            Id = o.Id, Ad = o.Ad, KurulusTarihi = o.KurulusTarihi,
            FaaliyetAlani = o.FaaliyetAlani, Iletisim = o.Iletisim
        };
    }
}
