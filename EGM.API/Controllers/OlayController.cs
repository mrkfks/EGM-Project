using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using EGM.Application.DTOs;
using EGM.Application.Services;
using EGM.Domain.Entities;
using EGM.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace EGM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OlayController : ControllerBase
    {
        private readonly IOlayService _olayService;
        private readonly ILogger<OlayController> _logger;

        public OlayController(IOlayService olayService, ILogger<OlayController> logger)
        {
            _olayService = olayService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetOlaylar([FromQuery] OlayDurum? durum = null, [FromQuery] int sayfaBoyutu = 100)
        {
            var olaylar = await _olayService.GetOlaylarAsync(durum, sayfaBoyutu);
            return Ok(olaylar);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllOlaylar()
        {
            var olaylar = await _olayService.GetAllOlaylarAsync();
            return Ok(olaylar);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOlay([FromBody] OlayDto olayDto)
        {
            _logger.LogInformation("Gelen veri: {@OlayDto}", olayDto);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdOlay = await _olayService.CreateOlayAsync(olayDto);
            return CreatedAtAction(nameof(GetOlaylar), new { takipNo = createdOlay.TakipNo }, createdOlay);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateOlay(Guid id, [FromBody] OlayDto olayDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _olayService.UpdateOlayAsync(id, olayDto);
            if (!result.Success)
                return BadRequest(result.Error);

            return Ok(result);
        }

        [HttpPost("filtre")]
        public async Task<IActionResult> GetFilteredForMap([FromBody] OlayFilterDto filter)
        {
            var result = await _olayService.GetFilteredMapOlaylarAsync(filter);
            
            return Ok(new
            {
                items = result.Items.Select(o => new
                {
                    o.Id,
                    o.OlayTuru,
                    organizatorId = o.OrganizatorId,
                    o.OrganizatorId,
                    konuId = o.KonuId,
                    o.KonuId,
                    tarih = o.Tarih.ToString("O"),
                    o.BaslangicSaati,
                    o.BitisSaati,
                    o.Il,
                    o.Ilce,
                    o.Mahalle,
                    o.Mekan,
                    o.Latitude,
                    o.Longitude,
                    o.KatilimciSayisi,
                    o.GozaltiSayisi,
                    o.SehitOluSayisi,
                    o.Aciklama,
                    o.EvrakNumarasi,
                    durum = (int)o.Durum,
                    hassasiyet = (int)o.Hassasiyet,
                    o.Tarih,
                    gercekBaslangicTarihi = o.GercekBaslangicTarihi?.ToString("O"),
                    gercekBitisTarihi = o.GercekBitisTarihi?.ToString("O"),
                    o.CreatedByUserId,
                    o.CityId,
                    o.TakipNo,
                    gerceklesmeSekliId = o.GerceklesmeSekliId?.ToString(),
                    organizatorAd = o.Organizator?.Ad,
                    konuAd = o.Konu?.Ad
                }).ToList(),
                totalCount = result.TotalCount,
                result.Page,
                result.PageSize,
                result.TotalPages,
                result.HasNextPage,
                result.HasPreviousPage
            });
        }

        [HttpGet("{takipNo}")]
        public async Task<IActionResult> GetOlayByTakipNo(string takipNo)
        {
            var olay = await _olayService.GetByTakipNoAsync(takipNo);
            if (olay == null)
            {
                return NotFound(new { Message = $"Olay bulunamadı: {takipNo}" });
            }

            return Ok(olay);
        }

        private static OlayResponseDto MapToResponse(Olay o) => new OlayResponseDto
        {
            Id = o.Id,
            OlayTuru = o.OlayTuru,
            Durum = o.Durum,
            Tarih = o.Tarih,
            Il = o.Il,
            Ilce = o.Ilce,
            Mahalle = o.Mahalle,
            Hassasiyet = o.Hassasiyet,
            KatilimciSayisi = o.KatilimciSayisi,
            Aciklama = o.Aciklama
        };
    }
}
