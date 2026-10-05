namespace TodoList.Api.Features.Tasks.Update;

public sealed record UpdateTaskRequest(
    string? Title,
    string? Description,
    string? Priority,
    DateOnly? DueDate);
