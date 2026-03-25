
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
        public async Task<IActionResult> GetAll()
        {
            var users = (await _userService.GetAllUsersAsync()).Select(u => new UserResponseDto
            {
                Id = u.Id, Sicil = u.Sicil, Role = u.Role,
                FullName = u.FullName, Email = u.Email, GSM = u.GSM
            });
            return Ok(users);
        }

        [HttpDelete("{sicil}")]
        [Authorize(Policy = "HQManagerOnly")]
        public async Task<IActionResult> Delete(int sicil)
        {
            await _userService.DeleteUserAsync(sicil);
            return Ok("Kullanıcı silindi.");
        }

        /// <summary>
        /// Hiyerarşik rol atama.
        /// İl Yöneticisi: Izleyici → IlPersoneli (aynı il, kendi ilindeki kullanıcı)
        /// Başkanlık Yöneticisi: tüm roller
        /// </summary>
        [HttpPost("{sicil}/rol-ata")]
        [Authorize(Policy = "CityManagerOrAbove")]
        public async Task<IActionResult> RolAta(int sicil, [FromBody] RolAtamaDto dto)
        {
            await _roleAssignmentService.AssignRoleAsync(sicil, dto.YeniRol, dto.CityId);
            return Ok(new { Mesaj = $"Sicil {sicil} kullanıcısına '{dto.YeniRol}' rolü atandı." });
        }
    }
}
