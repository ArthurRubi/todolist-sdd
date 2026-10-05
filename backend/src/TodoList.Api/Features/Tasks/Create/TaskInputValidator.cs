namespace TodoList.Api.Features.Tasks.Create;

public static class TaskInputValidator
{
    public static TaskInputValidationResult Validate(CreateTaskRequest request) => ValidateFields(
        request.Title,
        request.Description,
        request.Priority,
        request.DueDate,
        priorityIsRequired: false);

    public static TaskInputValidationResult ValidateForUpdate(
        string? title,
        string? description,
        string? priority,
        DateOnly? dueDate) => ValidateFields(
            title,
            description,
            priority,
            dueDate,
            priorityIsRequired: true);

    private static TaskInputValidationResult ValidateFields(
        string? rawTitle,
        string? description,
        string? rawPriority,
        DateOnly? dueDate,
        bool priorityIsRequired)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        var title = rawTitle?.Trim() ?? string.Empty;

        if (title.Length == 0)
        {
            errors["title"] = ["O título deve ser informado."];
        }
        else if (title.Length > 200)
        {
            errors["title"] = ["O título deve ter no máximo 200 caracteres."];
        }

        if (description?.Length > 10_000)
        {
            errors["description"] = ["A descrição deve ter no máximo 10.000 caracteres."];
        }

        var priority = priorityIsRequired && rawPriority is null
            ? null
            : ParsePriority(rawPriority);
        if (priority is null)
        {
            errors["priority"] = ["A prioridade informada é inválida."];
        }

        if (errors.Count > 0)
        {
            return new TaskInputValidationResult(null, errors);
        }

        return new TaskInputValidationResult(
            new NormalizedTaskInput(
                title,
                string.IsNullOrEmpty(description) ? null : description,
                priority!.Value,
                dueDate),
            errors);
    }

    private static TaskPriority? ParsePriority(string? priority) => priority switch
    {
        null or "" or "none" => TaskPriority.None,
        "low" => TaskPriority.Low,
        "medium" => TaskPriority.Medium,
        "high" => TaskPriority.High,
        "urgent" => TaskPriority.Urgent,
        _ => null,
    };
}

public sealed record NormalizedTaskInput(
    string Title,
    string? Description,
    TaskPriority Priority,
    DateOnly? DueDate);

public sealed record TaskInputValidationResult(
    NormalizedTaskInput? Value,
    Dictionary<string, string[]> Errors)
{
    public bool IsValid => Errors.Count == 0;
}
