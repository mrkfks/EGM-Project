namespace EGM.Domain.Interfaces
{
    /// <summary>
    /// Aktif HTTP isteğini yapan kullanıcının kimlik bilgilerini sağlar.
    /// JWT claim'lerinden beslenir; Infrastructure katmanında uygulanır.
    /// </summary>
    public interface ICurrentUserService
    {
        /// <summary>Kullanıcının sicil numarası (JWT sub claim).</summary>
        string UserId { get; }

        /// <summary>Kullanıcının rolü (Izleyici, IlPersoneli, …).</summary>
        string Role { get; }

        /// <summary>
        /// Kullanıcının bağlı olduğu il plaka kodu.
        /// IlPersoneli ve IlYoneticisi için dolu; Başkanlık rolleri için null.
        /// </summary>
        int? CityId { get; }

        /// <summary>Kullanıcı kimliği doğrulanmış mı?</summary>
        bool IsAuthenticated { get; }
    }
}
