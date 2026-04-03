using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EGM.Application.DTOs;
using EGM.Domain.Entities;
using EGM.Domain.Enums;
using EGM.Domain.Interfaces;

namespace EGM.Application.Services
{
    public class AuditService
    {
        private readonly IRepository<AuditLog> _auditRepository;
        private readonly IUserRepository _userRepository;

        public AuditService(IRepository<AuditLog> auditRepository, IUserRepository userRepository)
        {
            _auditRepository = auditRepository;
            _userRepository  = userRepository;
        }

        // Tüm audit logları getir
        public async Task<IReadOnlyList<AuditLog>> GetAllAsync()
            => await _auditRepository.ListAllAsync();

        // Belirli bir entity türüne ait loglar
        public async Task<IReadOnlyList<AuditLog>> GetByEntityAsync(string entityName)
            => await _auditRepository.FindAsync(a => a.EntityName == entityName);

        // Belirli bir entity kaydına ait loglar
        public async Task<IReadOnlyList<AuditLog>> GetByEntityIdAsync(string entityName, Guid entityId)
            => await _auditRepository.FindAsync(a => a.EntityName == entityName && a.EntityId == entityId);

        // Belirli kullanıcının işlemleri
        public async Task<IReadOnlyList<AuditLog>> GetByUserAsync(string userId)
            => await _auditRepository.FindAsync(a => a.UserId == userId);

        // İşlem türüne göre (Create / Update / Delete)
        public async Task<IReadOnlyList<AuditLog>> GetByActionAsync(EGM.Domain.Enums.AuditAction action)
            => await _auditRepository.FindAsync(a => a.Action == action);

        // Tarih aralığına göre filtrele
        public async Task<IReadOnlyList<AuditLog>> GetByTarihAraligiAsync(System.DateTime baslangic, System.DateTime bitis)
            => await _auditRepository.FindAsync(a => a.Timestamp >= baslangic && a.Timestamp <= bitis);

        // ID ile getir
        public async Task<AuditLog?> GetByIdAsync(Guid id)
            => await _auditRepository.GetByIdAsync(id);

        // ── Veri girişleri: AuditLog + User join ─────────────────────────
        private static readonly Dictionary<string, string> _konuMap = new()
        {
            ["Olay"]                 = "Olay",
            ["OperasyonelFaaliyet"]  = "Sokak Olayı",
            ["SosyalMedyaOlay"]      = "Sosyal Medya",
            ["SecimSonucu"]          = "Seçim",
            ["SandikOlay"]           = "Seçim (Sandık)",
            ["VIPZiyaret"]           = "VIP Ziyaret",
            ["Organizator"]          = "Kuruluş",
            ["Konu"]                 = "Konu",
            ["Supheli"]              = "Şüpheli",
            ["Sehit"]                = "Şehit",
            ["Olu"]                  = "Ölü",
            ["User"]                 = "Kullanıcı",
        };

        private static readonly Dictionary<AuditAction, string> _faaliyetMap = new()
        {
            [AuditAction.Create] = "Eklendi",
            [AuditAction.Update] = "Güncellendi",
            [AuditAction.Delete] = "Silindi",
        };

        public async Task<IReadOnlyList<VeriGirisiDto>> GetVeriGirislerAsync(int limit = 100)
        {
            var logs  = await _auditRepository.ListAllAsync();
            var users = await _userRepository.GetAllAsync();

            var userMap = users.ToDictionary(
                u => u.Sicil.ToString(),
                u => (FullName: u.FullName, Birim: u.Birim));

            return logs
                .OrderByDescending(l => l.Timestamp)
                .Take(limit)
                .Select(l =>
                {
                    userMap.TryGetValue(l.UserId, out var info);
                    return new VeriGirisiDto
                    {
                        Sicil    = int.TryParse(l.UserId, out var s) ? s : 0,
                        AdSoyad  = info.FullName ?? "-",
                        Birim    = info.Birim    ?? "-",
                        Tarih    = l.Timestamp,
                        Konu     = _konuMap.TryGetValue(l.EntityName, out var k) ? k : l.EntityName,
                        Faaliyet = _faaliyetMap.TryGetValue(l.Action, out var f) ? f : l.Action.ToString(),
                    };
                })
                .ToList()
                .AsReadOnly();
        }
    }
}
