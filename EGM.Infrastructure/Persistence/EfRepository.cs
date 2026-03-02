using EGM.Application.Common.Interfaces;
using EGM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EGM.Infrastructure.Persistence
{
    public class EfRepository<T> : IRepository<T> where T : class
    {
        private readonly EGMDbContext _dbContext;

        public EfRepository(EGMDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }

        public async Task<IReadOnlyList<T>> ListAllAsync()
        {
            return await _dbContext.Set<T>().ToListAsync();
        }

        public async Task<T> AddAsync(T entity)
        {
            _dbContext.Set<T>().Add(entity);
            await _dbContext.SaveChangesAsync();

            await AddAuditLog(entity, "Create");

            return entity;
        }

        public async Task UpdateAsync(T entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            await AddAuditLog(entity, "Update");
        }

        public async Task DeleteAsync(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
            await _dbContext.SaveChangesAsync();

            await AddAuditLog(entity, "Delete");
        }

        private async Task AddAuditLog(T entity, string action)
        {
            var idProp = entity.GetType().GetProperty("Id");
            var entityId = idProp != null ? (int)idProp.GetValue(entity)! : 0;

            var audit = new AuditLog
            {
                Entity = typeof(T).Name,
                EntityId = entityId,
                Action = action,
                UserId = "system", // Burada gerçek kullanıcı kimliği alınmalı
                Timestamp = DateTime.UtcNow,
                Changes = System.Text.Json.JsonSerializer.Serialize(entity)
            };

            _dbContext.AuditLoglar.Add(audit);
            await _dbContext.SaveChangesAsync();
        }
    }
}
