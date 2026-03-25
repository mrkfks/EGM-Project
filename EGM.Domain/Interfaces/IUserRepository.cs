using EGM.Domain.Entities;

namespace EGM.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetBySicilAsync(int sicil);
        Task<IEnumerable<User>> GetByRoleAsync(string role);
        Task<IEnumerable<User>> GetByFullNameAsync(string fullName);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByGsmAsync(string gsm);

        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(int sicil);

        Task<IEnumerable<User>> GetAllAsync();
    }
}

