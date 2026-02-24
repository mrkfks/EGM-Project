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
    
        public User? GetBySicil(int sicil)
        {
            return _context.Users.FirstOrDefault(u => u.Sicil == sicil);
        }

        public IEnumerable<User> GetByRole(string role)
        {
            return _context.Users.Where(u => u.Role == role).ToList();
        }
        public IEnumerable<User>GetByFullName(string fullName)
        {
            return _context.Users.Where(u => u.FullName == fullName);
        }

        public User? GetByEmail(string email)
        {
            return _context.Users.FirstOrDefault(u => u.Email == email);
        }

        public User? GetByGsm(string gsm)
        {
            return _context.Users.FirstOrDefault(u => u.GSM == gsm);
        }

        public void Add(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public void Update(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public void Delete(int sicil)
        {
            var user = GetBySicil(sicil);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
        }



        public IEnumerable<User> GetAll()
        {
            return _context.Users.ToList();
        }
    }
}