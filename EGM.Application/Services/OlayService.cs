
using EGM.Domain.Constants;
using EGM.Domain.Entities;
using EGM.Domain.Enums;
using EGM.Domain.Interfaces;

namespace EGM.Application.Services
{
    public class OlayService
    {
        private readonly IRepository<Olay> _olayRepository;
        private readonly IRepository<YuruyusRota> _rotaRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IInAppNotificationService _notificationService;

        public OlayService(
            IRepository<Olay> olayRepository,
            IRepository<YuruyusRota> rotaRepository,
            ICurrentUserService currentUser,
            IInAppNotificationService notificationService)
        {
            _olayRepository      = olayRepository;
            _rotaRepository      = rotaRepository;
            _currentUser         = currentUser;
            _notificationService = notificationService;
        }

        // ── Listeleme ────────────────────────────────────────────────────
        /// <summary>
        /// İl Personeli / İl Yöneticisi → yalnızca kendi illerinin olaylarını görür.
        /// Başkanlık rolleri → tüm olayları görür.
        /// </summary>
        public async Task<IReadOnlyList<Olay>> GetAllAsync()
        {
            if (Roles.IsCityScoped(_currentUser.Role) && _currentUser.CityId.HasValue)
                return await _olayRepository.FindAsync(o => o.CityId == _currentUser.CityId);

            return await _olayRepository.ListAllAsync();
        }

        // ── ID ile getir ─────────────────────────────────────────────────
        public async Task<Olay?> GetByIdAsync(Guid id)
        {
            var olay = await _olayRepository.GetByIdAsync(id);
            if (olay == null) return null;

            // İl kısıtlı kullanıcı sadece kendi ilinin olayını görebilir
            if (Roles.IsCityScoped(_currentUser.Role)
                && _currentUser.CityId.HasValue
                && olay.CityId != _currentUser.CityId)
                return null;

            return olay;
        }

        // ── Oluşturma ────────────────────────────────────────────────────
        public async Task<Olay> CreateOlayAsync(Olay olay)
        {
            olay.CreatedByUserId = _currentUser.UserId;
            olay.Durum           = OlayDurum.Planlandi;
            olay.RiskPuani       = CalculateRisk(olay);

            // İl personeli → CityId otomatik atanır
            if (Roles.IsCityScoped(_currentUser.Role) && _currentUser.CityId.HasValue)
                olay.CityId = _currentUser.CityId;

            var created = await _olayRepository.AddAsync(olay);
            await _notificationService.NotifyOlayRiskAsync(created);
            return created;
        }
        /// <summary>
        /// Güncelleme yetkisi:
        ///   • Başkanlık Yöneticisi / İl Yöneticisi → her kaydı düzenleyebilir.
        ///   • İl Personeli / Başkanlık Personeli   → yalnızca kendi oluşturduğu kaydı.
        /// </summary>
        public async Task<(bool Success, string? Error)> UpdateOlayAsync(Guid id, Olay updated)
        {
            var existing = await _olayRepository.GetByIdAsync(id);
            if (existing == null) return (false, null);

            // İl kısıtı
            if (Roles.IsCityScoped(_currentUser.Role)
                && _currentUser.CityId.HasValue
                && existing.CityId != _currentUser.CityId)
                return (false, "Bu il verisi üzerinde yetkiniz bulunmamaktadır.");

            // Personel rolleri → sadece kendi kaydı
            var isStaffRole = _currentUser.Role is Roles.IlPersoneli or Roles.BaskanlikPersoneli;
            if (isStaffRole && existing.CreatedByUserId != _currentUser.UserId)
                return (false, "Yalnızca kendi oluşturduğunuz kayıtları düzenleyebilirsiniz.");

            existing.Baslik         = updated.Baslik;
            existing.OlayTuru       = updated.OlayTuru;
            existing.OrganizatorId  = updated.OrganizatorId;
            existing.KonuId         = updated.KonuId;
            existing.Tarih          = updated.Tarih;
            existing.BaslangicSaati = updated.BaslangicSaati;
            existing.BitisSaati     = updated.BitisSaati;
            existing.Il             = updated.Il;
            existing.Ilce           = updated.Ilce;
            existing.Mekan          = updated.Mekan;
            existing.Latitude       = updated.Latitude;
            existing.Longitude      = updated.Longitude;
            existing.KatilimciSayisi = updated.KatilimciSayisi;
            existing.Aciklama       = updated.Aciklama;
            existing.KaynakKurum    = updated.KaynakKurum;
            existing.Hassasiyet     = updated.Hassasiyet;
            existing.RiskPuani      = CalculateRisk(existing);

            await _olayRepository.UpdateAsync(existing);
            var isSelfCorrection = isStaffRole && existing.CreatedByUserId == _currentUser.UserId;
            await _notificationService.NotifyOlayRiskAsync(existing, isSelfCorrection: isSelfCorrection);
            return (true, null);
        }

        // ── Silme (yalnızca Başkanlık Yöneticisi) ───────────────────────
        public async Task<bool> DeleteOlayAsync(Guid id)
        {
            var existing = await _olayRepository.GetByIdAsync(id);
            if (existing == null) return false;

            await _olayRepository.DeleteAsync(existing);
            return true;
        }

        // ── Durum değişiklikleri ─────────────────────────────────────────
        public async Task<Olay?> BaslatOlayAsync(Guid olayId)
        {
            var olay = await _olayRepository.GetByIdAsync(olayId);
            if (olay == null) return null;
            olay.Durum = OlayDurum.Gerceklesti;
            olay.GercekBaslangicTarihi = DateTime.UtcNow;
            await _olayRepository.UpdateAsync(olay);
            return olay;
        }

        public async Task<Olay?> BitirOlayAsync(Guid olayId)
        {
            var olay = await _olayRepository.GetByIdAsync(olayId);
            if (olay == null) return null;
            olay.GercekBitisTarihi = DateTime.UtcNow;
            await _olayRepository.UpdateAsync(olay);
            return olay;
        }

        public async Task<Olay?> IptalEtOlayAsync(Guid olayId)
        {
            var olay = await _olayRepository.GetByIdAsync(olayId);
            if (olay == null) return null;
            olay.Durum = OlayDurum.Iptal;
            await _olayRepository.UpdateAsync(olay);
            return olay;
        }

        // ── Risk hesaplama ───────────────────────────────────────────────
        private static double CalculateRisk(Olay olay)
        {
            double risk = 0;
            if (olay.KatilimciSayisi.HasValue && olay.KatilimciSayisi.Value > 1000)
                risk += 10;
            if (olay.Hassasiyet == Hassasiyet.Kritik)
                risk += 20;
            if (olay.OlayTuru == "Yürüyüş" && olay.YuruyusRotasi != null && olay.YuruyusRotasi.Count > 2)
                risk += 15;
            return risk;
        }

        // ── Rota ─────────────────────────────────────────────────────────
        public async Task<YuruyusRota> AddRotaNoktasiAsync(Guid olayId, string noktaAdi, double lat, double lng, int siraNo)
        {
            var rota = new YuruyusRota
            {
                OlayId   = olayId,
                NoktaAdi = noktaAdi,
                Latitude = lat,
                Longitude = lng,
                SiraNo   = siraNo,
                CreatedByUserId = _currentUser.UserId
            };
            return await _rotaRepository.AddAsync(rota);
        }

        public async Task<IReadOnlyList<YuruyusRota>> GetRotaAsync(Guid olayId)
        {
            var all = await _rotaRepository.FindAsync(r => r.OlayId == olayId);
            return all.OrderBy(r => r.SiraNo).ToList();
        }
    }
}
