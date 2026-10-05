using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TodoList.Api.Data;
using TodoList.Api.IntegrationTests.Infrastructure;

namespace TodoList.Api.IntegrationTests.Common;

[Collection(PostgresCollection.Name)]
public sealed class ApplicationInfrastructureTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _postgres;
    private ApiFactory _factory = null!;
    private HttpClient _client = null!;

    public ApplicationInfrastructureTests(PostgresContainerFixture postgres)
    {
        _postgres = postgres;
    }

    public async Task InitializeAsync()
    {
        _factory = new ApiFactory(_postgres.ConnectionString);
        using var scope = _factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<TodoListDbContext>().Database.MigrateAsync();
        await _postgres.ResetAsync();
        _client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Liveness_and_readiness_are_healthy_with_database_available()
    {
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/health/live")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/health/ready")).StatusCode);
    }

    [Fact]
    public async Task Unknown_route_returns_problem_details_with_trace_id()
    {
        var response = await _client.GetAsync("/route-that-does-not-exist");
        var problem = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.NotNull(problem);
        Assert.True(problem.ContainsKey("traceId"));
    }

    [Fact]
    public async Task Cors_preflight_allows_only_the_configured_frontend_origin()
    {
        using var request = new HttpRequestMessage(HttpMethod.Options, "/health/live");
        request.Headers.Add("Origin", "http://localhost:5173");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("http://localhost:5173", response.Headers.GetValues("Access-Control-Allow-Origin").Single());
    }
}
