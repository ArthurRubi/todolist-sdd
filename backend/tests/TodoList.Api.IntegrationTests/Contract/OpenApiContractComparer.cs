using System.Text.Json;
using YamlDotNet.Serialization;

namespace TodoList.Api.IntegrationTests.Contract;

internal static class OpenApiContractComparer
{
    private static readonly string[] HttpMethods = ["get", "post", "put", "patch", "delete"];

    public static IReadOnlyList<string> Compare(string approvedYaml, string runtimeJson)
    {
        var approved = new DeserializerBuilder().Build()
            .Deserialize<Dictionary<object, object>>(approvedYaml);
        using var runtime = JsonDocument.Parse(runtimeJson);
        var differences = new List<string>();

        CompareOperations(approved, runtime.RootElement, differences);
        CompareSchemas(approved, runtime.RootElement, differences);
        return differences;
    }

    private static void CompareOperations(
        Dictionary<object, object> approved,
        JsonElement runtime,
        List<string> differences)
    {
        var serverPrefix = GetSequence(approved, "servers")
            .Select(item => GetString(GetMap(item), "url"))
            .FirstOrDefault() ?? string.Empty;
        var approvedPaths = GetMap(approved, "paths");
        var runtimePaths = runtime.GetProperty("paths");

        foreach (var (pathKey, pathValue) in approvedPaths)
        {
            var approvedPath = pathKey.ToString()!;
            var runtimePath = NormalizePath(serverPrefix + approvedPath);
            if (!TryGetProperty(runtimePaths, runtimePath, out var runtimePathItem))
            {
                differences.Add($"Rota ausente no OpenAPI runtime: {runtimePath}.");
                continue;
            }

            var approvedPathItem = GetMap(pathValue);
            foreach (var method in HttpMethods.Where(approvedPathItem.ContainsKey))
            {
                if (!TryGetProperty(runtimePathItem, method, out var runtimeOperation))
                {
                    differences.Add($"Operação ausente no runtime: {method.ToUpperInvariant()} {runtimePath}.");
                    continue;
                }

                var approvedOperation = GetMap(approvedPathItem, method);
                CompareValue(
                    $"{method.ToUpperInvariant()} {runtimePath} operationId",
                    GetString(approvedOperation, "operationId"),
                    runtimeOperation.GetProperty("operationId").GetString(),
                    differences);
                CompareParameters(
                    method,
                    runtimePath,
                    approved,
                    approvedPathItem,
                    approvedOperation,
                    runtimeOperation,
                    differences);

                var approvedResponses = GetMap(approvedOperation, "responses").Keys
                    .Select(key => key.ToString()!)
                    .Order()
                    .ToArray();
                var runtimeResponses = runtimeOperation.GetProperty("responses").EnumerateObject()
                    .Select(response => response.Name)
                    .Order()
                    .ToArray();
                if (!approvedResponses.SequenceEqual(runtimeResponses))
                {
                    differences.Add(
                        $"{method.ToUpperInvariant()} {runtimePath} respostas: esperado [{string.Join(", ", approvedResponses)}], runtime [{string.Join(", ", runtimeResponses)}].");
                }

                CompareRequestBody(method, runtimePath, approvedOperation, runtimeOperation, differences);
            }
        }
    }

    private static void CompareRequestBody(
        string method,
        string path,
        Dictionary<object, object> approvedOperation,
        JsonElement runtimeOperation,
        List<string> differences)
    {
        var hasApprovedBody = approvedOperation.ContainsKey("requestBody");
        var hasRuntimeBody = runtimeOperation.TryGetProperty("requestBody", out var runtimeBody);
        if (hasApprovedBody != hasRuntimeBody)
        {
            differences.Add($"{method.ToUpperInvariant()} {path} requestBody diverge.");
            return;
        }

        if (!hasApprovedBody)
        {
            return;
        }

        var approvedBody = GetMap(approvedOperation, "requestBody");
        var expectedRequired = GetBoolean(approvedBody, "required");
        var actualRequired = runtimeBody.TryGetProperty("required", out var required) && required.GetBoolean();
        if (expectedRequired != actualRequired)
        {
            differences.Add($"{method.ToUpperInvariant()} {path} requestBody.required diverge.");
        }

        var approvedSchema = GetMap(GetMap(GetMap(approvedBody, "content"), "application/json"), "schema");
        var runtimeSchema = runtimeBody.GetProperty("content").GetProperty("application/json").GetProperty("schema");
        CompareValue(
            $"{method.ToUpperInvariant()} {path} requestBody schema",
            GetReferenceName(approvedSchema),
            GetReferenceName(runtimeSchema),
            differences);
    }

