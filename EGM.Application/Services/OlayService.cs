using EGM.Application.DTOs;
using EGM.Application.Helpers;
using EGM.Domain.Constants;
using EGM.Domain.Entities;
using EGM.Domain.Enums;
using EGM.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EGM.Application.Services
{
    public class OlayService : IOlayService
    {
        private readonly IOlayRepository _olayRepository;
        private readonly IRepository<Group> _groupRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IInAppNotificationService _notificationService;
        private readonly ILogger<OlayService> _logger;

        public OlayService(
            IOlayRepository olayRepository,
            IRepository<Group> groupRepository,
            ICurrentUserService currentUser,
            IInAppNotificationService notificationService,
            ILogger<OlayService> logger)
        {
            _olayRepository = olayRepository;
            _groupRepository = groupRepository;
            _currentUser = currentUser;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<List<OlayResponseDto>> GetOlaylarAsync(OlayDurum? durum, int sayfaBoyutu)
        {
            int? cityId = Roles.IsCityScoped(_currentUser.Role) ? _currentUser.CityId : null;
            var (items, _) = await _olayRepository.GetFilteredPagedAsync(durum, null, null, cityId, 1, sayfaBoyutu);
            return items.Select(MapToResponseDto).ToList();
        }

        public async Task<List<OlayResponseDto>> GetAllOlaylarAsync()
        {
            var (items, _) = await _olayRepository.GetFilteredPagedAsync(null, null, null, null, 1, 10000);
            return items.Select(MapToResponseDto).ToList();
        }

        public async Task<OlayResponseDto> CreateOlayAsync(OlayCreateDto createDto)
        {
            var olay = new Olay
            {
                TurId = createDto.TurId,
                SekilId = createDto.SekilId,
                KonuId = createDto.KonuId,
                OrganizatorId = createDto.OrganizatorId,
                BaslangicTarihi = createDto.BaslangicTarihi,
                BitisTarihi = createDto.BitisTarihi,
                Aciklama = createDto.Aciklama,
                Durum = OlayDurum.Planlanan,
                PersonelId = Guid.Parse(_currentUser.UserId),
                CityId = createDto.CityId ?? _currentUser.CityId,
                CreatedByUserId = _currentUser.UserId
            };

            // Konumlar
            olay.Locations = createDto.Locations.Select(l => new Location
            {
                Il = l.Il,
                Ilce = l.Ilce,
                Mahalle = l.Mahalle,
                Mekan = l.Mekan,
                Latitude = l.Latitude,
                Longitude = l.Longitude
            }).ToList();

            // Kaynaklar
            olay.Resources = createDto.Resources.Select(r => new Resource
            {
                Platform = r.Platform,
                KullaniciAdi = r.KullaniciAdi,
                Link = r.Link,
                GorselPath = r.GorselPath,
                Aciklama = r.Aciklama
            }).ToList();

            // Gruplar
            if (createDto.ParticipantGroupIds.Any())
            {
                foreach (var groupId in createDto.ParticipantGroupIds)
                {
                    var group = await _groupRepository.GetByIdAsync(groupId);
                    if (group != null) olay.ParticipantGroups.Add(group);
                }
            }

            // Olay No üret
            int count = (await _olayRepository.FindAsync(o => 
                o.BaslangicTarihi.Date == olay.BaslangicTarihi.Date && 
                o.CityId == olay.CityId)).Count;
            
            olay.OlayNo = TakipNoHelper.GenerateOlayNo(olay.BaslangicTarihi, olay.CityId ?? 0, count + 1);

            await _olayRepository.AddAsync(olay);
            
            try 
            {
                await _notificationService.NotifyOlayRiskAsync(olay);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bildirim gönderilemedi: {OlayNo}", olay.OlayNo);
            }

            return MapToResponseDto(olay);
        }

        public async Task<(bool Success, string? Error)> UpdateOlayAsync(Guid id, OlayCreateDto updateDto)
        {
            var existing = await _olayRepository.GetByIdWithDetailsAsync(id);
            if (existing == null) return (false, "Olay bulunamadı.");

            if (updateDto.Durum == OlayDurum.Gerceklesen && updateDto.Details == null)
                return (false, "Gerçekleşen olaylar için detay bilgisi zorunludur.");

            existing.TurId = updateDto.TurId;
            existing.SekilId = updateDto.SekilId;
            existing.KonuId = updateDto.KonuId;
            existing.OrganizatorId = updateDto.OrganizatorId;
            existing.BaslangicTarihi = updateDto.BaslangicTarihi;
            existing.BitisTarihi = updateDto.BitisTarihi;
            existing.Aciklama = updateDto.Aciklama;
            existing.Durum = updateDto.Durum;

            if (updateDto.Details != null)
            {
                existing.EventDetail = new EventDetail
                {
                    Hassasiyet = updateDto.Details.Hassasiyet,
                    KatilimciSayisi = updateDto.Details.KatilimciSayisi,
                    SupheliSayisi = updateDto.Details.SupheliSayisi,
                    GozaltiSayisi = updateDto.Details.GozaltiSayisi,
                    SehitSayisi = updateDto.Details.SehitSayisi,
                    OluSayisi = updateDto.Details.OluSayisi
                };
            }

            await _olayRepository.UpdateAsync(existing);
            return (true, null);
        }

        public async Task<OlayResponseDto?> GetByOlayNoAsync(string olayNo)
        {
            var olay = await _olayRepository.GetByOlayNoAsync(olayNo);
            return olay != null ? MapToResponseDto(olay) : null;
        }

        public async Task<OlayResponseDto?> GetByTakipNoAsync(string takipNo) => await GetByOlayNoAsync(takipNo);

        public async Task<OlayResponseDto?> GetByIdAsync(Guid id)
        {
            var olay = await _olayRepository.GetByIdWithDetailsAsync(id);
            return olay != null ? MapToResponseDto(olay) : null;
        }

        public async Task<Olay?> BaslatOlayAsync(Guid olayId)
        {
            var olay = await _olayRepository.GetByIdAsync(olayId);
            if (olay != null)
            {
                olay.Durum = OlayDurum.DevamEden;
                await _olayRepository.UpdateAsync(olay);
            }
            return olay;
        }

        public async Task<Olay?> BitirOlayAsync(Guid olayId, EventDetailDto details)
        {
            var olay = await _olayRepository.GetByIdAsync(olayId);
            if (olay != null)
            {
                olay.Durum = OlayDurum.Gerceklesen;
                olay.EventDetail = new EventDetail
                {
                    Hassasiyet = details.Hassasiyet,
                    KatilimciSayisi = details.KatilimciSayisi,
                    SupheliSayisi = details.SupheliSayisi,
                    GozaltiSayisi = details.GozaltiSayisi,
                    SehitSayisi = details.SehitSayisi,
                    OluSayisi = details.OluSayisi
                };
                await _olayRepository.UpdateAsync(olay);
            }
            return olay;
        }

        public async Task<Olay?> IptalEtOlayAsync(Guid olayId)
        {
            var olay = await _olayRepository.GetByIdAsync(olayId);
            if (olay != null)
            {
                olay.Durum = OlayDurum.Iptal;
                await _olayRepository.UpdateAsync(olay);
            }
            return olay;
        }

        public async Task<PagedResult<Olay>> GetAllAsync(int page, int pageSize, OlayDurum? durum, DateTime? tarihBaslangic, DateTime? tarihBitis)
        {
            int? cityId = Roles.IsCityScoped(_currentUser.Role) ? _currentUser.CityId : null;
            var (items, total) = await _olayRepository.GetFilteredPagedAsync(durum, tarihBaslangic, tarihBitis, cityId, page, pageSize);
            return new PagedResult<Olay> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
        }

        public async Task<PagedResult<Olay>> GetFilteredMapOlaylarAsync(OlayFilterDto filter)
        {
            int? cityId = Roles.IsCityScoped(_currentUser.Role) ? _currentUser.CityId : null;
            var (items, total) = await _olayRepository.GetFilteredMapOlaylarAsync(
                filter.TarihBaslangic, filter.TarihBitis, filter.KonuId, filter.OrganizatorId, 
                filter.OlayTuru, filter.GerceklesmeSekliId, filter.Durum, cityId, filter.Page, filter.PageSize);
            return new PagedResult<Olay> { Items = items, TotalCount = total, Page = filter.Page, PageSize = filter.PageSize };
        }

        private OlayResponseDto MapToResponseDto(Olay o) => new OlayResponseDto
        {
            Id = o.Id,
            OlayNo = o.OlayNo,
            Durum = o.Durum,
            BaslangicTarihi = o.BaslangicTarihi,
            BitisTarihi = o.BitisTarihi,
            TurId = o.TurId,
            TurAd = o.Tur?.Name,
            SekilId = o.SekilId,
            SekilAd = o.Sekil?.Name,
            KonuId = o.KonuId,
            KonuAd = o.Konu?.Ad,
            OrganizatorId = o.OrganizatorId,
            OrganizatorAd = o.Organizator?.Ad,
            Aciklama = o.Aciklama,
            PersonelId = o.PersonelId,
            CityId = o.CityId,
            Locations = o.Locations.Select(l => new LocationDto
            {
                Il = l.Il, Ilce = l.Ilce, Mahalle = l.Mahalle, Mekan = l.Mekan, Latitude = l.Latitude, Longitude = l.Longitude
            }).ToList(),
            Resources = o.Resources.Select(r => new ResourceDto
            {
                Platform = r.Platform, KullaniciAdi = r.KullaniciAdi, Link = r.Link, GorselPath = r.GorselPath, Aciklama = r.Aciklama
            }).ToList(),
            Details = o.EventDetail != null ? new EventDetailDto
            {
                Hassasiyet = o.EventDetail.Hassasiyet,
                KatilimciSayisi = o.EventDetail.KatilimciSayisi,
                SupheliSayisi = o.EventDetail.SupheliSayisi,
                GozaltiSayisi = o.EventDetail.GozaltiSayisi,
                SehitSayisi = o.EventDetail.SehitSayisi,
                OluSayisi = o.EventDetail.OluSayisi
            } : null,
            ParticipantGroups = o.ParticipantGroups.Select(g => g.Name).ToList()
        };
    }
}

