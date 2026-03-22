
using EGM.Domain.Entities;
using EGM.Domain.Enums;
using EGM.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

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

        // BBOX ile filtreleme: minLat, minLon, maxLat, maxLon
        public async Task<IReadOnlyList<Olay>> GetByBoundingBoxAsync(double minLat, double minLon, double maxLat, double maxLon)
        {
            var all = await _olayRepository.ListAllAsync();
            return all.Where(o => o.Latitude.HasValue && o.Longitude.HasValue
                                  && o.Latitude.Value >= minLat && o.Latitude.Value <= maxLat
                                  && o.Longitude.Value >= minLon && o.Longitude.Value <= maxLon)
                      .ToList();
        }

        /// <summary>
        /// BBOX içindeki olayları grid hücrelerine göre kümeler.
        /// gridSize: hücre boyutu (derece cinsinden). Zoom arttıkça küçülür.
        /// </summary>
        public async Task<IReadOnlyList<(double Lat, double Lon, int Count)>> GetClusteredAsync(
            double minLat, double minLon, double maxLat, double maxLon, double gridSize)
        {
            var all = await _olayRepository.ListAllAsync();
            var inBbox = all.Where(o => o.Latitude.HasValue && o.Longitude.HasValue
                                        && o.Latitude.Value >= minLat && o.Latitude.Value <= maxLat
                                        && o.Longitude.Value >= minLon && o.Longitude.Value <= maxLon);

            var clusters = inBbox
                .GroupBy(o =>
                {
                    var row = Math.Floor(o.Latitude!.Value / gridSize);
                    var col = Math.Floor(o.Longitude!.Value / gridSize);
                    return (row, col);
                })
                .Select(g =>
                {
                    var (row, col) = g.Key;
                    double centerLat = (row + 0.5) * gridSize;
                    double centerLon = (col + 0.5) * gridSize;
                    return (Lat: centerLat, Lon: centerLon, Count: g.Count());
                })
                .ToList();

            return clusters;
        }
    }
}
