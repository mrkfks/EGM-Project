using EGM.Domain.Entities;

namespace EGM.Domain.Interfaces
{
    public interface IUserRepository
    {
        User? GetBySicil(int sicil);
        IEnumerable<User> GetByRole(string role);
        IEnumerable<User> GetByFullName(string fullName);
        User? GetByEmail(string email);
        User? GetByGsm(string gsm);

        void Add(User user);
        void Update(User user);
        void Delete(int sicil);

        IEnumerable<User> GetAll();
    }
}

