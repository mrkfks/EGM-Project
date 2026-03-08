
using EGM.Domain.Entities;
using EGM.Domain.Enums;
using EGM.Domain.Interfaces;

namespace EGM.Application.Services
{
    public class OlayService
    {
        private readonly IRepository<Olay> _olayRepository;
        private readonly IRepository<YuruyusRota> _rotaRepository;

        public OlayService(IRepository<Olay> olayRepository, IRepository<YuruyusRota> rotaRepository)
        {
            _olayRepository = olayRepository;
            _rotaRepository = rotaRepository;
        }

        // ✅ Tüm olayları listeleme
        public async Task<IReadOnlyList<Olay>> GetAllAsync()
            => await _olayRepository.ListAllAsync();

        // ✅ ID ile getir
        public async Task<Olay?> GetByIdAsync(int id)
            => await _olayRepository.GetByIdAsync(id);

        // ✅ Olay oluşturma
        public async Task<Olay> CreateOlayAsync(Olay olay)
        {
            olay.Durum = OlayDurum.Planlandi;
            olay.RiskPuani = CalculateRisk(olay);
            return await _olayRepository.AddAsync(olay);
        }

        // ✅ Olay güncelleme
        public async Task<bool> UpdateOlayAsync(int id, Olay updated)
        {
            var existing = await _olayRepository.GetByIdAsync(id);
            if (existing == null) return false;

            existing.Baslik = updated.Baslik;
            existing.OlayTuru = updated.OlayTuru;
            existing.OrganizatorId = updated.OrganizatorId;
            existing.KonuId = updated.KonuId;
            existing.Tarih = updated.Tarih;
            existing.BaslangicSaati = updated.BaslangicSaati;
            existing.BitisSaati = updated.BitisSaati;
            existing.Il = updated.Il;
            existing.Ilce = updated.Ilce;
            existing.Mekan = updated.Mekan;
            existing.Latitude = updated.Latitude;
            existing.Longitude = updated.Longitude;
            existing.KatilimciSayisi = updated.KatilimciSayisi;
            existing.Aciklama = updated.Aciklama;
            existing.KaynakKurum = updated.KaynakKurum;
            existing.Hassasiyet = updated.Hassasiyet;
            existing.RiskPuani = CalculateRisk(existing);

            await _olayRepository.UpdateAsync(existing);
            return true;
        }

        // ✅ Olay silme
        public async Task<bool> DeleteOlayAsync(int id)
        {
            var existing = await _olayRepository.GetByIdAsync(id);
            if (existing == null) return false;

            await _olayRepository.DeleteAsync(existing);
            return true;
        }

        // ✅ Olay başlatma
        public async Task<Olay?> BaslatOlayAsync(int olayId)
        {
            var olay = await _olayRepository.GetByIdAsync(olayId);
            if (olay == null) return null;

            olay.Durum = OlayDurum.Gerceklesti;
            olay.GercekBaslangicTarihi = DateTime.UtcNow;

            await _olayRepository.UpdateAsync(olay);
            return olay;
        }

        // ✅ Olay bitirme
        public async Task<Olay?> BitirOlayAsync(int olayId)
        {
            var olay = await _olayRepository.GetByIdAsync(olayId);
            if (olay == null) return null;

            olay.GercekBitisTarihi = DateTime.UtcNow;
            await _olayRepository.UpdateAsync(olay);
            return olay;
        }

        // ✅ Olay iptal etme
        public async Task<Olay?> IptalEtOlayAsync(int olayId)
        {
            var olay = await _olayRepository.GetByIdAsync(olayId);
            if (olay == null) return null;

            olay.Durum = OlayDurum.Iptal;
            await _olayRepository.UpdateAsync(olay);
            return olay;
        }

        // ✅ Risk puanı hesaplama
        private double CalculateRisk(Olay olay)
        {
            double risk = 0;

            if (olay.KatilimciSayisi.HasValue && olay.KatilimciSayisi.Value > 1000)
                risk += 10;

            if (olay.Hassasiyet == Hassasiyet.Kritik)
                risk += 20;

            if (olay.OlayTuru == "Yürüyüş" && olay.YuruyusRotasi != null && olay.YuruyusRotasi.Count > 2)
                risk += 15; // uzun rota daha riskli olabilir

            return risk;
        }

        // ✅ Yürüyüş rotası ekleme
        public async Task<YuruyusRota> AddRotaNoktasiAsync(int olayId, string noktaAdi, double lat, double lng, int siraNo)
        {
            var rota = new YuruyusRota
            {
                OlayId = olayId,
                NoktaAdi = noktaAdi,
                Latitude = lat,
                Longitude = lng,
                SiraNo = siraNo
            };

            return await _rotaRepository.AddAsync(rota);
        }

        // ✅ Yürüyüş rotasını listeleme
        public async Task<IReadOnlyList<YuruyusRota>> GetRotaAsync(int olayId)
        {
            var all = await _rotaRepository.ListAllAsync();
            return all.Where(r => r.OlayId == olayId).OrderBy(r => r.SiraNo).ToList();
        }
    }
}
