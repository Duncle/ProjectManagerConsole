namespace AuthConsoleApp.Models;

public record User(Guid Id, string Login, string PasswordHash, Role Role);