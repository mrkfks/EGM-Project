using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EGM.Domain.Constants;
using EGM.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace EGM.Infrastructure.Security
{
    /// <summary>
    /// HTTP kontekstindeki JWT claim'lerinden aktif kullanıcı bilgilerini okur.
    /// </summary>
    public sealed class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
            => _httpContextAccessor = httpContextAccessor;

        private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

        public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

        /// <summary>JWT sub claim → sicil numarası string olarak.</summary>
        public string UserId =>
            Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? Principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? string.Empty;

        /// <summary>JWT "role" claim → rol adı.</summary>
        public string Role =>
            Principal?.FindFirst(ClaimTypes.Role)?.Value
            ?? Principal?.FindFirst("role")?.Value
            ?? Roles.Izleyici;

        /// <summary>JWT "cityId" claim → plaka kodu.</summary>
        public int? CityId
        {
            get
            {
                var raw = Principal?.FindFirst("cityId")?.Value;
                return int.TryParse(raw, out var id) ? id : null;
            }
        }
    }
}
