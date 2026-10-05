using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TodoList.Api.Data;
using TodoList.Api.IntegrationTests.Infrastructure;

namespace TodoList.Api.IntegrationTests.Features.Tasks;

[Collection(PostgresCollection.Name)]
public sealed class CreateAndReadTaskTests(PostgresContainerFixture postgres)
{
    [Fact]
    public async Task Minimal_task_can_be_created_listed_and_retrieved()
    {
        await ResetDatabaseAsync();
        using var factory = new ApiFactory(postgres.ConnectionString);
        using var client = factory.CreateClient();

        using var request = CreateRequest(new { title = "  Comprar café  " });
        using var response = await client.SendAsync(request);
        var created = await response.Content.ReadFromJsonAsync<TaskDto>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(created);
        Assert.Equal("Comprar café", created.Title);
        Assert.Equal("not_started", created.Status);
        Assert.Equal("none", created.Priority);
        Assert.Null(created.Description);
        Assert.Null(created.DueDate);
        Assert.Equal($"/api/tasks/{created.Id}", response.Headers.Location?.OriginalString);

        var active = await client.GetFromJsonAsync<List<TaskDto>>("/api/tasks?view=active");
        var details = await client.GetFromJsonAsync<TaskDto>($"/api/tasks/{created.Id}");

        var activeTask = Assert.Single(active!);
        Assert.Equal(created.Id, activeTask.Id);
        Assert.Equal(created, details);
    }

    [Fact]
    public async Task Detailed_task_accepts_a_past_calendar_date()
    {
        await ResetDatabaseAsync();
        using var factory = new ApiFactory(postgres.ConnectionString);
        using var client = factory.CreateClient();
        using var request = CreateRequest(new
        {
            title = "Enviar relatório",
            description = "Linha um\nLinha dois",
            priority = "urgent",
            dueDate = "2000-01-02",
        });

        using var response = await client.SendAsync(request);
        var task = await response.Content.ReadFromJsonAsync<TaskDto>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("Linha um\nLinha dois", task!.Description);
        Assert.Equal("urgent", task.Priority);
        Assert.Equal(new DateOnly(2000, 1, 2), task.DueDate);
    }

    [Fact]
    public async Task A_task_remains_available_to_a_new_client()
    {
        await ResetDatabaseAsync();
        Guid taskId;
        using (var firstFactory = new ApiFactory(postgres.ConnectionString))
        using (var firstClient = firstFactory.CreateClient())
        using (var request = CreateRequest(new { title = "Persistir entre sessões" }))
        {
            using var response = await firstClient.SendAsync(request);
            taskId = (await response.Content.ReadFromJsonAsync<TaskDto>())!.Id;
        }

        using var secondFactory = new ApiFactory(postgres.ConnectionString);
        using var secondClient = secondFactory.CreateClient();
        var persisted = await secondClient.GetFromJsonAsync<TaskDto>($"/api/tasks/{taskId}");

        Assert.Equal("Persistir entre sessões", persisted!.Title);
    }

    [Fact]
    public async Task Repeating_the_same_idempotency_key_and_content_returns_the_original_task()
    {
        await ResetDatabaseAsync();
        using var factory = new ApiFactory(postgres.ConnectionString);
        using var client = factory.CreateClient();
        var key = Guid.NewGuid();

        using var firstRequest = CreateRequest(new { title = "Única" }, key);
        using var firstResponse = await client.SendAsync(firstRequest);
        var first = await firstResponse.Content.ReadFromJsonAsync<TaskDto>();

        using var replayRequest = CreateRequest(new { title = "Única" }, key);
        using var replayResponse = await client.SendAsync(replayRequest);
        var replay = await replayResponse.Content.ReadFromJsonAsync<TaskDto>();

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, replayResponse.StatusCode);
        Assert.Equal(first, replay);
        Assert.Single((await client.GetFromJsonAsync<List<TaskDto>>("/api/tasks"))!);
    }

    [Fact]
    public async Task Reusing_an_idempotency_key_with_different_content_returns_conflict()
    {
        await ResetDatabaseAsync();
        using var factory = new ApiFactory(postgres.ConnectionString);
        using var client = factory.CreateClient();
        var key = Guid.NewGuid();

        using var firstRequest = CreateRequest(new { title = "Primeiro conteúdo" }, key);
        using var firstResponse = await client.SendAsync(firstRequest);
        using var conflictRequest = CreateRequest(new { title = "Conteúdo diferente" }, key);
        using var conflictResponse = await client.SendAsync(conflictRequest);

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, conflictResponse.StatusCode);
        Assert.Equal("application/problem+json", conflictResponse.Content.Headers.ContentType?.MediaType);
        Assert.Single((await client.GetFromJsonAsync<List<TaskDto>>("/api/tasks"))!);
    }

    [Fact]
    public async Task Invalid_title_returns_validation_problem_without_persisting_a_task()
    {
        await ResetDatabaseAsync();
        using var factory = new ApiFactory(postgres.ConnectionString);
        using var client = factory.CreateClient();
        using var request = CreateRequest(new { title = "   ", description = "Não perder" });

        using var response = await client.SendAsync(request);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDto>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("title", problem!.Errors);
        Assert.False(string.IsNullOrWhiteSpace(problem.TraceId));
        Assert.Empty((await client.GetFromJsonAsync<List<TaskDto>>("/api/tasks"))!);
    }

    private static HttpRequestMessage CreateRequest(object body, Guid? key = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/tasks")
        {
            Content = JsonContent.Create(body),
        };
        request.Headers.Add("Idempotency-Key", (key ?? Guid.NewGuid()).ToString());
        return request;
    }

    private async Task ResetDatabaseAsync()
    {
        await using var db = new TodoListDbContext(new DbContextOptionsBuilder<TodoListDbContext>()
            .UseNpgsql(postgres.ConnectionString)
            .Options);
        await db.Database.MigrateAsync();
        await postgres.ResetAsync();
    }

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
