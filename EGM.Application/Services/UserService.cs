using EGM.Domain.Entities;
using EGM.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EGM.Application.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<User?> ValidateUserAsync(int sicil, string password)
        {
            var user = await _userRepository.GetBySicilAsync(sicil);
            if (user == null)
            {
                _logger.LogWarning("Sicil bulunamadı: {Sicil}", sicil);
                return null;
            }

            bool isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            if (!isValid)
                _logger.LogWarning("Hatalı şifre girişi. Sicil: {Sicil}", sicil);
            return isValid ? user : null;
        }

        public async Task RegisterUserAsync(User user)
        {
            var existing = await _userRepository.GetBySicilAsync(user.Sicil);
            if (existing != null)
            {
                _logger.LogWarning("Tekrar kayıt denemesi. Sicil: {Sicil}", user.Sicil);
                throw new InvalidOperationException("Bu sicil numarası zaten mevcut!");
            }
            await _userRepository.AddAsync(user);
        }

        public async Task UpdateUserAsync(User user)
            => await _userRepository.UpdateAsync(user);

        public async Task DeleteUserAsync(int sicil)
            => await _userRepository.DeleteAsync(sicil);

        public async Task<IEnumerable<User>> GetAllUsersAsync()
            => await _userRepository.GetAllAsync();
    }
}
