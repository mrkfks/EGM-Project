using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EGM.Domain.Entities;
using EGM.Domain.Interfaces;

namespace EGM.Application.Services
{
    public class SehitService
    {
        private readonly IRepository<Sehit> _sehitRepository;

        public SehitService(IRepository<Sehit> sehitRepository)
        {
            _sehitRepository = sehitRepository;
        }

        // Tüm şehit kayıtlarını getir
        public async Task<IReadOnlyList<Sehit>> GetAllAsync()
            => await _sehitRepository.ListAllAsync();

        // ID ile getir
        public async Task<Sehit?> GetByIdAsync(Guid id)
            => await _sehitRepository.GetByIdAsync(id);

        // Belirli bir operasyonel faaliyete ait şehitler
        public async Task<IReadOnlyList<Sehit>> GetByOperasyonelFaaliyetAsync(Guid operasyonelFaaliyetId)
        {
            var all = await _sehitRepository.ListAllAsync();
            return all.Where(s => s.OperasyonelFaaliyetId == operasyonelFaaliyetId).ToList();
        }

        // Göreve göre filtrele (polis, asker vb.)
        public async Task<IReadOnlyList<Sehit>> GetByGorevAsync(string gorev)
        {
            var all = await _sehitRepository.ListAllAsync();
            return all.Where(s => s.Gorev == gorev).ToList();
        }

        // Yeni şehit kaydı ekle
        public async Task<Sehit> CreateAsync(Sehit sehit)
            => await _sehitRepository.AddAsync(sehit);

        // Güncelle
        public async Task<bool> UpdateAsync(Guid id, Sehit updated)
        {
            var existing = await _sehitRepository.GetByIdAsync(id);
            if (existing == null) return false;

            existing.Ad = updated.Ad;
            existing.Soyad = updated.Soyad;
            existing.TcKimlikNo = updated.TcKimlikNo;
            existing.DogumTarihi = updated.DogumTarihi;
            existing.Gorev = updated.Gorev;
            existing.OperasyonelFaaliyetId = updated.OperasyonelFaaliyetId;

            await _sehitRepository.UpdateAsync(existing);
            return true;
        }

        // Sil
        public async Task<bool> DeleteAsync(Guid id)
        {
            var existing = await _sehitRepository.GetByIdAsync(id);
            if (existing == null) return false;

            await _sehitRepository.DeleteAsync(existing);
            return true;
        }

        // Faaliyetteki toplam şehit sayısı
        public async Task<int> GetSayiByFaaliyetAsync(Guid operasyonelFaaliyetId)
        {
            var list = await GetByOperasyonelFaaliyetAsync(operasyonelFaaliyetId);
            return list.Count;
        }
    }
}
