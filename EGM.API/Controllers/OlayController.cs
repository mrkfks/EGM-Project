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
    public class OlayController : ControllerBase
    {
        private readonly OlayService _olayService;

        public OlayController(OlayService olayService)
        {
            _olayService = olayService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var olaylar = await _olayService.GetAllAsync();
            var result = olaylar.Select(o => MapToResponse(o));
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var olay = await _olayService.GetByIdAsync(id);
            if (olay == null) return NotFound();
            return Ok(MapToResponse(olay));
        }

        [HttpGet("{id}/rota")]
        public async Task<IActionResult> GetRota(int id)
        {
            var rota = await _olayService.GetRotaAsync(id);
            return Ok(rota);
        }

        [HttpPost]
        [Authorize(Roles = $"{Roles.IlAdmin},{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> Create([FromBody] OlayCreateDto dto)
        {
            var olay = new Olay
            {
                Baslik = dto.Baslik,
                OlayTuru = dto.OlayTuru,
                OrganizatorId = dto.OrganizatorId,
                KonuId = dto.KonuId,
                Tarih = dto.Tarih,
                BaslangicSaati = dto.BaslangicSaati,
                BitisSaati = dto.BitisSaati,
                Il = dto.Il,
                Ilce = dto.Ilce,
                Mekan = dto.Mekan,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                KatilimciSayisi = dto.KatilimciSayisi,
                Aciklama = dto.Aciklama,
                KaynakKurum = dto.KaynakKurum,
                Hassasiyet = dto.Hassasiyet
            };
            var created = await _olayService.CreateOlayAsync(olay);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToResponse(created));
        }

        [HttpPost("{id}/rota")]
        [Authorize(Roles = $"{Roles.IlAdmin},{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> AddRota(int id, [FromBody] RotaNoktasiCreateDto dto)
        {
            var rota = await _olayService.AddRotaNoktasiAsync(id, dto.NoktaAdi!, dto.Latitude, dto.Longitude, dto.SiraNo);
            return Ok(rota);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{Roles.IlAdmin},{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> Update(int id, [FromBody] OlayCreateDto dto)
        {
            var updated = new Olay
            {
                Baslik = dto.Baslik,
                OlayTuru = dto.OlayTuru,
                OrganizatorId = dto.OrganizatorId,
                KonuId = dto.KonuId,
                Tarih = dto.Tarih,
                BaslangicSaati = dto.BaslangicSaati,
                BitisSaati = dto.BitisSaati,
                Il = dto.Il,
                Ilce = dto.Ilce,
                Mekan = dto.Mekan,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                KatilimciSayisi = dto.KatilimciSayisi,
                Aciklama = dto.Aciklama,
                KaynakKurum = dto.KaynakKurum,
                Hassasiyet = dto.Hassasiyet
            };
            return await _olayService.UpdateOlayAsync(id, updated) ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> Delete(int id)
            => await _olayService.DeleteOlayAsync(id) ? NoContent() : NotFound();

        [HttpPut("{id}/baslat")]
        [Authorize(Roles = $"{Roles.IlAdmin},{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> Baslat(int id)
        {
            var olay = await _olayService.BaslatOlayAsync(id);
            if (olay == null) return NotFound();
            return Ok(MapToResponse(olay));
        }

        [HttpPut("{id}/bitir")]
        [Authorize(Roles = $"{Roles.IlAdmin},{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> Bitir(int id)
        {
            var olay = await _olayService.BitirOlayAsync(id);
            if (olay == null) return NotFound();
            return Ok(MapToResponse(olay));
        }

        [HttpPut("{id}/iptal")]
        [Authorize(Roles = $"{Roles.IlAdmin},{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> Iptal(int id)
        {
            var olay = await _olayService.IptalEtOlayAsync(id);
            if (olay == null) return NotFound();
            return Ok(MapToResponse(olay));
        }

        private static OlayResponseDto MapToResponse(Olay o) => new()
        {
            Id = o.Id,
            Baslik = o.Baslik,
            OlayTuru = o.OlayTuru,
            OrganizatorId = o.OrganizatorId,
            KonuId = o.KonuId,
            Tarih = o.Tarih,
            Il = o.Il,
            Ilce = o.Ilce,
            Mekan = o.Mekan,
            Latitude = o.Latitude,
            Longitude = o.Longitude,
            KatilimciSayisi = o.KatilimciSayisi,
            Aciklama = o.Aciklama,
            KaynakKurum = o.KaynakKurum,
            Durum = o.Durum,
            Hassasiyet = o.Hassasiyet,
            RiskPuani = o.RiskPuani,
            GercekBaslangicTarihi = o.GercekBaslangicTarihi,
            GercekBitisTarihi = o.GercekBitisTarihi
        };
    }
}
