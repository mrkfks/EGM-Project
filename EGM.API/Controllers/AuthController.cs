using EGM.Application.DTOs;
using EGM.Application.Helpers;
using EGM.Application.Services;
using EGM.Domain.Constants;
using EGM.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EGM.API.Controllers
{
    [ApiController]
    [Route("Api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly IConfiguration _configuration;

        public AuthController(UserService userService, IConfiguration configuration)
        {
            _userService   = userService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public IActionResult Login(LoginRequest request)
        {
            var user = _userService.ValidateUser(request.Sicil, request.Password);
            if (user == null)
                return Unauthorized("Geçersiz sicil veya şifre.");

            var token = JwtHelper.GenerateToken(
                user,
                _configuration["Jwt:Key"]!,
                _configuration["Jwt:Issuer"]!
            );

            return Ok(new { Token = token });
        }

        /// <summary>
        /// Sisteme yeni personel kaydeder. Varsayılan rol: Izleyici.
        /// Rol ve CityId atama işlemi için /api/user/{sicil}/rol-ata kullanılmalıdır.
        /// </summary>
        [HttpPost("register")]
        public IActionResult Register(RegisterRequest request)
        {
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
            var user = new User
            {
                Sicil        = request.Sicil,
                PasswordHash = hashedPassword,
                Role         = Roles.Izleyici,  // Yeni giren herkes İzleyici olarak başlar
                GSM          = request.GSM,
                FullName     = request.FullName,
                Email        = request.Email,
                CityId       = null             // CityId daha sonra rol atamasıyla belirlenir
            };

            _userService.RegisterUser(user);
            return Ok("Kullanıcı başarıyla kaydedildi. Rol ataması için bir yöneticiye başvurunuz.");
        }
    }
}
