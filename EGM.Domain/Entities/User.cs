using EGM.Domain.Constants;

namespace EGM.Domain.Entities
{
    public class User : BaseEntity
    {
        public int Sicil { get; set; }
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>Varsayılan rol: Izleyici. Başkanlık Yöneticisi veya İl Yöneticisi tarafından değiştirilebilir.</summary>
        public string Role { get; set; } = Roles.Izleyici;

        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string GSM { get; set; } = string.Empty;

        /// <summary>
        /// Kullanıcının bağlı olduğu il plaka kodu (1-81).
        /// IlPersoneli ve IlYoneticisi için zorunludur; Başkanlık kullanıcıları için null.
        /// </summary>
        public int? CityId { get; set; }

        /// <summary>Kullanıcının çalıştığı birim / şube adı. Ör: "İstihbarat Şube Müdürlüğü"</summary>
        public string Birim { get; set; } = string.Empty;
    }
}