using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EGM.Domain.Entities;
using EGM.Domain.Interfaces;

namespace EGM.Application.Services
{
    public class SupheliService
    {
        private readonly IRepository<Supheli> _supheliRepository;

        public SupheliService(IRepository<Supheli> supheliRepository)
        {
            _supheliRepository = supheliRepository;
        }

        // Tüm şüphelileri getir
        public async Task<IReadOnlyList<Supheli>> GetAllAsync()
            => await _supheliRepository.ListAllAsync();

        // ID ile getir
        public async Task<Supheli?> GetByIdAsync(Guid id)
            => await _supheliRepository.GetByIdAsync(id);

        // Operasyonel faaliyete göre filtrele
        public async Task<IReadOnlyList<Supheli>> GetByOperasyonelFaaliyetAsync(Guid operasyonelFaaliyetId)
            => await _supheliRepository.FindAsync(s => s.OperasyonelFaaliyetId == operasyonelFaaliyetId);

        // Gözaltındaki şüphelileri getir
        public async Task<IReadOnlyList<Supheli>> GetGozaltindakileriAsync()
            => await _supheliRepository.FindAsync(s => s.Gozaltinda);

        // Faaliyetteki gözaltı sayısı
        public async Task<int> GetGozaltiSayisiByFaaliyetAsync(Guid operasyonelFaaliyetId)
        {
            var list = await GetByOperasyonelFaaliyetAsync(operasyonelFaaliyetId);
            return list.Count(s => s.Gozaltinda);
        }

        // Yeni şüpheli ekle
        public async Task<Supheli> CreateAsync(Supheli supheli)
            => await _supheliRepository.AddAsync(supheli);

        // Güncelle
        public async Task<bool> UpdateAsync(Guid id, Supheli updated)
        {
            var existing = await _supheliRepository.GetByIdAsync(id);
            if (existing == null) return false;

            existing.Ad = updated.Ad;
            existing.Soyad = updated.Soyad;
            existing.TcKimlikNo = updated.TcKimlikNo;
            existing.DogumTarihi = updated.DogumTarihi;
            existing.Gozaltinda = updated.Gozaltinda;
            existing.OperasyonelFaaliyetId = updated.OperasyonelFaaliyetId;

            await _supheliRepository.UpdateAsync(existing);
            return true;
        }

        // Gözaltı durumunu değiştir
        public async Task<bool> ToggleGozaltiAsync(Guid id)
        {
            var existing = await _supheliRepository.GetByIdAsync(id);
            if (existing == null) return false;

            existing.Gozaltinda = !existing.Gozaltinda;
            await _supheliRepository.UpdateAsync(existing);
            return true;
        }

        // Serbest bırak
        public async Task<bool> SerbetBirakAsync(Guid id)
        {
            var existing = await _supheliRepository.GetByIdAsync(id);
            if (existing == null) return false;

            existing.Gozaltinda = false;
            await _supheliRepository.UpdateAsync(existing);
            return true;
        }

        // Gözaltına al
        public async Task<bool> GozaltiyaAlAsync(Guid id)
        {
            var existing = await _supheliRepository.GetByIdAsync(id);
            if (existing == null) return false;

            existing.Gozaltinda = true;
            await _supheliRepository.UpdateAsync(existing);
            return true;
        }

        // Sil
        public async Task<bool> DeleteAsync(Guid id)
        {
            var existing = await _supheliRepository.GetByIdAsync(id);
            if (existing == null) return false;

            await _supheliRepository.DeleteAsync(existing);
            return true;
        }
    }
}
