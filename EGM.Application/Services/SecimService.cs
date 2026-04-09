using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EGM.Application.Helpers;
using EGM.Domain.Entities;
using EGM.Domain.Interfaces;

namespace EGM.Application.Services
{
    public class SecimService
    {
        private readonly IRepository<SandikOlay> _sandikOlayRepository;

        public SecimService(IRepository<SandikOlay> sandikOlayRepository)
        {
            _sandikOlayRepository = sandikOlayRepository;
        }

        // â”€â”€ Sandik Olay CRUD â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        public async Task<IReadOnlyList<SandikOlay>> GetAllSandikOlayAsync()
            => await _sandikOlayRepository.ListAllAsync();

        public async Task<SandikOlay?> GetSandikOlayByIdAsync(Guid id)
            => await _sandikOlayRepository.GetByIdAsync(id);

        public async Task<SandikOlay> CreateSandikOlayAsync(SandikOlay kayit)
        {
            // TakipNo üret: SC-YYYYMMDDPP-SSS
            var tarih        = kayit.Tarih.Date;
            var tarihBitis   = tarih.AddDays(1);
            var ilAdi        = kayit.Il ?? string.Empty;
            var plakaKodu    = IlPlakaHelper.GetPlaka(ilAdi);
            var mevcutSayisi = (await _sandikOlayRepository.FindAsync(
                s => s.Tarih >= tarih && s.Tarih < tarihBitis && s.Il == ilAdi)).Count;
            kayit.TakipNo = TakipNoHelper.Generate(TakipNoHelper.SecimOlay, kayit.Tarih, plakaKodu, mevcutSayisi + 1);
            return await _sandikOlayRepository.AddAsync(kayit);
        }

        public async Task<bool> UpdateSandikOlayAsync(Guid id, SandikOlay updated)
        {
            var existing = await _sandikOlayRepository.GetByIdAsync(id);
            if (existing == null) return false;
            existing.MusahitAdi    = updated.MusahitAdi;
            existing.Il            = updated.Il;
            existing.Ilce          = updated.Ilce;
            existing.Mahalle       = updated.Mahalle;
            existing.Okul          = updated.Okul;
            existing.Konu          = updated.Konu;
            existing.SandikNo      = updated.SandikNo;
            existing.OlayKategorisi= updated.OlayKategorisi;
            existing.OlaySaati     = updated.OlaySaati;
            existing.Aciklama      = updated.Aciklama;
            existing.Tarih         = updated.Tarih;
            existing.KatilimciSayisi = updated.KatilimciSayisi;
            existing.SehitSayisi   = updated.SehitSayisi;
            existing.OluSayisi     = updated.OluSayisi;
            existing.GozaltiSayisi = updated.GozaltiSayisi;
            await _sandikOlayRepository.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> DeleteSandikOlayAsync(Guid id)
        {
            var existing = await _sandikOlayRepository.GetByIdAsync(id);
            if (existing == null) return false;
            await _sandikOlayRepository.DeleteAsync(existing);
            return true;
        }
    }
}
