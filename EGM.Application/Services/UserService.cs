using EGM.Domain.Entities;
using EGM.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace EGM.Application.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public User? ValidateUser(int sicil, string password)
        {
            var user = _userRepository.GetBySicil(sicil);
            if (user == null) return null;

            //Hash Doğrulama
            bool isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            return isValid ? user : null;
        }
        public void RegisterUser(User user)
        {
            var existing = _userRepository.GetBySicil(user.Sicil);
            if (existing != null)
                throw new InvalidOperationException("Bu sicil numarası zaten mevcut!");
            _userRepository.Add(user);
        }
        public void UpdateUser(User user)
        {
            _userRepository.Update(user);
        }
        public void DeleteUser(int sicil)
        {
            _userRepository.Delete(sicil);
        }
        public IEnumerable<User> GetAllUsers()
        {
            return _userRepository.GetAll();
        }
    }
}
