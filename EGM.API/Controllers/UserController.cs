
using EGM.Application.DTOs;
using EGM.Application.Services;
using EGM.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EGM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Roles = $"{Roles.IlAdmin},{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
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
        [Authorize(Roles = $"{Roles.BaskanlikAdmin},{Roles.Yonetici}")]
        public IActionResult Delete(int sicil)
        {
            _userService.DeleteUser(sicil);
            return Ok("Kullanıcı silindi.");
        }
    }
}
