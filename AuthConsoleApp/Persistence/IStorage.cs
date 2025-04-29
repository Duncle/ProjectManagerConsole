using AuthConsoleApp.Models;

namespace AuthConsoleApp.Persistence;

public interface IStorage
{
    Task<IReadOnlyList<User>>  LoadUsersAsync();
    Task SaveUsersAsync(IEnumerable<User> users);

    Task<IReadOnlyList<ProjectTask>> LoadTasksAsync();
    Task SaveTasksAsync(IEnumerable<ProjectTask> tasks);
}