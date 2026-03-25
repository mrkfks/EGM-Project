using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EGM.Domain.Entities;
using EGM.Domain.Interfaces;

namespace EGM.Application.Services
{
    public class OperasyonelFaaliyetService
    {
        private readonly IRepository<OperasyonelFaaliyet> _faaliyetRepository;
        private readonly IRepository<KatilimciGrup> _grupRepository;

        public OperasyonelFaaliyetService(
            IRepository<OperasyonelFaaliyet> faaliyetRepository,
            IRepository<KatilimciGrup> grupRepository)
        {
            _faaliyetRepository = faaliyetRepository;
            _grupRepository = grupRepository;
        }

        // Tüm faaliyetleri getir
        public async Task<IReadOnlyList<OperasyonelFaaliyet>> GetAllAsync()
            => await _faaliyetRepository.ListAllAsync();

        // ID ile getir
        public async Task<OperasyonelFaaliyet?> GetByIdAsync(Guid id)
            => await _faaliyetRepository.GetByIdAsync(id);

        // Olaya göre faaliyetleri getir
        public async Task<IReadOnlyList<OperasyonelFaaliyet>> GetByOlayAsync(Guid olayId)
            => await _faaliyetRepository.FindAsync(f => f.OlayId == olayId);

        // Yeni faaliyet oluştur
        public async Task<OperasyonelFaaliyet> CreateAsync(OperasyonelFaaliyet faaliyet)
            => await _faaliyetRepository.AddAsync(faaliyet);

        // Güncelle
        public async Task<bool> UpdateAsync(Guid id, OperasyonelFaaliyet updated)
        {
            var existing = await _faaliyetRepository.GetByIdAsync(id);
            if (existing == null) return false;

            existing.Aciklama = updated.Aciklama;
            existing.OlayId = updated.OlayId;

            await _faaliyetRepository.UpdateAsync(existing);
            return true;
        }

        // Sil
        public async Task<bool> DeleteAsync(Guid id)
        {
            var existing = await _faaliyetRepository.GetByIdAsync(id);
            if (existing == null) return false;

            await _faaliyetRepository.DeleteAsync(existing);
            return true;
        }

        // Katılımcı grubu ekle
        public async Task<KatilimciGrup> AddKatilimciGrupAsync(Guid faaliyetId, string grupAdi, int katilimciSayisi)
        {
            var grup = new KatilimciGrup
            {
                OperasyonelFaaliyetId = faaliyetId,
                GrupAdi = grupAdi,
                GrupKatilimciSayisi = katilimciSayisi
            };
            return await _grupRepository.AddAsync(grup);
        }

        // Faaliyete ait katılımcı gruplarını getir
        public async Task<IReadOnlyList<KatilimciGrup>> GetGruplarAsync(Guid faaliyetId)
            => await _grupRepository.FindAsync(g => g.OperasyonelFaaliyetId == faaliyetId);

        // Toplam katılımcı sayısını hesapla
        public async Task<int> GetToplamKatilimciSayisiAsync(Guid faaliyetId)
        {
            var gruplar = await GetGruplarAsync(faaliyetId);
            return gruplar.Sum(g => g.GrupKatilimciSayisi ?? 0);
        }
    }
}
