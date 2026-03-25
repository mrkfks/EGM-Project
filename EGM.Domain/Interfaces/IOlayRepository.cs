using EGM.Domain.Entities;
using EGM.Domain.Enums;

namespace EGM.Domain.Interfaces
{
    /// <summary>
    /// Olay'a özgü sorgu metotları — navigation property'leri eager load eder (N+1 önleme).
    /// </summary>
    public interface IOlayRepository : IRepository<Olay>
    {
        /// <summary>Organizator ve Konu navigation property'leriyle birlikte getirir.</summary>
        Task<Olay?> GetByIdWithDetailsAsync(Guid id);

        /// <summary>Durum, tarih aralığı ve il filtresiyle sayfalanmış olay listesi döner.</summary>
        Task<(IReadOnlyList<Olay> Items, int TotalCount)> GetFilteredPagedAsync(
            OlayDurum? durum,
            DateTime? tarihBaslangic,
            DateTime? tarihBitis,
            int? cityId,
            int page,
            int pageSize);
    }
}
