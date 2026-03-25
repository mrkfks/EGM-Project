using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EGM.Domain.Entities;
using EGM.Domain.Enums;
using EGM.Domain.Interfaces;

namespace EGM.Application.Services
{
    public class SosyalMedyaOlayService
    {
        private readonly IRepository<SosyalMedyaOlay> _sosyalMedyaRepository;

        public SosyalMedyaOlayService(IRepository<SosyalMedyaOlay> sosyalMedyaRepository)
        {
            _sosyalMedyaRepository = sosyalMedyaRepository;
        }

        // Tüm paylaşımları getir
        public async Task<IReadOnlyList<SosyalMedyaOlay>> GetAllAsync()
            => await _sosyalMedyaRepository.ListAllAsync();

        // ID ile getir
        public async Task<SosyalMedyaOlay?> GetByIdAsync(Guid id)
            => await _sosyalMedyaRepository.GetByIdAsync(id);

        // Olaya göre filtrele
        public async Task<IReadOnlyList<SosyalMedyaOlay>> GetByOlayAsync(Guid olayId)
        {
            var all = await _sosyalMedyaRepository.ListAllAsync();
            return all.Where(s => s.OlayId == olayId).ToList();
        }

        // Platforma göre filtrele (Twitter, Facebook vb.)
        public async Task<IReadOnlyList<SosyalMedyaOlay>> GetByPlatformAsync(string platform)
        {
            var all = await _sosyalMedyaRepository.ListAllAsync();
            return all.Where(s => s.Platform == platform).ToList();
        }

        // Hassasiyete göre filtrele
        public async Task<IReadOnlyList<SosyalMedyaOlay>> GetByHassasiyetAsync(Hassasiyet hassasiyet)
        {
            var all = await _sosyalMedyaRepository.ListAllAsync();
            return all.Where(s => s.Hassasiyet == hassasiyet).ToList();
        }

        // Yüksek signal skoruna sahip paylaşımlar (eşik değer üstü)
        public async Task<IReadOnlyList<SosyalMedyaOlay>> GetHighSignalAsync(double minSkor)
        {
            var all = await _sosyalMedyaRepository.ListAllAsync();
            return all.Where(s => s.SosyalSignalSkoru >= minSkor)
                      .OrderByDescending(s => s.SosyalSignalSkoru)
                      .ToList();
        }

        // Tarih aralığına göre filtrele
        public async Task<IReadOnlyList<SosyalMedyaOlay>> GetByTarihAraligiAsync(DateTime baslangic, DateTime bitis)
        {
            var all = await _sosyalMedyaRepository.ListAllAsync();
            return all.Where(s => s.PaylasimTarihi >= baslangic && s.PaylasimTarihi <= bitis).ToList();
        }

        // Yeni kayıt ekle
        public async Task<SosyalMedyaOlay> CreateAsync(SosyalMedyaOlay kayit)
            => await _sosyalMedyaRepository.AddAsync(kayit);

        // Güncelle
        public async Task<bool> UpdateAsync(Guid id, SosyalMedyaOlay updated)
        {
            var existing = await _sosyalMedyaRepository.GetByIdAsync(id);
            if (existing == null) return false;

            existing.Platform = updated.Platform;
            existing.PaylasimLinki = updated.PaylasimLinki;
            existing.PaylasimTarihi = updated.PaylasimTarihi;
            existing.IcerikOzeti = updated.IcerikOzeti;
            existing.IlgiliKisiKurum = updated.IlgiliKisiKurum;
            existing.Hassasiyet = updated.Hassasiyet;
            existing.SosyalSignalSkoru = updated.SosyalSignalSkoru;
            existing.OlayId = updated.OlayId;

            await _sosyalMedyaRepository.UpdateAsync(existing);
            return true;
        }

        // Sil
        public async Task<bool> DeleteAsync(Guid id)
        {
            var existing = await _sosyalMedyaRepository.GetByIdAsync(id);
            if (existing == null) return false;

            await _sosyalMedyaRepository.DeleteAsync(existing);
            return true;
        }
    }
}
