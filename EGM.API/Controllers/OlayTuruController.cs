using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EGM.Domain.Entities;
using EGM.Domain.Constants;
using EGM.Domain.Interfaces;

namespace EGM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OlayTuruController : ControllerBase
    {
        private readonly IRepository<OlayTuru> _repo;
        public OlayTuruController(IRepository<OlayTuru> repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _repo.ListAllAsync();
            return Ok(list.Select(x => new { x.Id, x.Name }));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return NotFound();
            return Ok(new { entity.Id, entity.Name });
        }

        [HttpPost]
        [Authorize(Roles = $"{Roles.IlAdmin},{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> Create([FromBody] OlayTuru model)
        {
            var created = await _repo.AddAsync(model);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, new { created.Id, created.Name });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{Roles.IlAdmin},{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] OlayTuru model)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return NotFound();
            entity.Name = model.Name;
            await _repo.UpdateAsync(entity);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return NotFound();
            await _repo.DeleteAsync(entity);
            return NoContent();
        }
    }
}