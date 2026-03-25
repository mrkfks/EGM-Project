using System.Collections.Generic;
using System.Threading.Tasks;
using EGM.Domain.Entities;
using EGM.Domain.Interfaces;

namespace EGM.Application.Services
{
    public class AuditService
    {
        private readonly IRepository<AuditLog> _auditRepository;

        public AuditService(IRepository<AuditLog> auditRepository)
        {
            _auditRepository = auditRepository;
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
    }
}
