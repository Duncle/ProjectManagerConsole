using AuthConsoleApp.Models;
using AuthConsoleApp.Persistence;

namespace AuthConsoleApp.Services;

/// <summary>
/// Управление сотрудниками (регистрация, просмотр списков).
/// Все проверки прав доступа выполняет уровень UI ‒
/// сам сервис ничего не знает о текущем пользователе.
/// </summary>
public sealed class UserService
{
    private readonly IStorage _storage;
    private readonly AuthService _auth;      // используем для хеширования
    private List<User> _users = new();

    public UserService(IStorage storage, AuthService auth)
    {
        _storage = storage;
        _auth = auth;
    }

    public async Task InitAsync() => _users = (await _storage.LoadUsersAsync()).ToList();

    public IEnumerable<User> GetAllUsers() => _users;
    
    public IEnumerable<User> GetAllEmployees() =>
        _users.Where(u => u.Role == Role.Employee);

    /// <summary>Регистрирует нового сотрудника (роль Employee).</summary>
    public async Task<User> RegisterEmployeeAsync(string login, string rawPassword)
    {
        var user = await _auth.CreateUserAsync(login, rawPassword, Role.Employee);
        _users.Add(user); // CreateUserAsync уже сохранил список, но держим локальный кэш в синхроне
        return user;
    }

    /// <summary>Регистрация управляющего ‒ пригодится при первом запуске.</summary>
    public async Task<User> RegisterManagerAsync(string login, string rawPassword)
        => await _auth.CreateUserAsync(login, rawPassword, Role.Manager);
}