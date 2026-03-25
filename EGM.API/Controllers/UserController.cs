
using EGM.Application.DTOs;
using EGM.Application.Services;
using EGM.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EGM.API.Controllers
{
    /// <summary>
    /// Rol atama endpoint'i dahil kullanıcı yönetimi.
    ///
    ///  POST /api/user/{sicil}/rol-ata
    ///     İl Yöneticisi  → yalnızca kendi ilindeki Izleyici'yi IlPersoneli yapabilir.
    ///     Başkanlık Yön. → her kullanıcıya her rolü (kendi altındakileri) atayabilir.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly RoleAssignmentService _roleAssignmentService;

        public UserController(UserService userService, RoleAssignmentService roleAssignmentService)
        {
            _userService           = userService;
            _roleAssignmentService = roleAssignmentService;
        }

        [HttpGet]
        [Authorize(Policy = "CityManagerOrAbove")]
        public IActionResult GetAll()
        {
            var users = _userService.GetAllUsers().Select(u => new UserResponseDto
            {
                Id = u.Id, Sicil = u.Sicil, Role = u.Role,
                FullName = u.FullName, Email = u.Email, GSM = u.GSM
            });
            return Ok(users);
        }

        [HttpDelete("{sicil}")]
        [Authorize(Policy = "HQManagerOnly")]
        public IActionResult Delete(int sicil)
        {
            _userService.DeleteUser(sicil);
            return Ok("Kullanıcı silindi.");
        }

        /// <summary>
        /// Hiyerarşik rol atama.
        /// İl Yöneticisi: Izleyici → IlPersoneli (aynı il, kendi ilindeki kullanıcı)
        /// Başkanlık Yöneticisi: tüm roller
        /// </summary>
        [HttpPost("{sicil}/rol-ata")]
        [Authorize(Policy = "CityManagerOrAbove")]
        public IActionResult RolAta(int sicil, [FromBody] RolAtamaDto dto)
        {
            try
            {
                _roleAssignmentService.AssignRole(sicil, dto.YeniRol, dto.CityId);
                return Ok(new { Mesaj = $"Sicil {sicil} kullanıcısına '{dto.YeniRol}' rolü atandı." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Hata = ex.Message });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { Hata = "Kullanıcı bulunamadı." });
            }
        }
    }
}
