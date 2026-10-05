using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoList.Api.Data;
using TodoList.Api.Features.Tasks.Create;

namespace TodoList.Api.Features.Tasks.Update;

public static class UpdateTaskEndpoint
{
    public static RouteHandlerBuilder MapUpdateTask(this RouteGroupBuilder group) => group
        .MapPut("/{taskId:guid}", HandleAsync)
        .WithName("updateTask")
        .WithSummary("Substitui atomicamente os campos editáveis")
        .Produces<TaskResponse>()
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> HandleAsync(
        Guid taskId,
        [FromBody] UpdateTaskRequest request,
        TodoListDbContext db,
        CancellationToken cancellationToken)
    {
        var validation = TaskInputValidator.ValidateForUpdate(
            request.Title,
            request.Description,
            request.Priority,
            request.DueDate);
        if (!validation.IsValid)
        {
            return TypedResults.ValidationProblem(
                validation.Errors,
                title: "Um ou mais campos são inválidos.");
        }

        var task = await db.Tasks.SingleOrDefaultAsync(item => item.Id == taskId, cancellationToken);
        if (task is null)
        {
            return TypedResults.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Tarefa não encontrada.",
                type: "https://httpstatuses.com/404");
        }

        var input = validation.Value!;
        if (HasChanges(task, input))
        {
            task.Title = input.Title;
            task.Description = input.Description;
            task.Priority = input.Priority;
            task.DueDate = input.DueDate;
            task.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
        }

        return TypedResults.Ok(TaskResponse.From(task));
    }

    private static bool HasChanges(TaskItem task, NormalizedTaskInput input) =>
        task.Title != input.Title
        || task.Description != input.Description
        || task.Priority != input.Priority
        || task.DueDate != input.DueDate;
}
