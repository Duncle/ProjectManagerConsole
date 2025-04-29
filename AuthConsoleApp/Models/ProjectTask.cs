namespace AuthConsoleApp.Models;

public record ProjectTask(
    Guid Id,
    string Title,
    string Description,
    Guid AssigneeId,
    TaskStatus Status = TaskStatus.ToDo
);