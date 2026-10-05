using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using TodoList.Api.Data;
using TodoList.Api.Features.Tasks;
using TodoList.Api.IntegrationTests.Infrastructure;

namespace TodoList.Api.IntegrationTests.Features.Tasks;

[Collection(PostgresCollection.Name)]
public sealed class UpdateTaskTests(PostgresContainerFixture postgres)
{
    [Fact]
    public async Task Valid_update_replaces_editable_fields_and_renews_updated_at()
    {
        var taskId = await ResetAndSeedAsync();
        using var factory = new ApiFactory(postgres.ConnectionString);
        using var client = factory.CreateClient();
        var before = await client.GetFromJsonAsync<TaskDto>($"/api/tasks/{taskId}");

        using var response = await client.PutAsJsonAsync($"/api/tasks/{taskId}", new
        {
            title = "  Planejar viagem  ",
            description = "Passagens\nHospedagem",
            priority = "urgent",
            dueDate = "2030-06-15",
        });
        var updated = await response.Content.ReadFromJsonAsync<TaskDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(taskId, updated!.Id);
        Assert.Equal("Planejar viagem", updated.Title);
        Assert.Equal("Passagens\nHospedagem", updated.Description);
        Assert.Equal("urgent", updated.Priority);
        Assert.Equal(new DateOnly(2030, 6, 15), updated.DueDate);
        Assert.Equal(before!.Status, updated.Status);
        Assert.Equal(before.CreatedAt, updated.CreatedAt);
        Assert.True(updated.UpdatedAt > before.UpdatedAt);
        Assert.Equal(updated, await client.GetFromJsonAsync<TaskDto>($"/api/tasks/{taskId}"));
    }

    [Fact]
    public async Task Optional_fields_can_be_removed_without_affecting_other_data()
    {
        var taskId = await ResetAndSeedAsync();
        using var factory = new ApiFactory(postgres.ConnectionString);
        using var client = factory.CreateClient();

        using var response = await client.PutAsJsonAsync($"/api/tasks/{taskId}", new
        {
            title = "Tarefa preservada",
            description = (string?)null,
            priority = "none",
            dueDate = (string?)null,
        });
        var updated = await response.Content.ReadFromJsonAsync<TaskDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Tarefa preservada", updated!.Title);
        Assert.Null(updated.Description);
        Assert.Equal("none", updated.Priority);
        Assert.Null(updated.DueDate);
        Assert.Equal("not_started", updated.Status);
    }

    [Fact]
    public async Task Invalid_update_does_not_change_any_persisted_field()
    {
        var taskId = await ResetAndSeedAsync();
        using var factory = new ApiFactory(postgres.ConnectionString);
        using var client = factory.CreateClient();
        var before = await client.GetFromJsonAsync<TaskDto>($"/api/tasks/{taskId}");

        using var response = await client.PutAsJsonAsync($"/api/tasks/{taskId}", new
        {
            title = "   ",
            description = "Conteúdo que não pode substituir o anterior",
            priority = "urgent",
            dueDate = "2040-12-31",
        });
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDto>();
        var after = await client.GetFromJsonAsync<TaskDto>($"/api/tasks/{taskId}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("title", problem!.Errors);
        Assert.Equal(before, after);
    }

    [Fact]
    public async Task Updating_a_completed_task_preserves_status_and_current_completion()
    {
        var completedAt = DateTimeOffset.UtcNow.AddHours(-1);
        var taskId = await ResetAndSeedAsync(TodoTaskStatus.Completed, completedAt);
        using var factory = new ApiFactory(postgres.ConnectionString);
        using var client = factory.CreateClient();

        using var response = await client.PutAsJsonAsync($"/api/tasks/{taskId}", new
        {
            title = "Concluída, mas refinada",
            description = "Novo contexto",
            priority = "low",
            dueDate = "2020-02-03",
        });
        var updated = await response.Content.ReadFromJsonAsync<TaskDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("completed", updated!.Status);
        Assert.Equal(completedAt, updated.CompletedAt);
        Assert.Equal("Concluída, mas refinada", updated.Title);
        Assert.Equal("Novo contexto", updated.Description);
    }

    [Fact]
    public async Task Updating_an_unknown_task_returns_problem_details()
    {
        await ResetDatabaseAsync();
        using var factory = new ApiFactory(postgres.ConnectionString);
        using var client = factory.CreateClient();

        using var response = await client.PutAsJsonAsync($"/api/tasks/{Guid.NewGuid()}", new
        {
            title = "Inexistente",
            description = (string?)null,
            priority = "none",
            dueDate = (string?)null,
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    private async Task<Guid> ResetAndSeedAsync(
        TodoTaskStatus status = TodoTaskStatus.NotStarted,
        DateTimeOffset? completedAt = null)
    {
        await ResetDatabaseAsync();
        var createdAt = DateTimeOffset.UtcNow.AddDays(-2);
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            CreationIdempotencyKey = Guid.NewGuid(),
            Title = "Tarefa original",
            Description = "Descrição original",
            Status = status,
            Priority = TaskPriority.High,
            DueDate = new DateOnly(2028, 4, 20),
            CreatedAt = createdAt,
            UpdatedAt = createdAt,
            CompletedAt = completedAt,
        };

        await using var db = CreateDbContext();
        db.Tasks.Add(task);
        await db.SaveChangesAsync();
        return task.Id;
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

    private sealed record ValidationProblemDto(Dictionary<string, string[]> Errors, string TraceId);
}
