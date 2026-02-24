
using EGM.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace EGM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _userService.GetAllUsers();
            return Ok(users);
        }

        [HttpDelete("{sicil}")]
        public IActionResult Delete(int sicil)
        {
            _userService.DeleteUser(sicil);
            return Ok("Kullanıcı silindi.");
        }
    }
}