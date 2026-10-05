using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using TodoList.Api.Data;
using TodoList.Api.Features.Tasks;
using TodoList.Api.IntegrationTests.Infrastructure;

namespace TodoList.Api.IntegrationTests.Features.Tasks;

[Collection(PostgresCollection.Name)]
public sealed class TaskStatusTests(PostgresContainerFixture postgres)
{
    [Fact]
    public async Task Active_status_changes_are_persisted_with_one_event_and_no_completion()
    {
        var taskId = await ResetAndSeedAsync(TodoTaskStatus.NotStarted);
        using var factory = new ApiFactory(postgres.ConnectionString);
        using var client = factory.CreateClient();

        using var response = await client.PutAsJsonAsync($"/api/tasks/{taskId}/status", new
        {
            status = "in_progress",
        });
        var updated = await response.Content.ReadFromJsonAsync<TaskDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("in_progress", updated!.Status);
        Assert.Null(updated.CompletedAt);
        var statusEvent = Assert.Single(await ReadEventsAsync(taskId));
        Assert.Equal(TodoTaskStatus.NotStarted, statusEvent.FromStatus);
        Assert.Equal(TodoTaskStatus.InProgress, statusEvent.ToStatus);

        using var repeatedResponse = await client.PutAsJsonAsync($"/api/tasks/{taskId}/status", new
        {
            status = "in_progress",
        });

        Assert.Equal(HttpStatusCode.OK, repeatedResponse.StatusCode);
        Assert.Single(await ReadEventsAsync(taskId));
    }

