using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EGM.Domain.Entities;

namespace EGM.Domain.Interfaces
{
    public interface IRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<T>> ListAllAsync();

        /// <summary>Verilen koşula uyan tüm kayıtları döner (soft-delete filtresi uygulanmış).</summary>
        Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate);

        /// <summary>Sayfalanmış sorgu: toplam sayı + istenilen dilim döner.</summary>
        Task<(IReadOnlyList<T> Items, int TotalCount)> PageAsync(
            Expression<Func<T, bool>>? predicate, int page, int pageSize);

        Task<T> AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
    }
}
