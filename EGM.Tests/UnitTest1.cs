using EGM.Application.Services;
using EGM.Domain.Entities;
using EGM.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;

namespace EGM.Tests;

// ── Sahte IUserRepository ────────────────────────────────────────────────
internal sealed class FakeUserRepository : IUserRepository
{
    private readonly List<User> _store = new();

    public Task<User?> GetBySicilAsync(int sicil)
        => Task.FromResult(_store.FirstOrDefault(u => u.Sicil == sicil));

    public Task<IEnumerable<User>> GetAllAsync()
        => Task.FromResult<IEnumerable<User>>(_store.ToList());

    public Task AddAsync(User user)
    {
        user.Id = Guid.NewGuid();
        _store.Add(user);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(User user) => Task.CompletedTask;

    public Task DeleteAsync(int sicil)
    {
        var u = _store.FirstOrDefault(x => x.Sicil == sicil);
        if (u != null) _store.Remove(u);
        return Task.CompletedTask;
    }

    // Kullanılmayan metotlar (testlerde gerekmez)
    public Task<IEnumerable<User>> GetByRoleAsync(string role)
        => Task.FromResult<IEnumerable<User>>(new List<User>());
    public Task<IEnumerable<User>> GetByFullNameAsync(string fullName)
        => Task.FromResult<IEnumerable<User>>(new List<User>());
    public Task<User?> GetByEmailAsync(string email)
        => Task.FromResult<User?>(null);
    public Task<User?> GetByGsmAsync(string gsm)
        => Task.FromResult<User?>(null);
}

// ── UserService Testleri ─────────────────────────────────────────────────
public class UserServiceTests
{
    private static UserService BuildService(IUserRepository repo)
        => new(repo, NullLogger<UserService>.Instance);

    private static User CreateUser(int sicil, string password) => new()
    {
        Sicil        = sicil,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
        FullName     = "Test Kullanıcı",
        Email        = "test@egm.gov.tr",
        GSM          = "5001234567",
        Role         = "Izleyici"
    };

    // ── ValidateUserAsync ────────────────────────────────────────────────

    [Fact]
    public async Task ValidateUserAsync_SicilBulunamazsa_NullDoner()
    {
        var svc = BuildService(new FakeUserRepository());
        var result = await svc.ValidateUserAsync(99999, "sifre");
        Assert.Null(result);
    }

    [Fact]
    public async Task ValidateUserAsync_YanlisParola_NullDoner()
    {
        var repo = new FakeUserRepository();
        var user = CreateUser(1001, "dogruSifre");
        await repo.AddAsync(user);

        var svc = BuildService(repo);
        var result = await svc.ValidateUserAsync(1001, "yanlisSifre");
        Assert.Null(result);
    }

    [Fact]
    public async Task ValidateUserAsync_DogRuKimlikBilgileri_KullaniciyiDoner()
    {
        var repo = new FakeUserRepository();
        var user = CreateUser(1002, "dogruSifre123");
        await repo.AddAsync(user);

        var svc = BuildService(repo);
        var result = await svc.ValidateUserAsync(1002, "dogruSifre123");
        Assert.NotNull(result);
        Assert.Equal(1002, result!.Sicil);
    }

    // ── RegisterUserAsync ────────────────────────────────────────────────

    [Fact]
    public async Task RegisterUserAsync_YeniKullanici_BasariylaKaydeder()
    {
        var repo = new FakeUserRepository();
        var svc  = BuildService(repo);
        var user = CreateUser(2001, "Sifre123!");

        await svc.RegisterUserAsync(user);

        var saved = await repo.GetBySicilAsync(2001);
        Assert.NotNull(saved);
    }

    [Fact]
    public async Task RegisterUserAsync_MevcutSicil_HataFirlatir()
    {
        var repo = new FakeUserRepository();
        var user = CreateUser(3001, "Sifre!1");
        await repo.AddAsync(user);

        var svc      = BuildService(repo);
        var duplicate = CreateUser(3001, "BaskaSifre!1");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => svc.RegisterUserAsync(duplicate));
    }

    // ── GetAllUsersAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task GetAllUsersAsync_KayitliKullanicilar_HepsiniDoner()
    {
        var repo = new FakeUserRepository();
        await repo.AddAsync(CreateUser(5001, "s1"));
        await repo.AddAsync(CreateUser(5002, "s2"));

        var svc    = BuildService(repo);
        var result = await svc.GetAllUsersAsync();

        Assert.Equal(2, result.Count());
    }
}

