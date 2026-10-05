using System.Diagnostics;
using TodoList.Api.IntegrationTests.Infrastructure;

namespace TodoList.Api.IntegrationTests.Contract;

[Collection(PostgresCollection.Name)]
public sealed class OpenApiContractTests(PostgresContainerFixture postgres)
{
    [Fact]
    public async Task Runtime_openapi_matches_the_approved_contract_semantically()
    {
        await using var factory = new ApiFactory(postgres.ConnectionString);
        using var client = factory.CreateClient();
        var runtimeJson = await client.GetStringAsync("/openapi/v1.json");
        var approvedYaml = await File.ReadAllTextAsync(RepositoryPaths.ApprovedContract);

        var differences = OpenApiContractComparer.Compare(approvedYaml, runtimeJson);

        Assert.True(differences.Count == 0, string.Join(Environment.NewLine, differences));
    }

    [Fact]
    public void Generated_types_are_regenerated_without_drift()
    {
        var startInfo = new ProcessStartInfo("npm")
        {
            WorkingDirectory = RepositoryPaths.Root,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
        };
        startInfo.ArgumentList.Add("run");
        startInfo.ArgumentList.Add("check:api");
        startInfo.ArgumentList.Add("--prefix");
        startInfo.ArgumentList.Add("frontend");
        using var process = Process.Start(startInfo)!;
        var standardOutput = process.StandardOutput.ReadToEnd();
        var standardError = process.StandardError.ReadToEnd();
        process.WaitForExit();

        Assert.True(
            process.ExitCode == 0,
            $"Verificação de tipos falhou.{Environment.NewLine}{standardOutput}{Environment.NewLine}{standardError}");
    }
}

internal static class RepositoryPaths
{
    public static string Root { get; } = FindRoot();

    public static string ApprovedContract => Path.Combine(
        Root,
        "docs",
        "specs",
        "001-essential-task",
        "contracts",
        "tasks-api.openapi.yaml");

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "global.json")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException("A raiz do repositório não foi encontrada.");
    }
}