    private static void CompareParameters(
        string method,
        string path,
        Dictionary<object, object> approvedDocument,
        Dictionary<object, object> approvedPathItem,
        Dictionary<object, object> approvedOperation,
        JsonElement runtimeOperation,
        List<string> differences)
    {
        var approvedParameters = GetSequence(approvedPathItem, "parameters")
            .Concat(GetSequence(approvedOperation, "parameters"))
            .Select(item => ResolveApprovedParameter(approvedDocument, GetMap(item)))
            .Select(parameter => new ParameterShape(
                GetString(parameter, "name")!,
                GetString(parameter, "in")!,
                GetBoolean(parameter, "required"),
                ResolveApprovedSchemaType(approvedDocument, GetMap(parameter, "schema")),
                ResolveApprovedSchemaFormat(approvedDocument, GetMap(parameter, "schema"))))
            .OrderBy(parameter => parameter.Key)
            .ToArray();

        var runtimeParameters = runtimeOperation.TryGetProperty("parameters", out var parameters)
            ? parameters.EnumerateArray().Select(parameter => new ParameterShape(
                parameter.GetProperty("name").GetString()!,
                parameter.GetProperty("in").GetString()!,
                parameter.TryGetProperty("required", out var required) && required.GetBoolean(),
                GetJsonSchemaType(parameter.GetProperty("schema")),
                parameter.GetProperty("schema").TryGetProperty("format", out var format) ? format.GetString() : null))
                .OrderBy(parameter => parameter.Key)
                .ToArray()
            : [];

        if (!approvedParameters.SequenceEqual(runtimeParameters))
        {
            differences.Add(
                $"{method.ToUpperInvariant()} {path} parâmetros: esperado [{string.Join(", ", approvedParameters)}], runtime [{string.Join(", ", runtimeParameters)}].");
        }
    }

    private static void CompareSchemas(
        Dictionary<object, object> approved,
        JsonElement runtime,
        List<string> differences)
    {
        var approvedSchemas = GetMap(GetMap(approved, "components"), "schemas");
        var runtimeSchemas = runtime.GetProperty("components").GetProperty("schemas");
        var runtimeAliases = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Task"] = "TaskResponse",
            ["TaskStatus"] = "TodoTaskStatus",
            ["TaskPriority"] = "TaskPriority",
        };

