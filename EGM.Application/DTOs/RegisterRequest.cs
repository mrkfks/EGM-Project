using System.ComponentModel.DataAnnotations;
using EGM.Domain.Constants;

namespace EGM.Application.DTOs
{
    public class RegisterRequest
    {
        [Range(1, 9999999, ErrorMessage = "Geçerli bir sicil numarası giriniz.")]
        public int Sicil { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [MinLength(8, ErrorMessage = "Şifre en az 8 karakter olmalıdır.")]
        public string Password { get; set; } = string.Empty;

        /// <summary>Varsayılan: Izleyici. Başkanlık Yöneticisi haricinde dışarıdan rol verilmemelidir.</summary>
        public string Role { get; set; } = Roles.Izleyici;

        [Phone(ErrorMessage = "Geçerli bir GSM numarası giriniz.")]
        [StringLength(20)]
        public string GSM { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ad Soyad zorunludur.")]
        [StringLength(250)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [StringLength(250)]
        public string Email { get; set; } = string.Empty;

        /// <summary>İl plaka kodu. IlPersoneli / IlYoneticisi için zorunlu; başkanlık rolleri için null.</summary>
        public int? CityId { get; set; }

        /// <summary>Kullanıcının çalıştığı birim / şube adı.</summary>
        [StringLength(250)]
        public string Birim { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        [Range(1, 9999999, ErrorMessage = "Geçerli bir sicil numarası giriniz.")]
        public int Sicil { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur.")]
        public string Password { get; set; } = string.Empty;
    }
}