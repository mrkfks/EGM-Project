using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EGM.Domain.Entities;
using EGM.Domain.Interfaces;

namespace EGM.Application.Services
{
    public class OluService
    {
        private readonly IRepository<Olu> _oluRepository;

        public OluService(IRepository<Olu> oluRepository)
        {
            _oluRepository = oluRepository;
        }

        // Tüm ölü kayıtlarını getir
        public async Task<IReadOnlyList<Olu>> GetAllAsync()
            => await _oluRepository.ListAllAsync();

        // ID ile getir
        public async Task<Olu?> GetByIdAsync(Guid id)
            => await _oluRepository.GetByIdAsync(id);

        // Belirli bir operasyonel faaliyete ait ölü kayıtları
        public async Task<IReadOnlyList<Olu>> GetByOperasyonelFaaliyetAsync(Guid operasyonelFaaliyetId)
        {
            var all = await _oluRepository.ListAllAsync();
            return all.Where(o => o.OperasyonelFaaliyetId == operasyonelFaaliyetId).ToList();
        }

        // Yeni ölü kaydı ekle
        public async Task<Olu> CreateAsync(Olu olu)
            => await _oluRepository.AddAsync(olu);

        // Güncelle
        public async Task<bool> UpdateAsync(Guid id, Olu updated)
        {
            var existing = await _oluRepository.GetByIdAsync(id);
            if (existing == null) return false;

            existing.Ad = updated.Ad;
            existing.Soyad = updated.Soyad;
            existing.TcKimlikNo = updated.TcKimlikNo;
            existing.DogumTarihi = updated.DogumTarihi;
            existing.KatilimciDurumu = updated.KatilimciDurumu;
            existing.OperasyonelFaaliyetId = updated.OperasyonelFaaliyetId;

            await _oluRepository.UpdateAsync(existing);
            return true;
        }

        // Sil
        public async Task<bool> DeleteAsync(Guid id)
        {
            var existing = await _oluRepository.GetByIdAsync(id);
            if (existing == null) return false;

            await _oluRepository.DeleteAsync(existing);
            return true;
        }

        // Katılımcı durumuna göre filtrele (sivil, gösterici vb.)
        public async Task<IReadOnlyList<Olu>> GetByKatilimciDurumuAsync(string durum)
        {
            var all = await _oluRepository.ListAllAsync();
            return all.Where(o => o.KatilimciDurumu == durum).ToList();
        }
    }
}
