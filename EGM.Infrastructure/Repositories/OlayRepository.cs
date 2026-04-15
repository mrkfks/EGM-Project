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
                .Include(o => o.Tur)
                .Include(o => o.Sekil)
                .Include(o => o.Locations)
                .Include(o => o.Resources)
                .Include(o => o.EventDetail)
                .Include(o => o.ParticipantGroups)
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
                .Include(o => o.Locations)
                .Where(o => !o.IsDeleted);

            if (durum.HasValue)         q = q.Where(o => o.Durum == durum.Value);
            if (tarihBaslangic.HasValue) q = q.Where(o => o.BaslangicTarihi >= tarihBaslangic.Value);
            if (tarihBitis.HasValue)     q = q.Where(o => o.BaslangicTarihi <= tarihBitis.Value);
            if (cityId.HasValue)         q = q.Where(o => o.CityId == cityId.Value);

            var total = await q.CountAsync();
            var items = await q
                .OrderByDescending(o => o.BaslangicTarihi)
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
                .Where(o => !o.IsDeleted && o.OrganizatorId == organizatorId);

            if (tarihBaslangic.HasValue) q = q.Where(o => o.BaslangicTarihi >= tarihBaslangic.Value);
            if (tarihBitis.HasValue)     q = q.Where(o => o.BaslangicTarihi <= tarihBitis.Value);

            return await q
                .OrderByDescending(o => o.BaslangicTarihi)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Olay>> GetByKonuAsync(Guid konuId)
        {
            return await _db.Olaylar
                .Include(o => o.Konu)
                .Where(o => !o.IsDeleted && o.KonuId == konuId)
                .OrderByDescending(o => o.BaslangicTarihi)
                .ToListAsync();
        }

        public async Task<Olay?> GetByOlayNoAsync(string olayNo)
        {
            return await _db.Olaylar
                .Include(o => o.Organizator)
                .Include(o => o.Konu)
                .Include(o => o.Locations)
                .Include(o => o.Resources)
                .Include(o => o.EventDetail)
                .FirstOrDefaultAsync(o => o.OlayNo == olayNo && !o.IsDeleted);
        }

        public async Task<Olay?> GetByTakipNoAsync(string takipNo) => await GetByOlayNoAsync(takipNo);

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
                .Include(o => o.Locations)
                .Where(o => !o.IsDeleted);

            if (tarihBaslangic.HasValue)
                q = q.Where(o => o.BaslangicTarihi.Date >= tarihBaslangic.Value.Date);

            if (tarihBitis.HasValue)
                q = q.Where(o => o.BaslangicTarihi.Date <= tarihBitis.Value.Date);

            if (konuId.HasValue)
                q = q.Where(o => o.KonuId == konuId.Value);

            if (organizatorId.HasValue)
                q = q.Where(o => o.OrganizatorId == organizatorId.Value);

            if (gerceklesmeSekliId.HasValue)
                q = q.Where(o => o.SekilId == gerceklesmeSekliId.Value);

            if (durum.HasValue)
                q = q.Where(o => o.Durum == durum.Value);

            if (cityId.HasValue)
                q = q.Where(o => o.CityId == cityId.Value);

            var total = await q.CountAsync();
            var items = await q
                .OrderByDescending(o => o.BaslangicTarihi)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }
    }
}
