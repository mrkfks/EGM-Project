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
    }
}
