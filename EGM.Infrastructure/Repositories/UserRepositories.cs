using EGM.Domain.Interfaces;
using EGM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EGM.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly EGMDbContext _context;

        public UserRepository(EGMDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetBySicilAsync(int sicil)
            => await _context.Users.FirstOrDefaultAsync(u => u.Sicil == sicil);

        public async Task<IEnumerable<User>> GetByRoleAsync(string role)
            => await _context.Users.Where(u => u.Role == role).ToListAsync();

        public async Task<IEnumerable<User>> GetByFullNameAsync(string fullName)
            => await _context.Users.Where(u => u.FullName == fullName).ToListAsync();

        public async Task<User?> GetByEmailAsync(string email)
            => await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        public async Task<User?> GetByGsmAsync(string gsm)
            => await _context.Users.FirstOrDefaultAsync(u => u.GSM == gsm);

        public async Task AddAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int sicil)
        {
            var user = await GetBySicilAsync(sicil);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<User>> GetAllAsync()
            => await _context.Users.ToListAsync();
    }
}