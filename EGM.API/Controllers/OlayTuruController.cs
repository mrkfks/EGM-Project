using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EGM.Infrastructure;
using EGM.Domain.Entities;

namespace EGM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OlayTuruController : ControllerBase
    {
        private readonly EGMDbContext _context;
        public OlayTuruController(EGMDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _context.OlayTurleri
                .Select(x => new { x.Id, x.Name })
                .ToListAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var entity = await _context.OlayTurleri
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.Name })
                .FirstOrDefaultAsync();
            if (entity == null) return NotFound();
            return Ok(entity);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OlayTuru model)
        {
            _context.OlayTurleri.Add(model);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] OlayTuru model)
        {
            var entity = await _context.OlayTurleri.FindAsync(id);
            if (entity == null) return NotFound();
            entity.Name = model.Name;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var entity = await _context.OlayTurleri.FindAsync(id);
            if (entity == null) return NotFound();
            _context.OlayTurleri.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}