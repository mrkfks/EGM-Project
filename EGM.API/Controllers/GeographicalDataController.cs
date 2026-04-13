using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace EGM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GeographicalDataController : ControllerBase
    {
        private static readonly Dictionary<int, string[]> ILCE_MAP = new()
        {
            { 1, new[] { "Aladağ", "Ceyhan", "Çukurova", "Feke", "İmamoğlu", "Karaisalı", "Karataş", "Kozan", "Pozantı", "Saimbeyli", "Sarıçam", "Seyhan", "Tufanbeyli", "Yumurtalık", "Yüreğir" } },
            { 2, new[] { "Besni", "Çelikhan", "Gerger", "Gölbaşı", "Kahta", "Merkez", "Samsat", "Sincik", "Tut" } },
            // ... Add other mappings here
        };

        [HttpGet("ilceler/{ilId}")]
        public IActionResult GetDistrictsByCityId(int ilId)
        {
            if (ILCE_MAP.TryGetValue(ilId, out var districts))
            {
                return Ok(districts);
            }

            return NotFound("City ID not found.");
        }
    }
}