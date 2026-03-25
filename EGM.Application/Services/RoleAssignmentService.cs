using EGM.Domain.Constants;
using EGM.Domain.Interfaces;

namespace EGM.Application.Services
{
    /// <summary>
    /// Hiyerarşik rol atama servisi.
    ///
    /// Atama zinciri:
    ///   Başkanlık Yöneticisi → tüm rolleri atayabilir (IlYoneticisi, BaskanlikPersoneli dahil)
    ///   İl Yöneticisi        → yalnızca kendi ilindeki Izleyici'yi IlPersoneli yapabilir
    /// </summary>
    public sealed class RoleAssignmentService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUserService _currentUser;

        public RoleAssignmentService(IUserRepository userRepository, ICurrentUserService currentUser)
        {
            _userRepository = userRepository;
            _currentUser    = currentUser;
        }

        /// <summary>
        /// Hedef kullanıcıya yeni bir rol atar.
        /// </summary>
        /// <exception cref="UnauthorizedAccessException">Atayan yeterli yetkiye sahip değilse.</exception>
        /// <exception cref="InvalidOperationException">Atama kuralları ihlal edilmişse.</exception>
        /// <exception cref="KeyNotFoundException">Hedef kullanıcı bulunamazsa.</exception>
        /// <param name="cityIdOverride">
        /// Yalnızca Başkanlık Yöneticisi kullanır: hedef kullanıcıya atanacak il.
        /// </param>
        public void AssignRole(int targetSicil, string newRole, int? cityIdOverride = null)
        {
            var assignerRole = _currentUser.Role;

            // Yalnızca İl Yöneticisi ve Başkanlık Yöneticisi rol atayabilir
            if (assignerRole != Roles.IlYoneticisi && assignerRole != Roles.BaskanlikYoneticisi)
                throw new UnauthorizedAccessException("Rol atama yetkisine sahip değilsiniz.");

            // Atanan rol, atayan rolden daha yüksek olamaz
            if (!Roles.IsAbove(assignerRole, newRole))
                throw new UnauthorizedAccessException(
                    $"'{assignerRole}' rolü '{newRole}' rolünü atayamaz; hiyerarşi ihlali.");

            var target = _userRepository.GetBySicil(targetSicil)
                ?? throw new KeyNotFoundException($"Sicil {targetSicil} bulunamadı.");

            // İl Yöneticisi kısıtlamaları ─────────────────────────────────
            if (assignerRole == Roles.IlYoneticisi)
            {
                // Sadece Izleyici → IlPersoneli dönüşümüne izin verilir
                if (newRole != Roles.IlPersoneli)
                    throw new InvalidOperationException(
                        "İl Yöneticisi yalnızca 'Izleyici' kullanıcıyı 'İl Personeli' olarak atayabilir.");

                if (target.Role != Roles.Izleyici)
                    throw new InvalidOperationException(
                        "Atanacak kullanıcı 'Izleyici' rolünde olmalıdır.");

                // Aynı il kısıtı
                if (target.CityId != _currentUser.CityId)
                    throw new UnauthorizedAccessException(
                        "Başka ilin personeline rol atayamazsınız.");
            }

            target.Role = newRole;

            // İl personeli olarak atanıyorsa CityId senkronize edilir
            if (newRole == Roles.IlPersoneli || newRole == Roles.IlYoneticisi)
            {
                if (assignerRole == Roles.IlYoneticisi)
                    target.CityId = _currentUser.CityId;
                else if (cityIdOverride.HasValue)
                    target.CityId = cityIdOverride;
            }

            _userRepository.Update(target);
        }
    }
}
