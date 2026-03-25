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
            => await _sosyalMedyaRepository.FindAsync(s => s.OlayId == olayId);

        // Platforma göre filtrele (Twitter, Facebook vb.)
        public async Task<IReadOnlyList<SosyalMedyaOlay>> GetByPlatformAsync(string platform)
            => await _sosyalMedyaRepository.FindAsync(s => s.Platform == platform);

        // Hassasiyete göre filtrele
        public async Task<IReadOnlyList<SosyalMedyaOlay>> GetByHassasiyetAsync(Hassasiyet hassasiyet)
            => await _sosyalMedyaRepository.FindAsync(s => s.Hassasiyet == hassasiyet);

        // Yüksek signal skoruna sahip paylaşımlar (eşik değer üstü)
        public async Task<IReadOnlyList<SosyalMedyaOlay>> GetHighSignalAsync(double minSkor)
            => await _sosyalMedyaRepository.FindAsync(s => s.SosyalSignalSkoru >= minSkor);

        // Tarih aralığına göre filtrele
        public async Task<IReadOnlyList<SosyalMedyaOlay>> GetByTarihAraligiAsync(DateTime baslangic, DateTime bitis)
            => await _sosyalMedyaRepository.FindAsync(s => s.PaylasimTarihi >= baslangic && s.PaylasimTarihi <= bitis);

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
