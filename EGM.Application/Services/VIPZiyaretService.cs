using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EGM.Application.Helpers;
using EGM.Domain.Entities;
using EGM.Domain.Enums;
using EGM.Domain.Interfaces;

namespace EGM.Application.Services
{
    public class VIPZiyaretService
    {
        private readonly IRepository<VIPZiyaret> _vipRepository;

        public VIPZiyaretService(IRepository<VIPZiyaret> vipRepository)
        {
            _vipRepository = vipRepository;
        }

        // â”€â”€ VIP Ziyaret CRUD â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        public async Task<IReadOnlyList<VIPZiyaret>> GetAllAsync()
            => await _vipRepository.ListAllAsync();

        public async Task<VIPZiyaret?> GetByIdAsync(Guid id)
            => await _vipRepository.GetByIdAsync(id);

        public async Task<VIPZiyaret> CreateAsync(VIPZiyaret ziyaret)
        {
            // TakipNo üret: VZ-YYYYMMDDPP-SSS
            var tarih        = ziyaret.BaslangicTarihi.Date;
            var tarihBitis   = tarih.AddDays(1);
            var ilAdi        = ziyaret.Il ?? string.Empty;
            var plakaKodu    = IlPlakaHelper.GetPlaka(ilAdi);
            var mevcutSayisi = (await _vipRepository.FindAsync(
                v => v.BaslangicTarihi >= tarih && v.BaslangicTarihi < tarihBitis && v.Il == ilAdi)).Count;
            ziyaret.TakipNo = TakipNoHelper.Generate(TakipNoHelper.VipZiyaret, ziyaret.BaslangicTarihi, plakaKodu, mevcutSayisi + 1);
            return await _vipRepository.AddAsync(ziyaret);
        }

        public async Task<VIPZiyaret?> UpdateAsync(Guid id, VIPZiyaret updated)
        {
            var existing = await _vipRepository.GetByIdAsync(id);
            if (existing == null) return null;

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
            return existing;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var existing = await _vipRepository.GetByIdAsync(id);
            if (existing == null) return false;

            await _vipRepository.DeleteAsync(existing);
            return true;
        }

        public async Task<IReadOnlyList<VIPZiyaret>> GetByHassasiyetAsync(Hassasiyet hassasiyet)
            => await _vipRepository.FindAsync(v => v.Hassasiyet == hassasiyet);

        public async Task<IReadOnlyList<VIPZiyaret>> GetByIlAsync(string il)
            => await _vipRepository.FindAsync(v => v.Il == il);

        public async Task<IReadOnlyList<VIPZiyaret>> GetAktifZiyaretlerAsync()
        {
            var now = DateTime.UtcNow;
            return await _vipRepository.FindAsync(v => v.BaslangicTarihi <= now && v.BitisTarihi >= now);
        }

        public async Task<IReadOnlyList<VIPZiyaret>> GetByTarihAraligiAsync(DateTime baslangic, DateTime bitis)
            => await _vipRepository.FindAsync(v => v.BaslangicTarihi >= baslangic && v.BitisTarihi <= bitis);
    }
}
