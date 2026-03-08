using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EGM.Domain.Entities;
using EGM.Domain.Interfaces;

namespace EGM.Application.Services
{
    public class SecimService
    {
        private readonly IRepository<SecimSonucu> _secimRepository;
        private readonly IRepository<Aday> _adayRepository;
        private readonly IRepository<Parti> _partiRepository;
        private readonly IRepository<SecimKaynak> _kaynakRepository;

        public SecimService(
            IRepository<SecimSonucu> secimRepository,
            IRepository<Aday> adayRepository,
            IRepository<Parti> partiRepository,
            IRepository<SecimKaynak> kaynakRepository)
        {
            _secimRepository = secimRepository;
            _adayRepository = adayRepository;
            _partiRepository = partiRepository;
            _kaynakRepository = kaynakRepository;
        }

        // ── Seçim Sonucu CRUD ────────────────────────────────────────────

        public async Task<IReadOnlyList<SecimSonucu>> GetAllAsync()
            => await _secimRepository.ListAllAsync();

        public async Task<SecimSonucu?> GetByIdAsync(int id)
            => await _secimRepository.GetByIdAsync(id);

        public async Task<SecimSonucu> CreateAsync(SecimSonucu sonuc)
            => await _secimRepository.AddAsync(sonuc);

        public async Task<bool> UpdateAsync(int id, SecimSonucu updated)
        {
            var existing = await _secimRepository.GetByIdAsync(id);
            if (existing == null) return false;

            existing.SecimTuru = updated.SecimTuru;
            existing.Tarih = updated.Tarih;
            existing.BolgeTipi = updated.BolgeTipi;
            existing.BolgeId = updated.BolgeId;
            existing.AdayId = updated.AdayId;
            existing.PartiId = updated.PartiId;
            existing.OySayisi = updated.OySayisi;
            existing.OyOrani = updated.OyOrani;
            existing.KaynakId = updated.KaynakId;
            existing.KaynakOnayDurumu = updated.KaynakOnayDurumu;

            await _secimRepository.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _secimRepository.GetByIdAsync(id);
            if (existing == null) return false;

            await _secimRepository.DeleteAsync(existing);
            return true;
        }

        // Seçim türüne göre filtrele (yerel, genel vb.)
        public async Task<IReadOnlyList<SecimSonucu>> GetBySecimTuruAsync(string tur)
        {
            var all = await _secimRepository.ListAllAsync();
            return all.Where(s => s.SecimTuru == tur).ToList();
        }

        // Bölgeye göre sonuçları getir
        public async Task<IReadOnlyList<SecimSonucu>> GetByBolgeAsync(string bolgeTipi, int bolgeId)
        {
            var all = await _secimRepository.ListAllAsync();
            return all.Where(s => s.BolgeTipi == bolgeTipi && s.BolgeId == bolgeId).ToList();
        }

        // Onaylanmış kaynakların sonuçları
        public async Task<IReadOnlyList<SecimSonucu>> GetOnayliSonuclarAsync()
        {
            var all = await _secimRepository.ListAllAsync();
            return all.Where(s => s.KaynakOnayDurumu).ToList();
        }

        // Partiye göre toplam oy sayısı
        public async Task<int> GetToplamOyByPartiAsync(int partiId)
        {
            var all = await _secimRepository.ListAllAsync();
            return all.Where(s => s.PartiId == partiId).Sum(s => s.OySayisi);
        }

        // ── Aday CRUD ───────────────────────────────────────────────────

        public async Task<IReadOnlyList<Aday>> GetAllAdayAsync()
            => await _adayRepository.ListAllAsync();

        public async Task<Aday?> GetAdayByIdAsync(int id)
            => await _adayRepository.GetByIdAsync(id);

        public async Task<Aday> CreateAdayAsync(string adSoyad, string partiAdi)
        {
            var aday = new Aday { AdSoyad = adSoyad, PartiAdi = partiAdi };
            return await _adayRepository.AddAsync(aday);
        }

        public async Task<bool> DeleteAdayAsync(int id)
        {
            var existing = await _adayRepository.GetByIdAsync(id);
            if (existing == null) return false;
            await _adayRepository.DeleteAsync(existing);
            return true;
        }

        // ── Parti CRUD ──────────────────────────────────────────────────

        public async Task<IReadOnlyList<Parti>> GetAllPartiAsync()
            => await _partiRepository.ListAllAsync();

        public async Task<Parti> CreatePartiAsync(string ad)
        {
            var parti = new Parti { Ad = ad };
            return await _partiRepository.AddAsync(parti);
        }

        public async Task<bool> DeletePartiAsync(int id)
        {
            var existing = await _partiRepository.GetByIdAsync(id);
            if (existing == null) return false;
            await _partiRepository.DeleteAsync(existing);
            return true;
        }

        // ── Kaynak CRUD ─────────────────────────────────────────────────

        public async Task<IReadOnlyList<SecimKaynak>> GetAllKaynakAsync()
            => await _kaynakRepository.ListAllAsync();

        public async Task<SecimKaynak> CreateKaynakAsync(string kaynakAdi)
        {
            var kaynak = new SecimKaynak { KaynakAdi = kaynakAdi };
            return await _kaynakRepository.AddAsync(kaynak);
        }

        public async Task<bool> DeleteKaynakAsync(int id)
        {
            var existing = await _kaynakRepository.GetByIdAsync(id);
            if (existing == null) return false;
            await _kaynakRepository.DeleteAsync(existing);
            return true;
        }
    }
}
