using EGM.Domain.Constants;

namespace EGM.Application.DTOs
{
    public class RegisterRequest
    {
        public int Sicil { get; set; }
        public string Password { get; set; } = string.Empty;

        /// <summary>Varsayılan: Izleyici. Başkanlık Yöneticisi haricinde dışarıdan rol verilmemelidir.</summary>
        public string Role { get; set; } = Roles.Izleyici;

        public string GSM { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        /// <summary>İl plaka kodu. IlPersoneli / IlYoneticisi için zorunlu; başkanlık rolleri için null.</summary>
        public int? CityId { get; set; }
    }

    public class LoginRequest
    {
        public int Sicil { get; set; }
        public string Password { get; set; } = string.Empty;
    }
}