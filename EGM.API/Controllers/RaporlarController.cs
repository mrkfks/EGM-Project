using EGM.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EGM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RaporlarController : ControllerBase
    {
        private readonly RaporlarService _raporlarService;

        public RaporlarController(RaporlarService raporlarService)
        {
            _raporlarService = raporlarService;
        }

        /// <summary>
        /// Belirtilen tarihe ait günlük bülten verisini döner.
        /// </summary>
        /// <param name="tarih">Rapor tarihi (yyyy-MM-dd). Boş bırakılırsa bugün kullanılır.</param>
        [HttpGet("gunluk-bulten")]
        public async Task<IActionResult> GetGunlukBulten([FromQuery] DateTime? tarih)
        {
            var raporTarihi = tarih.HasValue ? tarih.Value.Date : DateTime.Today;
            var bulten = await _raporlarService.GetGunlukBultenAsync(raporTarihi);
            return Ok(bulten);
        }
    }
}
