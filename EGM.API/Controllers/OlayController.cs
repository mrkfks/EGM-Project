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
        public async Task<IActionResult> CreateOlay([FromBody] OlayCreateDto createDto)
        {
            _logger.LogInformation("Yeni Olay Kaydı: {@OlayDto}", createDto);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdOlay = await _olayService.CreateOlayAsync(createDto);
            return CreatedAtAction(nameof(GetOlayByOlayNo), new { olayNo = createdOlay.OlayNo }, createdOlay);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateOlay(Guid id, [FromBody] OlayCreateDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _olayService.UpdateOlayAsync(id, updateDto);
            if (!result.Success)
                return BadRequest(result.Error);

            return Ok(result);
        }

        [HttpPost("baslat/{id}")]
        public async Task<IActionResult> BaslatOlay(Guid id)
        {
            var result = await _olayService.BaslatOlayAsync(id);
            return result != null ? Ok(result) : NotFound();
        }

        [HttpPost("bitir/{id}")]
        public async Task<IActionResult> BitirOlay(Guid id, [FromBody] EventDetailDto details)
        {
            var result = await _olayService.BitirOlayAsync(id, details);
            return result != null ? Ok(result) : NotFound();
        }

        [HttpPost("iptal/{id}")]
        public async Task<IActionResult> IptalEtOlay(Guid id)
        {
            var result = await _olayService.IptalEtOlayAsync(id);
            return result != null ? Ok(result) : NotFound();
        }

        [HttpPost("filtre")]
        public async Task<IActionResult> GetFilteredForMap([FromBody] OlayFilterDto filter)
        {
            var result = await _olayService.GetFilteredMapOlaylarAsync(filter);
            return Ok(result);
        }

        [HttpGet("{olayNo}")]
        public async Task<IActionResult> GetOlayByOlayNo(string olayNo)
        {
            var olay = await _olayService.GetByOlayNoAsync(olayNo);
            if (olay == null)
                return NotFound(new { Message = $"Olay bulunamadı: {olayNo}" });

            return Ok(olay);
        }

        [HttpGet("id/{id}")]
        public async Task<IActionResult> GetOlayById(Guid id)
        {
            var olay = await _olayService.GetByIdAsync(id);
            if (olay == null)
                return NotFound();

            return Ok(olay);
        }
    }
}

