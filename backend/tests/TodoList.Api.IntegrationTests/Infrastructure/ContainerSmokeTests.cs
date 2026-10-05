namespace TodoList.Api.IntegrationTests.Infrastructure;

[Collection(PostgresCollection.Name)]
public sealed class ContainerSmokeTests(PostgresContainerFixture postgres)
{
    [Fact]
    public async Task PostgreSql_container_accepts_connections()
    {
        await postgres.ResetAsync();

        Assert.Contains("todolist_tests", postgres.ConnectionString, StringComparison.Ordinal);
    }
}
