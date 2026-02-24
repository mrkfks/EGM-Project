using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EGM.Domain.Entities;
using Microsoft.IdentityModel;
using Microsoft.IdentityModel.Tokens;


namespace EGM.Application.Helpers
{
    public static class JwtHelper
    {
        public static string GenerateToken(User user, string key, string issuer)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Sicil.ToString()),
                new Claim("role", user.Role),
                new Claim("gsm", user.GSM),
                new Claim("email", user.Email)
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer,
                issuer,
                claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);

        }
    }
}