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

        /// <summary>Belirli bir organizatöre ait olayları tarih filtresiyle getirir.</summary>
        Task<IReadOnlyList<Olay>> GetByOrganizatorAsync(
            Guid organizatorId,
            DateTime? tarihBaslangic,
            DateTime? tarihBitis);

        /// <summary>Belirli bir konuya ait olayları getirir.</summary>
        Task<IReadOnlyList<Olay>> GetByKonuAsync(Guid konuId);

        /// <summary>
        /// Harita sayfası için gelişmiş filtreleme destekli olayları sayfalanmış şekilde döner.
        /// Tüm parametreler optional; null olan parametreler filtre uygulanmaz.
        /// </summary>
        Task<(IReadOnlyList<Olay> Items, int TotalCount)> GetFilteredMapOlaylarAsync(
            DateTime? tarihBaslangic,
            DateTime? tarihBitis,
            Guid? konuId,
            Guid? organizatorId,
            string? olayTuru,
            Guid? gerceklesmeSekliId,
            OlayDurum? durum,
            int? cityId,
            int page,
            int pageSize);

        /// <summary>TakipNo ile olay getirir.</summary>
        Task<Olay?> GetByTakipNoAsync(string takipNo);
    }
}
