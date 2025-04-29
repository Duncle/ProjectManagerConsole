using System.Security.Cryptography;
using System.Text;
using AuthConsoleApp.Models;
using AuthConsoleApp.Persistence;

namespace AuthConsoleApp.Services;

/// <summary>
/// Отвечает за аутентификацию и хеширование паролей.
/// Хеш хранится в формате  {hash}:{salt}.
/// </summary>
public sealed class AuthService
{
    private readonly IStorage _storage;
    private List<User> _users = new();

    public AuthService(IStorage storage) => _storage = storage;

    /// <summary>Загружаем пользователей при старте приложения.</summary>
    public async Task InitAsync() => _users = (await _storage.LoadUsersAsync()).ToList();

    /// <summary>Пытаемся войти; null ‒ если логин/пароль не подходят.</summary>
    public User? Login(string login, string password)
    {
        var user = _users.SingleOrDefault(u => u.Login == login);
        if (user is null) return null;

        return VerifyPassword(password, user.PasswordHash) ? user : null;
    }

    /// <summary>Создаёт запись пользователя и сохраняет в хранилище.</summary>
    public async Task<User> CreateUserAsync(string login, string rawPassword, Role role)
    {
        if (_users.Any(u => u.Login == login))
            throw new InvalidOperationException("Такой логин уже используется");

        var user = new User(Guid.NewGuid(), login, HashPassword(rawPassword), role);
        _users.Add(user);
        await _storage.SaveUsersAsync(_users);
        return user;
    }

    /* ---------- internal helpers ---------- */

    private static string HashPassword(string password)
    {
        var saltBytes = RandomNumberGenerator.GetBytes(16);
        var salt = Convert.ToBase64String(saltBytes);

        using var sha = SHA256.Create();
        var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(salt + password));
        var hash = Convert.ToBase64String(hashBytes);

        return $"{hash}:{salt}";
    }

    private static bool VerifyPassword(string password, string stored)
    {
        var parts = stored.Split(':', 2);
        if (parts.Length != 2) return false;

        var storedHash = parts[0];
        var salt = parts[1];

        using var sha = SHA256.Create();
        var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(salt + password));
        var hash = Convert.ToBase64String(hashBytes);

        return hash == storedHash;
    }
}
