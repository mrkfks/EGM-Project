using EGM.Domain.Entities;
using EGM.Domain.Enums;
using EGM.Domain.Interfaces;
using EGM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EGM.Infrastructure.Repositories
{
    public class OlayRepository : EfRepository<Olay>, IOlayRepository
    {
        private readonly EGMDbContext _db;

        public OlayRepository(EGMDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<Olay?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _db.Olaylar
                .Include(o => o.Organizator)
                .Include(o => o.Konu)
                .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);
        }

        public async Task<(IReadOnlyList<Olay> Items, int TotalCount)> GetFilteredPagedAsync(
            OlayDurum? durum,
            DateTime? tarihBaslangic,
            DateTime? tarihBitis,
            int? cityId,
            int page,
            int pageSize)
        {
            var q = _db.Olaylar
                .Include(o => o.Organizator)
                .Include(o => o.Konu)
                .Where(o => !o.IsDeleted);

            if (durum.HasValue)         q = q.Where(o => o.Durum == durum.Value);
            if (tarihBaslangic.HasValue) q = q.Where(o => o.Tarih >= tarihBaslangic.Value);
            if (tarihBitis.HasValue)     q = q.Where(o => o.Tarih <= tarihBitis.Value);
            if (cityId.HasValue)         q = q.Where(o => o.CityId == cityId.Value);

            var total = await q.CountAsync();
            var items = await q
                .OrderByDescending(o => o.Tarih)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<IReadOnlyList<Olay>> GetByOrganizatorAsync(
            Guid organizatorId,
            DateTime? tarihBaslangic,
            DateTime? tarihBitis)
        {
            var q = _db.Olaylar
                .Include(o => o.Organizator)
                .Include(o => o.Konu)
                .Where(o => !o.IsDeleted && o.OrganizatorId == organizatorId);

            if (tarihBaslangic.HasValue) q = q.Where(o => o.Tarih >= tarihBaslangic.Value);
            if (tarihBitis.HasValue)     q = q.Where(o => o.Tarih <= tarihBitis.Value);

            return await q
                .OrderByDescending(o => o.Tarih)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Olay>> GetByKonuAsync(Guid konuId)
        {
            return await _db.Olaylar
                .Include(o => o.Organizator)
                .Include(o => o.Konu)
                .Where(o => !o.IsDeleted && o.KonuId == konuId)
                .OrderByDescending(o => o.Tarih)
                .ToListAsync();
        }

        public async Task<Olay?> GetByTakipNoAsync(string takipNo)
        {
            return await _db.Olaylar
                .Include(o => o.Organizator)
                .Include(o => o.Konu)
                .FirstOrDefaultAsync(o => o.TakipNo == takipNo && !o.IsDeleted);
        }

        /// <summary>
        /// Harita sayfası için gelişmiş filtreleme destekli olayları döner.
        /// Parametreler null ise filtre uygulanmaz.
        /// </summary>
        public async Task<(IReadOnlyList<Olay> Items, int TotalCount)> GetFilteredMapOlaylarAsync(
            DateTime? tarihBaslangic,
            DateTime? tarihBitis,
            Guid? konuId,
            Guid? organizatorId,
            string? olayTuru,
            Guid? gerceklesmeSekliId,
            OlayDurum? durum,
            int? cityId,
            int page,
            int pageSize)
        {
            var q = _db.Olaylar
                .Include(o => o.Organizator)
                .Include(o => o.Konu)
                .Include(o => o.GerceklesmeSekli)
                .Where(o => !o.IsDeleted);

            if (tarihBaslangic.HasValue)
                q = q.Where(o => o.Tarih.Date >= tarihBaslangic.Value.Date);

            if (tarihBitis.HasValue)
                q = q.Where(o => o.Tarih.Date <= tarihBitis.Value.Date);

            if (konuId.HasValue)
                q = q.Where(o => o.KonuId == konuId.Value);

            if (organizatorId.HasValue)
                q = q.Where(o => o.OrganizatorId == organizatorId.Value);

            if (!string.IsNullOrWhiteSpace(olayTuru))
                q = q.Where(o => o.OlayTuru != null && o.OlayTuru.Contains(olayTuru));

            if (gerceklesmeSekliId.HasValue)
                q = q.Where(o => o.GerceklesmeSekliId == gerceklesmeSekliId.Value);

            if (durum.HasValue)
            {
                // Sadece belirli bir durum isteniyorsa filtre uygula
                q = q.Where(o => o.Durum == durum.Value);
            }
            // Eğer durum null ise, hiçbir filtre uygulanmaz ve tüm olaylar gelir

            if (cityId.HasValue)
                q = q.Where(o => o.CityId == cityId.Value);

            var total = await q.CountAsync();
            var items = await q
                .OrderByDescending(o => o.Tarih)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }
    }
}
