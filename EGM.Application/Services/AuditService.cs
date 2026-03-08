using System.Collections.Generic;
using System.Linq;
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
        {
            var all = await _auditRepository.ListAllAsync();
            return all.Where(a => a.Entity == entityName).ToList();
        }

        // Belirli bir entity kaydına ait loglar
        public async Task<IReadOnlyList<AuditLog>> GetByEntityIdAsync(string entityName, int entityId)
        {
            var all = await _auditRepository.ListAllAsync();
            return all.Where(a => a.Entity == entityName && a.EntityId == entityId).ToList();
        }

        // Belirli kullanıcının işlemleri
        public async Task<IReadOnlyList<AuditLog>> GetByUserAsync(string userId)
        {
            var all = await _auditRepository.ListAllAsync();
            return all.Where(a => a.UserId == userId).ToList();
        }

        // İşlem türüne göre (Create / Update / Delete)
        public async Task<IReadOnlyList<AuditLog>> GetByActionAsync(string action)
        {
            var all = await _auditRepository.ListAllAsync();
            return all.Where(a => a.Action == action).ToList();
        }

        // Tarih aralığına göre filtrele
        public async Task<IReadOnlyList<AuditLog>> GetByTarihAraligiAsync(System.DateTime baslangic, System.DateTime bitis)
        {
            var all = await _auditRepository.ListAllAsync();
            return all.Where(a => a.Timestamp >= baslangic && a.Timestamp <= bitis).ToList();
        }

        // ID ile getir
        public async Task<AuditLog?> GetByIdAsync(int id)
            => await _auditRepository.GetByIdAsync(id);
    }
}
