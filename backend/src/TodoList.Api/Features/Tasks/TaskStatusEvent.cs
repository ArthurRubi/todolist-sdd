namespace TodoList.Api.Features.Tasks;

public sealed class TaskStatusEvent
{
    public Guid Id { get; set; }

    public Guid TaskId { get; set; }

    public TodoTaskStatus FromStatus { get; set; }

    public TodoTaskStatus ToStatus { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    public TaskItem Task { get; set; } = null!;
}