        foreach (var schemaName in new[] { "Task", "CreateTaskRequest", "UpdateTaskRequest", "ChangeActiveStatusRequest" })
        {
            var runtimeName = runtimeAliases.GetValueOrDefault(schemaName, schemaName);
            if (!runtimeSchemas.TryGetProperty(runtimeName, out var runtimeSchema))
            {
                differences.Add($"Schema ausente no runtime: {schemaName} (esperado como {runtimeName}).");
                continue;
            }

            var approvedSchema = GetMap(approvedSchemas, schemaName);
            var expectedRequired = GetSequence(approvedSchema, "required")
                .Select(value => value.ToString()!)
                .Order()
                .ToArray();
            var actualRequired = runtimeSchema.TryGetProperty("required", out var required)
                ? required.EnumerateArray().Select(value => value.GetString()!).Order().ToArray()
                : [];
            if (!expectedRequired.SequenceEqual(actualRequired))
            {
                differences.Add(
                    $"Schema {schemaName} required: esperado [{string.Join(", ", expectedRequired)}], runtime [{string.Join(", ", actualRequired)}].");
            }

            var expectedProperties = GetMap(approvedSchema, "properties").Keys
                .Select(key => key.ToString()!)
                .Order()
                .ToArray();
            var actualProperties = runtimeSchema.GetProperty("properties").EnumerateObject()
                .Select(property => property.Name)
                .Order()
                .ToArray();
            if (!expectedProperties.SequenceEqual(actualProperties))
            {
                differences.Add(
                    $"Schema {schemaName} propriedades: esperado [{string.Join(", ", expectedProperties)}], runtime [{string.Join(", ", actualProperties)}].");
            }
        }
    }

    private static string NormalizePath(string path) => path.Length > 1 ? path.TrimEnd('/') : path;

    private static void CompareValue(
        string label,
        string? expected,
        string? actual,
        List<string> differences)
    {
        if (!string.Equals(expected, actual, StringComparison.Ordinal))
        {
            differences.Add($"{label}: esperado '{expected}', runtime '{actual}'.");
        }
    }

    private static bool TryGetProperty(JsonElement element, string name, out JsonElement value)
    {
        if (element.TryGetProperty(name, out value))
        {
            return true;
        }

        return element.TryGetProperty(name + "/", out value);
    }

    private static Dictionary<object, object> GetMap(Dictionary<object, object> map, string key) =>
        GetMap(map[key]);

    private static Dictionary<object, object> GetMap(object value) =>
        (Dictionary<object, object>)value;

    private static List<object> GetSequence(Dictionary<object, object> map, string key) =>
        map.TryGetValue(key, out var value) ? (List<object>)value : [];

    private static string? GetString(Dictionary<object, object> map, string key) =>
        map.TryGetValue(key, out var value) ? value?.ToString() : null;

    private static bool GetBoolean(Dictionary<object, object> map, string key) =>
        map.TryGetValue(key, out var value)
        && bool.TryParse(value?.ToString(), out var parsed)
        && parsed;

    private static Dictionary<object, object> ResolveApprovedParameter(
        Dictionary<object, object> document,
        Dictionary<object, object> parameter)
    {
        var reference = GetString(parameter, "$ref");
        return reference is null
            ? parameter
            : GetMap(GetMap(GetMap(document, "components"), "parameters"), reference.Split('/').Last());
    }

    private static string? ResolveApprovedSchemaType(
        Dictionary<object, object> document,
        Dictionary<object, object> schema)
    {
        var reference = GetString(schema, "$ref");
        return reference is null
            ? GetString(schema, "type")
            : GetString(GetMap(GetMap(GetMap(document, "components"), "schemas"), reference.Split('/').Last()), "type");
    }

    private static string? ResolveApprovedSchemaFormat(
        Dictionary<object, object> document,
        Dictionary<object, object> schema)
    {
        var reference = GetString(schema, "$ref");
        return reference is null
            ? GetString(schema, "format")
            : GetString(GetMap(GetMap(GetMap(document, "components"), "schemas"), reference.Split('/').Last()), "format");
    }

    private static string? GetJsonSchemaType(JsonElement schema)
    {
        if (!schema.TryGetProperty("type", out var type))
        {
            return null;
        }

        return type.ValueKind == JsonValueKind.Array
            ? string.Join('|', type.EnumerateArray().Select(value => value.GetString()).Where(value => value != "null"))
            : type.GetString();
    }

    private static string? GetReferenceName(Dictionary<object, object> schema) =>
        GetString(schema, "$ref")?.Split('/').Last();

    private static string? GetReferenceName(JsonElement schema) =>
        schema.TryGetProperty("$ref", out var reference) ? reference.GetString()?.Split('/').Last() : null;

    private sealed record ParameterShape(
        string Name,
        string Location,
        bool Required,
        string? Type,
        string? Format)
    {
        public string Key => $"{Location}:{Name}";

        public override string ToString() => $"{Key} required={Required} {Type}/{Format}";
    }
}
