using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TodoList.Api.Data;
using TodoList.Api.IntegrationTests.Infrastructure;

namespace TodoList.Api.IntegrationTests.Common;

[Collection(PostgresCollection.Name)]
public sealed class SecurityAndOperationsTests(PostgresContainerFixture postgres) : IAsyncLifetime
{
    private readonly ApiFactory _factory = new(postgres.ConnectionString);
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<TodoListDbContext>().Database.MigrateAsync();
        await postgres.ResetAsync();
        _client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Request_body_above_configured_limit_returns_problem_details()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/tasks")
        {
            Content = JsonContent.Create(new
            {
                title = "Corpo excessivo",
                description = new string('x', 70_000),
            }),
        };
        request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());

        var response = await _client.SendAsync(request);
        var problem = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.NotNull(problem);
        Assert.True(problem.ContainsKey("traceId"));
    }

    [Fact]
    public async Task Cors_does_not_authorize_an_origin_other_than_the_configured_frontend()
    {
        using var request = new HttpRequestMessage(HttpMethod.Options, "/health/live");
        request.Headers.Add("Origin", "https://attacker.example");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        var response = await _client.SendAsync(request);

        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task Every_http_error_contains_a_trace_id()
    {
        var response = await _client.GetAsync("/api/tasks?view=invalid");
        var problem = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problem);
        Assert.True(problem.TryGetValue("traceId", out var traceId));
        Assert.False(string.IsNullOrWhiteSpace(traceId.ToString()));
    }

    [Fact]
    public async Task Liveness_stays_healthy_and_readiness_fails_when_database_is_unavailable()
    {
        await using var unavailableFactory = new ApiFactory(
            "Host=127.0.0.1;Port=1;Database=unavailable;Username=none;Password=none;Timeout=1;Command Timeout=1");
        using var client = unavailableFactory.CreateClient();

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health/live")).StatusCode);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, (await client.GetAsync("/health/ready")).StatusCode);
    }

    [Fact]
    public async Task Operational_log_correlates_request_without_task_content()
    {
        var logs = new TestLogSink();
        await using var loggingFactory = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureLogging(logging => logging.AddProvider(logs)));
        using var client = loggingFactory.CreateClient();
        const string title = "SEGREDO-TITULO-NAO-LOGAR";
        const string description = "SEGREDO-DESCRICAO-NAO-LOGAR";
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/tasks")
        {
            Content = JsonContent.Create(new { title, description }),
        };
        request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var operationLog = Assert.Single(logs.Entries, entry => entry.EventId.Name == "RequestCompleted");
        Assert.Contains("POST /api/tasks", operationLog.Message, StringComparison.Ordinal);
        Assert.DoesNotContain(title, logs.CombinedMessages, StringComparison.Ordinal);
        Assert.DoesNotContain(description, logs.CombinedMessages, StringComparison.Ordinal);
    }
}

internal sealed record TestLogEntry(EventId EventId, string Message);

internal sealed class TestLogSink : ILoggerProvider
{
    private readonly ConcurrentQueue<TestLogEntry> _entries = new();

    public IReadOnlyCollection<TestLogEntry> Entries => _entries.ToArray();

    public string CombinedMessages => string.Join('\n', _entries.Select(entry => entry.Message));

    public ILogger CreateLogger(string categoryName) => new TestLogger(_entries);

    public void Dispose()
    {
    }

    private sealed class TestLogger(ConcurrentQueue<TestLogEntry> entries) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter) =>
            entries.Enqueue(new TestLogEntry(eventId, formatter(state, exception)));
    }
}
