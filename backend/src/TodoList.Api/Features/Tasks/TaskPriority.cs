namespace TodoList.Api.Features.Tasks;

public enum TaskPriority
{
    None,
    Low,
    Medium,
    High,
    Urgent,
}

public static class TaskPriorityStorage
{
    public static string ToStorageValue(this TaskPriority priority) => priority switch
    {
        TaskPriority.None => "none",
        TaskPriority.Low => "low",
        TaskPriority.Medium => "medium",
        TaskPriority.High => "high",
        TaskPriority.Urgent => "urgent",
        _ => throw new ArgumentOutOfRangeException(nameof(priority), priority, null),
    };

    public static TaskPriority FromStorageValue(string value) => value switch
    {
        "none" => TaskPriority.None,
        "low" => TaskPriority.Low,
        "medium" => TaskPriority.Medium,
        "high" => TaskPriority.High,
        "urgent" => TaskPriority.Urgent,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };
}
