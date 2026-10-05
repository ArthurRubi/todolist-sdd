namespace TodoList.Api.Features.Tasks;

public sealed class TaskItem
{
    public Guid Id { get; set; }

    public Guid CreationIdempotencyKey { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TodoTaskStatus Status { get; set; }

    public TaskPriority Priority { get; set; }

    public DateOnly? DueDate { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public ICollection<TaskStatusEvent> StatusEvents { get; } = new List<TaskStatusEvent>();
}
