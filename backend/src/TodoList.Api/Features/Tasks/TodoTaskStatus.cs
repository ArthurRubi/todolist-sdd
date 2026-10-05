namespace TodoList.Api.Features.Tasks;

public enum TodoTaskStatus
{
    NotStarted,
    InProgress,
    Blocked,
    Completed,
}

public static class TodoTaskStatusStorage
{
    public static string ToStorageValue(this TodoTaskStatus status) => status switch
    {
        TodoTaskStatus.NotStarted => "not_started",
        TodoTaskStatus.InProgress => "in_progress",
        TodoTaskStatus.Blocked => "blocked",
        TodoTaskStatus.Completed => "completed",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
    };

    public static TodoTaskStatus FromStorageValue(string value) => value switch
    {
        "not_started" => TodoTaskStatus.NotStarted,
        "in_progress" => TodoTaskStatus.InProgress,
        "blocked" => TodoTaskStatus.Blocked,
        "completed" => TodoTaskStatus.Completed,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };
}
