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
        public async Task<IActionResult> GetById(Guid id)
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
                FaaliyetAlani = dto.FaaliyetAlani, Iletisim = dto.Iletisim,
                Tur = dto.Tur, Aciklama = dto.Aciklama,
                UstKurulusId = dto.UstKurulusId
            };
            var created = await _service.CreateAsync(entity);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToResponse(created));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{Roles.IlAdmin},{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] OrganizatorCreateDto dto)
        {
            var updated = new Organizator
            {
                Ad = dto.Ad, KurulusTarihi = dto.KurulusTarihi,
                FaaliyetAlani = dto.FaaliyetAlani, Iletisim = dto.Iletisim,
                Tur = dto.Tur, Aciklama = dto.Aciklama,
                UstKurulusId = dto.UstKurulusId
            };
            return await _service.UpdateAsync(id, updated) ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> Delete(Guid id)
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
        public async Task<IActionResult> DeleteKategori(Guid id)
            => await _service.DeleteKategoriAsync(id) ? NoContent() : NotFound();

        // ── Konu ─────────────────────────────────────────────────────────
        [HttpGet("konu")]
        public async Task<IActionResult> GetAllKonu()
            => Ok((await _service.GetAllKonuAsync()).Select(MapKonuToResponse));

        [HttpGet("konu/{id}")]
        public async Task<IActionResult> GetKonuById(Guid id)
        {
            var item = await _service.GetKonuByIdAsync(id);
            return item == null ? NotFound() : Ok(MapKonuToResponse(item));
        }

        [HttpPost("konu")]
        [Authorize(Roles = $"{Roles.IlAdmin},{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> CreateKonu([FromBody] KonuCreateDto dto)
        {
            var created = await _service.CreateKonuAsync(dto.Ad!, dto.Aciklama, dto.Tur, dto.UstKonuId);
            return Ok(MapKonuToResponse(created));
        }

        [HttpPut("konu/{id}")]
        [Authorize(Roles = $"{Roles.IlAdmin},{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> UpdateKonu(Guid id, [FromBody] KonuCreateDto dto)
            => await _service.UpdateKonuAsync(id, dto.Ad!, dto.Aciklama, dto.Tur, dto.UstKonuId) ? NoContent() : NotFound();

        [HttpDelete("konu/{id}")]
        [Authorize(Roles = $"{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> DeleteKonu(Guid id)
            => await _service.DeleteKonuAsync(id) ? NoContent() : NotFound();

        private static KonuResponseDto MapKonuToResponse(Konu k) => new()
        {
            Id = k.Id, Ad = k.Ad, Aciklama = k.Aciklama,
            Tur = k.Tur, UstKonuId = k.UstKonuId,
            UstKonuAd = k.UstKonu?.Ad,
            AltKonuSayisi = k.AltKonular?.Count ?? 0,
            CreatedAt = k.CreatedAt
        };

        private static OrganizatorResponseDto MapToResponse(Organizator o) => new()
        {
            Id = o.Id, Ad = o.Ad, KurulusTarihi = o.KurulusTarihi,
            FaaliyetAlani = o.FaaliyetAlani, Iletisim = o.Iletisim,
            Tur = o.Tur, Aciklama = o.Aciklama,
            UstKurulusId = o.UstKurulusId,
            UstKurulusAd = o.UstKurulus?.Ad,
            AltKurulusSayisi = o.AltKuruluslar?.Count ?? 0
        };
    }
}
