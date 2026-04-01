using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EGM.Infrastructure;
using EGM.Domain.Entities;

namespace EGM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GerceklesmeSekliController : ControllerBase
    {
        private readonly EGMDbContext _context;
        public GerceklesmeSekliController(EGMDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _context.GerceklesmeSekilleri
                .Select(x => new { x.Id, x.Name, x.OlayTuruId })
                .ToListAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var entity = await _context.GerceklesmeSekilleri
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.Name, x.OlayTuruId })
                .FirstOrDefaultAsync();
            if (entity == null) return NotFound();
            return Ok(entity);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] GerceklesmeSekli model)
        {
            _context.GerceklesmeSekilleri.Add(model);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] GerceklesmeSekli model)
        {
            var entity = await _context.GerceklesmeSekilleri.FindAsync(id);
            if (entity == null) return NotFound();
            entity.Name = model.Name;
            entity.OlayTuruId = model.OlayTuruId;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var entity = await _context.GerceklesmeSekilleri.FindAsync(id);
            if (entity == null) return NotFound();
            _context.GerceklesmeSekilleri.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}