using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EGM.Domain.Entities;
using EGM.Domain.Interfaces;

namespace EGM.Application.Services
{
    public class OrganizatorService
    {
        private readonly IRepository<Organizator> _organizatorRepository;
        private readonly IRepository<KategoriOrganizator> _kategoriRepository;
        private readonly IRepository<Konu> _konuRepository;

        public OrganizatorService(
            IRepository<Organizator> organizatorRepository,
            IRepository<KategoriOrganizator> kategoriRepository,
            IRepository<Konu> konuRepository)
        {
            _organizatorRepository = organizatorRepository;
            _kategoriRepository = kategoriRepository;
            _konuRepository = konuRepository;
        }

        // ── Organizatör CRUD ──────────────────────────────────────────────

        public async Task<IReadOnlyList<Organizator>> GetAllAsync()
            => await _organizatorRepository.ListAllAsync();

        public async Task<Organizator?> GetByIdAsync(Guid id)
            => await _organizatorRepository.GetByIdAsync(id);

        public async Task<Organizator> CreateAsync(Organizator organizator)
            => await _organizatorRepository.AddAsync(organizator);

        public async Task<bool> UpdateAsync(Guid id, Organizator updated)
        {
            var existing = await _organizatorRepository.GetByIdAsync(id);
            if (existing == null) return false;

            existing.Ad = updated.Ad;
            existing.KurulusTarihi = updated.KurulusTarihi;
            existing.FaaliyetAlani = updated.FaaliyetAlani;
            existing.Iletisim = updated.Iletisim;
            existing.Tur = updated.Tur;
            existing.Aciklama = updated.Aciklama;
            existing.UstKurulusId = updated.UstKurulusId;

            await _organizatorRepository.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var existing = await _organizatorRepository.GetByIdAsync(id);
            if (existing == null) return false;

            await _organizatorRepository.DeleteAsync(existing);
            return true;
        }

        // Faaliyet alanına göre filtrele
        public async Task<IReadOnlyList<Organizator>> GetByFaaliyetAlaniAsync(string alan)
        {
            var all = await _organizatorRepository.ListAllAsync();
            return all.Where(o => o.FaaliyetAlani == alan).ToList();
        }

        // ── Kategori Organizatör CRUD ────────────────────────────────────

        public async Task<IReadOnlyList<KategoriOrganizator>> GetAllKategoriAsync()
            => await _kategoriRepository.ListAllAsync();

        public async Task<KategoriOrganizator?> GetKategoriByIdAsync(Guid id)
            => await _kategoriRepository.GetByIdAsync(id);

        public async Task<KategoriOrganizator> CreateKategoriAsync(string ad)
        {
            var kategori = new KategoriOrganizator { Ad = ad };
            return await _kategoriRepository.AddAsync(kategori);
        }

        public async Task<bool> DeleteKategoriAsync(Guid id)
        {
            var existing = await _kategoriRepository.GetByIdAsync(id);
            if (existing == null) return false;

            await _kategoriRepository.DeleteAsync(existing);
            return true;
        }

        // ── Konu CRUD ────────────────────────────────────────────────────

        public async Task<IReadOnlyList<Konu>> GetAllKonuAsync()
            => await _konuRepository.ListAllAsync();

        public async Task<Konu?> GetKonuByIdAsync(Guid id)
            => await _konuRepository.GetByIdAsync(id);

        public async Task<Konu> CreateKonuAsync(string ad, string? aciklama, string? tur, Guid? ustKonuId)
        {
            var konu = new Konu { Ad = ad, Aciklama = aciklama, Tur = tur, UstKonuId = ustKonuId };
            return await _konuRepository.AddAsync(konu);
        }

        public async Task<bool> UpdateKonuAsync(Guid id, string ad, string? aciklama, string? tur, Guid? ustKonuId)
        {
            var existing = await _konuRepository.GetByIdAsync(id);
            if (existing == null) return false;

            existing.Ad = ad;
            existing.Aciklama = aciklama;
            existing.Tur = tur;
            existing.UstKonuId = ustKonuId;
            await _konuRepository.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> DeleteKonuAsync(Guid id)
        {
            var existing = await _konuRepository.GetByIdAsync(id);
            if (existing == null) return false;

            await _konuRepository.DeleteAsync(existing);
            return true;
        }
    }
}
