using System.Text.Json;
using AuthConsoleApp.Models;

namespace AuthConsoleApp.Persistence;

public sealed class JsonFileStorage : IStorage
{
    private readonly string _usersFile = "users.json";
    private readonly string _tasksFile = "tasks.json";
    private readonly JsonSerializerOptions _opts = new() { WriteIndented = true };

    public async Task<IReadOnlyList<User>> LoadUsersAsync() =>
        await LoadAsync<User>(_usersFile);

    public async Task SaveUsersAsync(IEnumerable<User> users) =>
        await SaveAsync(_usersFile, users);

    public async Task<IReadOnlyList<ProjectTask>> LoadTasksAsync() =>
        await LoadAsync<ProjectTask>(_tasksFile);

    public async Task SaveTasksAsync(IEnumerable<ProjectTask> tasks) =>
        await SaveAsync(_tasksFile, tasks);
    
    //helpers
    private static async Task<IReadOnlyList<T>> LoadAsync<T>(string path)
    {
        if (!File.Exists(path)) return Array.Empty<T>();
        var json = await File.ReadAllTextAsync(path);
        return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
    }

    private async Task SaveAsync<T>(string path, IEnumerable<T> data)
    {
        var json = JsonSerializer.Serialize(data, _opts);
        await File.WriteAllTextAsync(path, json);
    }
}