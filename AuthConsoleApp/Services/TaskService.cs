using AuthConsoleApp.Models;
using AuthConsoleApp.Persistence;
using TaskStatus = AuthConsoleApp.Models.TaskStatus;

namespace AuthConsoleApp.Services;

public sealed class TaskService
{
    private readonly IStorage _storage;
    private List<ProjectTask> _tasks = new();

    public TaskService(IStorage storage) => _storage = storage;

    public async Task InitAsync() => _tasks = (await _storage.LoadTasksAsync()).ToList();

    public IEnumerable<ProjectTask> GetTasksFor(Guid userId) =>
        _tasks.Where(t => t.AssigneeId == userId);

    public IReadOnlyList<ProjectTask> GetTasksForList(Guid userId) =>
        _tasks.Where(t => userId == Guid.Empty || t.AssigneeId == userId)
            .OrderBy(t => t.Status)
            .ToList();
    
    public ProjectTask Create(string title, string desc, Guid assigneeId)
    {
        var t = new ProjectTask(Guid.NewGuid(), title, desc, assigneeId);
        _tasks.Add(t);
        return t;
    }

    public void UpdateStatus(Guid taskId, TaskStatus status, Guid userId)
    {
        var t = _tasks.Single(x => x.Id == taskId && x.AssigneeId == userId);
        _tasks[_tasks.IndexOf(t)] = t with { Status = status };
    }

    public async Task PersistAsync() => await _storage.SaveTasksAsync(_tasks);
}