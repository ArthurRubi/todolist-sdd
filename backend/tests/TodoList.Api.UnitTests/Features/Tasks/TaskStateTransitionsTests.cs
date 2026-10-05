using TodoList.Api.Features.Tasks;
using TodoList.Api.Features.Tasks.Status;

namespace TodoList.Api.UnitTests.Features.Tasks;

public sealed class TaskStateTransitionsTests
{
    private static readonly DateTimeOffset InitialTime = new(2026, 10, 5, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Active_status_change_updates_task_and_appends_one_event()
    {
        var task = CreateTask(TodoTaskStatus.NotStarted);
        var occurredAt = InitialTime.AddMinutes(5);

        var result = TaskStateTransitions.ChangeActiveStatus(task, TodoTaskStatus.InProgress, occurredAt);

        Assert.True(result.Succeeded);
        Assert.True(result.Changed);
        Assert.Equal(TodoTaskStatus.InProgress, task.Status);
        Assert.Equal(occurredAt, task.UpdatedAt);
        Assert.Null(task.CompletedAt);
        var statusEvent = Assert.Single(task.StatusEvents);
        Assert.Equal(TodoTaskStatus.NotStarted, statusEvent.FromStatus);
        Assert.Equal(TodoTaskStatus.InProgress, statusEvent.ToStatus);
        Assert.Equal(occurredAt, statusEvent.OccurredAt);
    }

    [Fact]
    public void Repeating_the_current_active_status_is_a_no_op()
    {
        var task = CreateTask(TodoTaskStatus.Blocked);

        var result = TaskStateTransitions.ChangeActiveStatus(
            task,
            TodoTaskStatus.Blocked,
            InitialTime.AddMinutes(5));

        Assert.True(result.Succeeded);
        Assert.False(result.Changed);
        Assert.Equal(InitialTime, task.UpdatedAt);
        Assert.Empty(task.StatusEvents);
    }

    [Fact]
    public void Completion_is_idempotent_and_preserves_the_first_completion()
    {
        var task = CreateTask(TodoTaskStatus.InProgress);
        var firstCompletion = InitialTime.AddMinutes(5);

        var first = TaskStateTransitions.Complete(task, firstCompletion);
        var repeated = TaskStateTransitions.Complete(task, firstCompletion.AddMinutes(2));

        Assert.True(first.Succeeded);
        Assert.True(first.Changed);
        Assert.True(repeated.Succeeded);
        Assert.False(repeated.Changed);
        Assert.Equal(TodoTaskStatus.Completed, task.Status);
        Assert.Equal(firstCompletion, task.CompletedAt);
        Assert.Equal(firstCompletion, task.UpdatedAt);
        var statusEvent = Assert.Single(task.StatusEvents);
        Assert.Equal(TodoTaskStatus.InProgress, statusEvent.FromStatus);
        Assert.Equal(TodoTaskStatus.Completed, statusEvent.ToStatus);
    }

    [Fact]
    public void Reopening_restores_the_status_before_the_current_completion_and_keeps_history()
    {
        var task = CreateTask(TodoTaskStatus.Blocked);
        var completion = InitialTime.AddMinutes(5);
        TaskStateTransitions.Complete(task, completion);

        var reopenedAt = completion.AddMinutes(2);
        var result = TaskStateTransitions.Reopen(task, reopenedAt);

        Assert.True(result.Succeeded);
        Assert.True(result.Changed);
        Assert.Equal(TodoTaskStatus.Blocked, task.Status);
        Assert.Null(task.CompletedAt);
        Assert.Equal(reopenedAt, task.UpdatedAt);
        Assert.Collection(
            task.StatusEvents.OrderBy(statusEvent => statusEvent.OccurredAt),
            completionEvent =>
            {
                Assert.Equal(TodoTaskStatus.Blocked, completionEvent.FromStatus);
                Assert.Equal(TodoTaskStatus.Completed, completionEvent.ToStatus);
            },
            reopenEvent =>
            {
                Assert.Equal(TodoTaskStatus.Completed, reopenEvent.FromStatus);
                Assert.Equal(TodoTaskStatus.Blocked, reopenEvent.ToStatus);
            });
    }

    [Fact]
    public void Completing_again_after_reopen_creates_a_new_current_completion_without_erasing_events()
    {
        var task = CreateTask(TodoTaskStatus.NotStarted);
        var firstCompletion = InitialTime.AddMinutes(1);
        TaskStateTransitions.Complete(task, firstCompletion);
        TaskStateTransitions.Reopen(task, firstCompletion.AddMinutes(1));

        var secondCompletion = firstCompletion.AddMinutes(3);
        var result = TaskStateTransitions.Complete(task, secondCompletion);

        Assert.True(result.Succeeded);
        Assert.True(result.Changed);
        Assert.Equal(secondCompletion, task.CompletedAt);
        Assert.Equal(TodoTaskStatus.Completed, task.Status);
        Assert.Equal(3, task.StatusEvents.Count);
        Assert.Equal(2, task.StatusEvents.Count(statusEvent => statusEvent.ToStatus == TodoTaskStatus.Completed));
    }

    [Fact]
    public void Invalid_transitions_return_explicit_failures_without_mutating_the_task()
    {
        var active = CreateTask(TodoTaskStatus.NotStarted);
        var completed = CreateTask(TodoTaskStatus.Completed, InitialTime);

        var completedAsActive = TaskStateTransitions.ChangeActiveStatus(
            active,
            TodoTaskStatus.Completed,
            InitialTime.AddMinutes(1));
        var activeChangeOnCompleted = TaskStateTransitions.ChangeActiveStatus(
            completed,
            TodoTaskStatus.InProgress,
            InitialTime.AddMinutes(1));
        var reopenActive = TaskStateTransitions.Reopen(active, InitialTime.AddMinutes(1));
        var reopenWithoutHistory = TaskStateTransitions.Reopen(completed, InitialTime.AddMinutes(1));

        Assert.Equal(TaskTransitionFailure.InvalidActiveStatus, completedAsActive.Failure);
        Assert.Equal(TaskTransitionFailure.CompletedTaskMustBeReopened, activeChangeOnCompleted.Failure);
        Assert.Equal(TaskTransitionFailure.TaskIsNotCompleted, reopenActive.Failure);
        Assert.Equal(TaskTransitionFailure.CompletionEventNotFound, reopenWithoutHistory.Failure);
        Assert.Equal(TodoTaskStatus.NotStarted, active.Status);
        Assert.Equal(TodoTaskStatus.Completed, completed.Status);
        Assert.Empty(active.StatusEvents);
        Assert.Empty(completed.StatusEvents);
    }

    private static TaskItem CreateTask(
        TodoTaskStatus status,
        DateTimeOffset? completedAt = null) => new()
        {
            Id = Guid.NewGuid(),
            CreationIdempotencyKey = Guid.NewGuid(),
            Title = "Testar transição",
            Status = status,
            Priority = TaskPriority.None,
            CreatedAt = InitialTime,
            UpdatedAt = InitialTime,
            CompletedAt = completedAt,
        };
}
