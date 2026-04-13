using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EGM.Application.DTOs;
using EGM.Domain.Entities;
using EGM.Domain.Enums;

namespace EGM.Application.Services
{
    public interface IOlayService
    {
        // ── OlayController metotları ──────────────────────────────────
        Task<List<OlayDto>> GetOlaylarAsync(OlayDurum? durum, int sayfaBoyutu);
        Task<List<OlayDto>> GetAllOlaylarAsync();
        Task<OlayDto> CreateOlayAsync(OlayDto olayDto);
        Task<(bool Success, string? Error)> UpdateOlayAsync(Guid id, OlayDto olayDto);
        Task<OlayDto?> GetByTakipNoAsync(string takipNo);

        // ── GeoController / Harita metotları ──────────────────────────
        Task<PagedResult<Olay>> GetAllAsync(
            int page = 1,
            int pageSize = 20,
            OlayDurum? durum = null,
            DateTime? tarihBaslangic = null,
            DateTime? tarihBitis = null);
        Task<PagedResult<Olay>> GetFilteredMapOlaylarAsync(OlayFilterDto filter);
    }
}