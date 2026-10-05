namespace TodoList.Api.Features.Tasks.Status;

public enum TaskTransitionFailure
{
    None,
    InvalidActiveStatus,
    CompletedTaskMustBeReopened,
    TaskIsNotCompleted,
    CompletionEventNotFound,
}

public readonly record struct TaskTransitionResult(bool Changed, TaskTransitionFailure Failure)
{
    public bool Succeeded => Failure == TaskTransitionFailure.None;

    public static TaskTransitionResult Applied() => new(true, TaskTransitionFailure.None);

    public static TaskTransitionResult Unchanged() => new(false, TaskTransitionFailure.None);

    public static TaskTransitionResult Rejected(TaskTransitionFailure failure) => new(false, failure);
}

public static class TaskStateTransitions
{
    public static TaskTransitionResult ChangeActiveStatus(
        TaskItem task,
        TodoTaskStatus targetStatus,
        DateTimeOffset occurredAt)
    {
        if (targetStatus == TodoTaskStatus.Completed)
        {
            return TaskTransitionResult.Rejected(TaskTransitionFailure.InvalidActiveStatus);
        }

        if (task.Status == TodoTaskStatus.Completed)
        {
            return TaskTransitionResult.Rejected(TaskTransitionFailure.CompletedTaskMustBeReopened);
        }

        return task.Status == targetStatus
            ? TaskTransitionResult.Unchanged()
            : Apply(task, targetStatus, occurredAt, completedAt: null);
    }

    public static TaskTransitionResult Complete(TaskItem task, DateTimeOffset occurredAt)
    {
        if (task.Status == TodoTaskStatus.Completed)
        {
            return TaskTransitionResult.Unchanged();
        }

        return Apply(task, TodoTaskStatus.Completed, occurredAt, occurredAt);
    }

    public static TaskTransitionResult Reopen(TaskItem task, DateTimeOffset occurredAt)
    {
        if (task.Status != TodoTaskStatus.Completed)
        {
            return TaskTransitionResult.Rejected(TaskTransitionFailure.TaskIsNotCompleted);
        }

        var currentCompletion = task.CompletedAt;
        var completionEvent = task.StatusEvents
            .Where(statusEvent =>
                statusEvent.ToStatus == TodoTaskStatus.Completed
                && statusEvent.OccurredAt == currentCompletion)
            .OrderByDescending(statusEvent => statusEvent.Id)
            .FirstOrDefault();
        if (completionEvent is null || completionEvent.FromStatus == TodoTaskStatus.Completed)
        {
            return TaskTransitionResult.Rejected(TaskTransitionFailure.CompletionEventNotFound);
        }

        return Apply(task, completionEvent.FromStatus, occurredAt, completedAt: null);
    }

    private static TaskTransitionResult Apply(
        TaskItem task,
        TodoTaskStatus targetStatus,
        DateTimeOffset occurredAt,
        DateTimeOffset? completedAt)
    {
        var statusEvent = new TaskStatusEvent
        {
            Id = Guid.NewGuid(),
            TaskId = task.Id,
            Task = task,
            FromStatus = task.Status,
            ToStatus = targetStatus,
            OccurredAt = occurredAt,
        };

        task.Status = targetStatus;
        task.UpdatedAt = occurredAt;
        task.CompletedAt = completedAt;
        task.StatusEvents.Add(statusEvent);
        return TaskTransitionResult.Applied();
    }
}
