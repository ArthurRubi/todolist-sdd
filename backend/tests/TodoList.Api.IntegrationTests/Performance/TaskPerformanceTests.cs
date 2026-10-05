using System.Diagnostics;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TodoList.Api.Data;
using TodoList.Api.Features.Tasks;
using TodoList.Api.IntegrationTests.Infrastructure;
using Xunit.Abstractions;

namespace TodoList.Api.IntegrationTests.Performance;

[Collection(PostgresCollection.Name)]
public sealed class TaskPerformanceTests(
    PostgresContainerFixture postgres,
    ITestOutputHelper output) : IAsyncLifetime
{
    private readonly ApiFactory _factory = new(postgres.ConnectionString);
    private HttpClient _client = null!;
    private List<Guid> _seededIds = [];

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TodoListDbContext>();
        await db.Database.MigrateAsync();
        await postgres.ResetAsync();

        var now = DateTimeOffset.UtcNow;
        var seeded = Enumerable.Range(1, 966).Select(index => new TaskItem
        {
            Id = Guid.NewGuid(),
            CreationIdempotencyKey = Guid.NewGuid(),
            Title = $"Carga {index:0000}",
            Status = TodoTaskStatus.NotStarted,
            Priority = TaskPriority.None,
            CreatedAt = now.AddMilliseconds(-index),
            UpdatedAt = now.AddMilliseconds(-index),
        }).ToList();
        db.Tasks.AddRange(seeded);
        await db.SaveChangesAsync();
        _seededIds = seeded.Select(task => task.Id).ToList();
        _client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Create_update_and_status_operations_have_p95_below_one_second_at_1000_tasks()
    {
        var durations = new List<double>(100);

        for (var index = 0; index < 34; index++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/tasks")
            {
                Content = JsonContent.Create(new { title = $"Criada em carga {index:00}" }),
            };
            request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
            durations.Add(await MeasureSuccessfulAsync(request));
        }

        for (var index = 0; index < 33; index++)
        {
            var taskId = _seededIds[index];
            using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/tasks/{taskId}")
            {
                Content = JsonContent.Create(new
                {
                    title = $"Carga atualizada {index:00}",
                    description = (string?)null,
                    priority = "none",
                    dueDate = (string?)null,
                }),
            };
            durations.Add(await MeasureSuccessfulAsync(request));
        }

        for (var index = 0; index < 33; index++)
        {
            var taskId = _seededIds[index + 33];
            using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/tasks/{taskId}/status")
            {
                Content = JsonContent.Create(new { status = "in_progress" }),
            };
            durations.Add(await MeasureSuccessfulAsync(request));
        }

        var p95 = Percentile95(durations);
        output.WriteLine($"Operações medidas: {durations.Count}; p95: {p95:F2} ms; máximo: {durations.Max():F2} ms.");

        Assert.Equal(100, durations.Count);
        Assert.True(p95 <= 1_000, $"p95 esperado <= 1000 ms, observado {p95:F2} ms.");
        Assert.Equal(1_000, await CountTasksAsync());
    }

    private async Task<double> MeasureSuccessfulAsync(HttpRequestMessage request)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.SendAsync(request);
        stopwatch.Stop();
        response.EnsureSuccessStatusCode();
        return stopwatch.Elapsed.TotalMilliseconds;
    }

    private async Task<int> CountTasksAsync()
    {
        using var scope = _factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<TodoListDbContext>().Tasks.CountAsync();
    }

    private static double Percentile95(IEnumerable<double> values)
    {
        var ordered = values.Order().ToArray();
        return ordered[(int)Math.Ceiling(ordered.Length * 0.95) - 1];
    }
}
