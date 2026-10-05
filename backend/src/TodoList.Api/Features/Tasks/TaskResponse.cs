namespace TodoList.Api.Features.Tasks;

public sealed record TaskResponse(
    Guid Id,
    string Title,
    string? Description,
    string Status,
    string Priority,
    DateOnly? DueDate,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? CompletedAt)
{
    public static TaskResponse From(TaskItem task) => new(
        task.Id,
        task.Title,
        task.Description,
        task.Status.ToStorageValue(),
        task.Priority.ToStorageValue(),
        task.DueDate,
        task.CreatedAt,
        task.UpdatedAt,
        task.CompletedAt);
}
