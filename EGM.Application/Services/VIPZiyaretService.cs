using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EGM.Domain.Entities;
using EGM.Domain.Enums;
using EGM.Domain.Interfaces;

namespace EGM.Application.Services
{
    public class VIPZiyaretService
    {
        private readonly IRepository<VIPZiyaret> _vipRepository;
        private readonly IRepository<GuvenlikPlani> _guvenlikRepository;
        private readonly IRepository<Ekip> _ekipRepository;

        public VIPZiyaretService(
            IRepository<VIPZiyaret> vipRepository,
            IRepository<GuvenlikPlani> guvenlikRepository,
            IRepository<Ekip> ekipRepository)
        {
            _vipRepository = vipRepository;
            _guvenlikRepository = guvenlikRepository;
            _ekipRepository = ekipRepository;
        }

        // ── VIP Ziyaret CRUD ─────────────────────────────────────────────

        public async Task<IReadOnlyList<VIPZiyaret>> GetAllAsync()
            => await _vipRepository.ListAllAsync();

        public async Task<VIPZiyaret?> GetByIdAsync(Guid id)
            => await _vipRepository.GetByIdAsync(id);

        public async Task<VIPZiyaret> CreateAsync(VIPZiyaret ziyaret)
            => await _vipRepository.AddAsync(ziyaret);

        public async Task<bool> UpdateAsync(Guid id, VIPZiyaret updated)
        {
            var existing = await _vipRepository.GetByIdAsync(id);
            if (existing == null) return false;

            existing.ZiyaretEdenAdSoyad = updated.ZiyaretEdenAdSoyad;
            existing.Unvan = updated.Unvan;
            existing.BaslangicTarihi = updated.BaslangicTarihi;
            existing.BitisTarihi = updated.BitisTarihi;
            existing.Il = updated.Il;
            existing.Mekan = updated.Mekan;
            existing.Hassasiyet = updated.Hassasiyet;
            existing.GuvenlikSeviyesi = updated.GuvenlikSeviyesi;
            existing.GozlemNoktalari = updated.GozlemNoktalari;
            existing.ZiyaretDurumu = updated.ZiyaretDurumu;

            await _vipRepository.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var existing = await _vipRepository.GetByIdAsync(id);
            if (existing == null) return false;

            await _vipRepository.DeleteAsync(existing);
            return true;
        }

        // Hassasiyet seviyesine göre filtrele
        public async Task<IReadOnlyList<VIPZiyaret>> GetByHassasiyetAsync(Hassasiyet hassasiyet)
            => await _vipRepository.FindAsync(v => v.Hassasiyet == hassasiyet);

        // İle göre filtrele
        public async Task<IReadOnlyList<VIPZiyaret>> GetByIlAsync(string il)
            => await _vipRepository.FindAsync(v => v.Il == il);

        // Aktif ziyaretler (şu an devam eden)
        public async Task<IReadOnlyList<VIPZiyaret>> GetAktifZiyaretlerAsync()
        {
            var now = DateTime.UtcNow;
            return await _vipRepository.FindAsync(v => v.BaslangicTarihi <= now && v.BitisTarihi >= now);
        }

        // Tarih aralığına göre filtrele
        public async Task<IReadOnlyList<VIPZiyaret>> GetByTarihAraligiAsync(DateTime baslangic, DateTime bitis)
            => await _vipRepository.FindAsync(v => v.BaslangicTarihi >= baslangic && v.BitisTarihi <= bitis);

        // ── Güvenlik Planı CRUD ──────────────────────────────────────────

        public async Task<IReadOnlyList<GuvenlikPlani>> GetAllGuvenlikPlaniAsync()
            => await _guvenlikRepository.ListAllAsync();

        public async Task<GuvenlikPlani?> GetGuvenlikPlaniByIdAsync(Guid id)
            => await _guvenlikRepository.GetByIdAsync(id);

        public async Task<GuvenlikPlani> CreateGuvenlikPlaniAsync(Guid vipZiyaretId, string ad, string? aciklama)
        {
            var plan = new GuvenlikPlani { VIPZiyaretId = vipZiyaretId, Ad = ad, Aciklama = aciklama };
            return await _guvenlikRepository.AddAsync(plan);
        }

        public async Task<bool> UpdateGuvenlikPlaniAsync(Guid id, string ad, string? aciklama)
        {
            var existing = await _guvenlikRepository.GetByIdAsync(id);
            if (existing == null) return false;

            existing.Ad = ad;
            existing.Aciklama = aciklama;
            await _guvenlikRepository.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> DeleteGuvenlikPlaniAsync(Guid id)
        {
            var existing = await _guvenlikRepository.GetByIdAsync(id);
            if (existing == null) return false;

            await _guvenlikRepository.DeleteAsync(existing);
            return true;
        }

        // ── Ekip CRUD ────────────────────────────────────────────────────

        public async Task<IReadOnlyList<Ekip>> GetAllEkipAsync()
            => await _ekipRepository.ListAllAsync();

        public async Task<Ekip?> GetEkipByIdAsync(Guid id)
            => await _ekipRepository.GetByIdAsync(id);

        public async Task<Ekip> CreateEkipAsync(Guid vipZiyaretId, string ad)
        {
            var ekip = new Ekip { VIPZiyaretId = vipZiyaretId, Ad = ad };
            return await _ekipRepository.AddAsync(ekip);
        }

        public async Task<bool> UpdateEkipAsync(Guid id, string ad)
        {
            var existing = await _ekipRepository.GetByIdAsync(id);
            if (existing == null) return false;

            existing.Ad = ad;
            await _ekipRepository.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> DeleteEkipAsync(Guid id)
        {
            var existing = await _ekipRepository.GetByIdAsync(id);
            if (existing == null) return false;

            await _ekipRepository.DeleteAsync(existing);
            return true;
        }
    }
}
