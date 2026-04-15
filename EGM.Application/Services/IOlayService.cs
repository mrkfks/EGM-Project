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
        // ── Olay Yönetimi Metotları ──────────────────────────────────
        Task<List<OlayResponseDto>> GetOlaylarAsync(OlayDurum? durum, int sayfaBoyutu);
        Task<List<OlayResponseDto>> GetAllOlaylarAsync();
        Task<OlayResponseDto> CreateOlayAsync(OlayCreateDto createDto);
        Task<(bool Success, string? Error)> UpdateOlayAsync(Guid id, OlayCreateDto updateDto);
        Task<OlayResponseDto?> GetByOlayNoAsync(string olayNo);
        Task<OlayResponseDto?> GetByIdAsync(Guid id);

        // ── Durum Geçiş Metotları ─────────────────────────────────────
        Task<Olay?> BaslatOlayAsync(Guid olayId);
        Task<Olay?> BitirOlayAsync(Guid olayId, EventDetailDto details);
        Task<Olay?> IptalEtOlayAsync(Guid olayId);

        // ── Filtreleme ve Harita Metotları ──────────────────────────
        Task<PagedResult<Olay>> GetAllAsync(
            int page = 1,
            int pageSize = 20,
            OlayDurum? durum = null,
            DateTime? tarihBaslangic = null,
            DateTime? tarihBitis = null);
        Task<PagedResult<Olay>> GetFilteredMapOlaylarAsync(OlayFilterDto filter);
    }
}