    [Fact]
    public async Task Completion_is_idempotent_and_moves_the_task_to_the_completed_view()
    {
        var taskId = await ResetAndSeedAsync(TodoTaskStatus.InProgress);
        using var factory = new ApiFactory(postgres.ConnectionString);
        using var client = factory.CreateClient();

        using var firstResponse = await client.PostAsync($"/api/tasks/{taskId}/complete", null);
        var first = await firstResponse.Content.ReadFromJsonAsync<TaskDto>();
        using var repeatedResponse = await client.PostAsync($"/api/tasks/{taskId}/complete", null);
        var repeated = await repeatedResponse.Content.ReadFromJsonAsync<TaskDto>();

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, repeatedResponse.StatusCode);
        Assert.Equal("completed", first!.Status);
        Assert.NotNull(first.CompletedAt);
        Assert.Equal(first.CompletedAt, repeated!.CompletedAt);
        Assert.Equal(first.UpdatedAt, repeated.UpdatedAt);
        Assert.Single(await ReadEventsAsync(taskId));
        var active = await client.GetFromJsonAsync<List<TaskDto>>("/api/tasks?view=active");
        var completed = await client.GetFromJsonAsync<List<TaskDto>>("/api/tasks?view=completed");
        Assert.DoesNotContain(active!, task => task.Id == taskId);
        Assert.Contains(completed!, task => task.Id == taskId);
    }

    [Fact]
    public async Task Reopen_restores_the_previous_active_status_and_a_new_completion_keeps_all_events()
    {
        var taskId = await ResetAndSeedAsync(TodoTaskStatus.Blocked);
        using var factory = new ApiFactory(postgres.ConnectionString);
        using var client = factory.CreateClient();
        var firstCompletion = await CompleteAsync(client, taskId);

        using var reopenResponse = await client.PostAsync($"/api/tasks/{taskId}/reopen", null);
        var reopened = await reopenResponse.Content.ReadFromJsonAsync<TaskDto>();

        Assert.Equal(HttpStatusCode.OK, reopenResponse.StatusCode);
        Assert.Equal("blocked", reopened!.Status);
        Assert.Null(reopened.CompletedAt);
        var active = await client.GetFromJsonAsync<List<TaskDto>>("/api/tasks?view=active");
        Assert.Contains(active!, task => task.Id == taskId);

        var secondCompletion = await CompleteAsync(client, taskId);
        Assert.NotEqual(firstCompletion.CompletedAt, secondCompletion.CompletedAt);
        Assert.Equal(3, (await ReadEventsAsync(taskId)).Count);
        Assert.Equal(2, (await ReadEventsAsync(taskId)).Count(statusEvent =>
            statusEvent.ToStatus == TodoTaskStatus.Completed));
    }

    [Fact]
    public async Task Invalid_status_and_invalid_reopen_return_problem_details_without_changes()
    {
        var activeTaskId = await ResetAndSeedAsync(TodoTaskStatus.NotStarted);
        using var factory = new ApiFactory(postgres.ConnectionString);
        using var client = factory.CreateClient();

        using var invalidStatus = await client.PutAsJsonAsync($"/api/tasks/{activeTaskId}/status", new
        {
            status = "completed",
        });
        using var invalidReopen = await client.PostAsync($"/api/tasks/{activeTaskId}/reopen", null);

        Assert.Equal(HttpStatusCode.BadRequest, invalidStatus.StatusCode);
        Assert.Equal("application/problem+json", invalidStatus.Content.Headers.ContentType?.MediaType);
        Assert.Equal(HttpStatusCode.Conflict, invalidReopen.StatusCode);
        var persisted = await client.GetFromJsonAsync<TaskDto>($"/api/tasks/{activeTaskId}");
        Assert.Equal("not_started", persisted!.Status);
        Assert.Empty(await ReadEventsAsync(activeTaskId));
    }

    [Fact]
    public async Task Completed_task_requires_reopen_and_missing_completion_history_is_rejected()
    {
        var completedAt = DateTimeOffset.UtcNow.AddHours(-1);
        var taskId = await ResetAndSeedAsync(TodoTaskStatus.Completed, completedAt);
        using var factory = new ApiFactory(postgres.ConnectionString);
        using var client = factory.CreateClient();

        using var statusResponse = await client.PutAsJsonAsync($"/api/tasks/{taskId}/status", new
        {
            status = "blocked",
        });
        using var reopenResponse = await client.PostAsync($"/api/tasks/{taskId}/reopen", null);

        Assert.Equal(HttpStatusCode.Conflict, statusResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, reopenResponse.StatusCode);
        var persisted = await client.GetFromJsonAsync<TaskDto>($"/api/tasks/{taskId}");
        Assert.Equal("completed", persisted!.Status);
        Assert.Equal(completedAt, persisted.CompletedAt);
        Assert.Empty(await ReadEventsAsync(taskId));
    }

    [Fact]
    public async Task Task_and_event_are_rolled_back_together_when_event_insert_fails()
    {
        var taskId = await ResetAndSeedAsync(TodoTaskStatus.NotStarted);
        await using (var db = CreateDbContext())
        {
            await db.Database.ExecuteSqlRawAsync("""
                CREATE OR REPLACE FUNCTION reject_status_event() RETURNS trigger AS $$
                BEGIN
                    RAISE EXCEPTION 'forced status event failure';
                END;
                $$ LANGUAGE plpgsql;
                CREATE TRIGGER reject_status_event_trigger
                BEFORE INSERT ON task_status_events
                FOR EACH ROW EXECUTE FUNCTION reject_status_event();
                """);
        }

        try
        {
            using var factory = new ApiFactory(postgres.ConnectionString);
            using var client = factory.CreateClient();
            using var response = await client.PutAsJsonAsync($"/api/tasks/{taskId}/status", new
            {
                status = "blocked",
            });

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            await using var verificationDb = CreateDbContext();
            var task = await verificationDb.Tasks.AsNoTracking().SingleAsync(item => item.Id == taskId);
            Assert.Equal(TodoTaskStatus.NotStarted, task.Status);
            Assert.Empty(await verificationDb.TaskStatusEvents.AsNoTracking().ToListAsync());
        }
        finally
        {
            await using var cleanupDb = CreateDbContext();
            await cleanupDb.Database.ExecuteSqlRawAsync("""
                DROP TRIGGER IF EXISTS reject_status_event_trigger ON task_status_events;
                DROP FUNCTION IF EXISTS reject_status_event();
                """);
        }
    }

    private async Task<TaskDto> CompleteAsync(HttpClient client, Guid taskId)
    {
        using var response = await client.PostAsync($"/api/tasks/{taskId}/complete", null);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TaskDto>())!;
    }

    private async Task<Guid> ResetAndSeedAsync(
        TodoTaskStatus status,
        DateTimeOffset? completedAt = null)
    {
        await ResetDatabaseAsync();
        var createdAt = DateTimeOffset.UtcNow.AddDays(-2);
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            CreationIdempotencyKey = Guid.NewGuid(),
            Title = "Acompanhar estado",
            Status = status,
            Priority = TaskPriority.None,
            CreatedAt = createdAt,
            UpdatedAt = createdAt,
            CompletedAt = completedAt,
        };

        await using var db = CreateDbContext();
        db.Tasks.Add(task);
        await db.SaveChangesAsync();
        return task.Id;
    }

    private async Task<List<TaskStatusEvent>> ReadEventsAsync(Guid taskId)
    {
        await using var db = CreateDbContext();
        return await db.TaskStatusEvents.AsNoTracking()
            .Where(statusEvent => statusEvent.TaskId == taskId)
            .OrderBy(statusEvent => statusEvent.OccurredAt)
            .ThenBy(statusEvent => statusEvent.Id)
            .ToListAsync();
    }

    private async Task ResetDatabaseAsync()
    {
        await using var db = CreateDbContext();
        await db.Database.MigrateAsync();
        await postgres.ResetAsync();
    }

    private TodoListDbContext CreateDbContext() => new(
        new DbContextOptionsBuilder<TodoListDbContext>()
            .UseNpgsql(postgres.ConnectionString)
            .Options);

    private sealed record TaskDto(
        Guid Id,
        string Title,
        string? Description,
        string Status,
        string Priority,
        DateOnly? DueDate,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt,
        DateTimeOffset? CompletedAt);
}
