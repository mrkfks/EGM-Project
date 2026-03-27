using EGM.Application.DTOs;
using EGM.Application.Services;
using EGM.Domain.Entities;
using EGM.Domain.Enums;
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

        /// <summary>
        /// Kayıt yapılmadan önce risk puanı ön izlemesi hesaplar.
        /// Max puan: 55 (Katılımcı 10 + Hassasiyet 20 + Yürüyüş 15 + Sosyal 10).
        /// </summary>
        [HttpPost("risk-preview")]
        public IActionResult RiskPreview([FromBody] RiskPreviewRequestDto dto)
        {
            double raw = 0;
            if (dto.KatilimciSayisi.HasValue && dto.KatilimciSayisi.Value > 1000) raw += 10;
            raw += dto.Hassasiyet switch
            {
                Hassasiyet.Kritik  => 20,
                Hassasiyet.Yuksek => 12,
                Hassasiyet.Orta   => 5,
                _                 => 0
            };
            if (dto.OlayTuru == "Yürüyüş") raw += 15;
            if (dto.SosyalSignalSkoru > 70)   raw += 10;

            const double max = 55.0;
            var normalized = Math.Round(raw / max, 3);
            var seviye = normalized switch
            {
                >= 0.8 => "Kritik",
                >= 0.6 => "Yüksek",
                >= 0.4 => "Orta",
                _      => "Düşük"
            };
            return Ok(new RiskPreviewResponseDto
            {
                RiskPuaniRaw        = raw,
                RiskPuaniNormalized = normalized,
                Seviye              = seviye
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int sayfa = 1,
            [FromQuery] int sayfaBoyutu = 20,
            [FromQuery] OlayDurum? durum = null,
            [FromQuery] DateTime? tarihBaslangic = null,
            [FromQuery] DateTime? tarihBitis = null)
        {
            if (sayfa < 1) sayfa = 1;
            if (sayfaBoyutu < 1 || sayfaBoyutu > 500) sayfaBoyutu = 20;

            var paged = await _olayService.GetAllAsync(sayfa, sayfaBoyutu, durum, tarihBaslangic, tarihBitis);
            return Ok(new
            {
                paged.TotalCount,
                paged.Page,
                paged.PageSize,
                paged.TotalPages,
                paged.HasNextPage,
                paged.HasPreviousPage,
                Items = paged.Items.Select(o => MapToResponse(o))
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var olay = await _olayService.GetByIdAsync(id);
            if (olay == null) return NotFound();
            return Ok(MapToResponse(olay));
        }

        [HttpGet("{id}/rota")]
        public async Task<IActionResult> GetRota(Guid id)
        {
            var rota = await _olayService.GetRotaAsync(id);
            return Ok(rota);
        }

        [HttpPost]
        [Authorize(Policy = "CityStaffOrAbove")]
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
                GozaltiSayisi = dto.GozaltiSayisi,
                SehitOluSayisi = dto.SehitOluSayisi,
                Aciklama = dto.Aciklama,
                KaynakKurum = dto.KaynakKurum,
                EvrakNumarasi = dto.EvrakNumarasi,
                Hassasiyet = dto.Hassasiyet,
                CityId = dto.CityId
            };
            // CreatedByUserId ve CityId otomatik atama OlayService içinde yapılır
            var created = await _olayService.CreateOlayAsync(olay);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToResponse(created));
        }

        [HttpPost("{id}/rota")]
        [Authorize(Policy = "CityStaffOrAbove")]
        public async Task<IActionResult> AddRota(Guid id, [FromBody] RotaNoktasiCreateDto dto)
        {
            var rota = await _olayService.AddRotaNoktasiAsync(id, dto.NoktaAdi!, dto.Latitude, dto.Longitude, dto.SiraNo);
            return Ok(rota);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "CityStaffOrAbove")]
        public async Task<IActionResult> Update(Guid id, [FromBody] OlayCreateDto dto)
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
                GozaltiSayisi = dto.GozaltiSayisi,
                SehitOluSayisi = dto.SehitOluSayisi,
                Aciklama = dto.Aciklama,
                KaynakKurum = dto.KaynakKurum,
                EvrakNumarasi = dto.EvrakNumarasi,
                Hassasiyet = dto.Hassasiyet
            };
            var (success, error) = await _olayService.UpdateOlayAsync(id, updated);
            if (!success && error != null) return Forbid();
            if (!success)                  return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "HQManagerOnly")]
        public async Task<IActionResult> Delete(Guid id)
            => await _olayService.DeleteOlayAsync(id) ? NoContent() : NotFound();

        [HttpPut("{id}/baslat")]
        [Authorize(Policy = "CityManagerOrAbove")]
        public async Task<IActionResult> Baslat(Guid id)
        {
            var olay = await _olayService.BaslatOlayAsync(id);
            if (olay == null) return NotFound();
            return Ok(MapToResponse(olay));
        }

        [HttpPut("{id}/bitir")]
        [Authorize(Policy = "CityManagerOrAbove")]
        public async Task<IActionResult> Bitir(Guid id)
        {
            var olay = await _olayService.BitirOlayAsync(id);
            if (olay == null) return NotFound();
            return Ok(MapToResponse(olay));
        }

        [HttpPut("{id}/iptal")]
        [Authorize(Policy = "CityManagerOrAbove")]
        public async Task<IActionResult> Iptal(Guid id)
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
            OrganizatorAd = o.Organizator?.Ad,
            KonuId = o.KonuId,
            KonuAd = o.Konu?.Ad,
            Tarih = o.Tarih,
            BaslangicSaati = o.BaslangicSaati,
            BitisSaati = o.BitisSaati,
            Il = o.Il,
            Ilce = o.Ilce,
            Mekan = o.Mekan,
            Latitude = o.Latitude,
            Longitude = o.Longitude,
            KatilimciSayisi = o.KatilimciSayisi,
            GozaltiSayisi = o.GozaltiSayisi,
            SehitOluSayisi = o.SehitOluSayisi,
            Aciklama = o.Aciklama,
            KaynakKurum = o.KaynakKurum,
            EvrakNumarasi = o.EvrakNumarasi,
            Durum = o.Durum,
            Hassasiyet = o.Hassasiyet,
            RiskPuani = o.RiskPuani,
            GercekBaslangicTarihi = o.GercekBaslangicTarihi,
            GercekBitisTarihi = o.GercekBitisTarihi,
            CreatedByUserId = o.CreatedByUserId,
            CityId = o.CityId
        };
    }
}
