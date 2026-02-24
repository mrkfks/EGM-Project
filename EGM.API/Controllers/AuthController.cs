using EGM.Application.DTOs;
using EGM.Application.Helpers;
using EGM.Application.Services;
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
            _userService = userService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public IActionResult Login(LoginRequest request)
        {
            var user = _userService.ValidateUser(request.Sicil, request.Password);
            if (user == null)
            return Unauthorized("Geçersiz sicil veya şifre");

            //JWT token üretimi
            var token = JwtHelper.GenerateToken(
                user,
                _configuration["Jwt:Key"]!,
                _configuration["Jwt:Issuer"]!
            );
            return Ok(new{Token = token});
        }

        [HttpPost("register")]
        public IActionResult Register(RegisterRequest request)
        {
            var user = new User
            {
                Sicil = request.Sicil,
                PasswordHash = request.Password,
                Role = request.Role,
                GSM = request.GSM,
                FullName = request.FullName,
                Email = request.Email
            };

            _userService.RegisterUser(user);
            return Ok ("Kullanıcı başarıyla kaydedildi.");
        }
    }
}