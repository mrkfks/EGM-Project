using EGM.Application.DTOs;
using EGM.Application.Helpers;
using EGM.Application.Services;
using EGM.Domain.Constants;
using EGM.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EGM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(UserService userService, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _userService   = userService;
            _configuration = configuration;
            _logger        = logger;
        }

        [HttpPost("login")]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await _userService.ValidateUserAsync(request.Sicil, request.Password);
            if (user == null)
            {
                _logger.LogWarning("Başarısız giriş denemesi. Sicil: {Sicil}, IP: {IP}",
                    request.Sicil, HttpContext.Connection.RemoteIpAddress);
                return Unauthorized("Geçersiz sicil veya şifre.");
            }

            _logger.LogInformation("Başarılı giriş. Sicil: {Sicil}, Rol: {Role}", user.Sicil, user.Role);
            var token = JwtHelper.GenerateToken(
                user,
                _configuration["Jwt:Key"]!,
                _configuration["Jwt:Issuer"]!,
                _configuration["Jwt:Audience"]!
            );

            return Ok(new { Token = token });
        }

        /// <summary>
        /// Sisteme yeni personel kaydeder.
        /// Kimlik doğrulaması yapılmış yöneticiler (IlYoneticisi, BaskanlikYoneticisi, Yetkili)
        /// istedikleri rolü atayabilir. Diğer durumlarda rol Izleyici olarak atanır.
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            // Yalnızca yönetici rolleri özel rol atayabilir
            var rol = Roles.Izleyici;
            if (!string.IsNullOrWhiteSpace(request.Role)
                && User.Identity?.IsAuthenticated == true
                && (User.IsInRole(Roles.IlAdmin)
                    || User.IsInRole(Roles.BaskanlikAdmin)
                    || User.IsInRole(Roles.Yonetici)))
            {
                rol = request.Role;
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
            var user = new User
            {
                Sicil        = request.Sicil,
                PasswordHash = hashedPassword,
                Role         = rol,
                GSM          = request.GSM,
                FullName     = request.FullName,
                Email        = request.Email,
                CityId       = null,
                Birim        = request.Birim
            };

            await _userService.RegisterUserAsync(user);
            _logger.LogInformation("Yeni kullanıcı kaydedildi. Sicil: {Sicil}", user.Sicil);
            return Ok("Kullanıcı başarıyla kaydedildi. Rol ataması için bir yöneticiye başvurunuz.");
        }
    }
}
