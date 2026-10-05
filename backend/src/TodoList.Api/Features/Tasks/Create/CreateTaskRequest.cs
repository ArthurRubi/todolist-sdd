namespace TodoList.Api.Features.Tasks.Create;

public sealed record CreateTaskRequest(
    string? Title,
    string? Description = null,
    string? Priority = null,
    DateOnly? DueDate = null);
