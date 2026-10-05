using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoList.Api.Data;

namespace TodoList.Api.Features.Tasks.Status;

public static class TaskStatusEndpoints
{
    public static RouteGroupBuilder MapTaskStatusEndpoints(this RouteGroupBuilder group)
    {
        group.MapPut("/{taskId:guid}/status", ChangeActiveStatusAsync)
            .WithName("changeActiveTaskStatus")
            .WithSummary("Altera uma tarefa entre estados ativos")
            .Produces<TaskResponse>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{taskId:guid}/complete", CompleteAsync)
            .WithName("completeTask")
            .WithSummary("Conclui uma tarefa de forma idempotente")
            .Produces<TaskResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{taskId:guid}/reopen", ReopenAsync)
            .WithName("reopenTask")
            .WithSummary("Reabre uma tarefa no estado ativo anterior")
            .Produces<TaskResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return group;
    }

    private static async Task<IResult> ChangeActiveStatusAsync(
        Guid taskId,
        [FromBody] ChangeActiveStatusRequest request,
        TodoListDbContext db,
        CancellationToken cancellationToken)
    {
        if (!TryParseActiveStatus(request.Status, out var targetStatus))
        {
            return TypedResults.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["status"] = ["O estado deve ser 'not_started', 'in_progress' ou 'blocked'."],
                },
                title: "Um ou mais campos são inválidos.");
        }

        var task = await db.Tasks.SingleOrDefaultAsync(item => item.Id == taskId, cancellationToken);
        if (task is null)
        {
            return NotFound();
        }

        var transition = TaskStateTransitions.ChangeActiveStatus(
            task,
            targetStatus,
            DateTimeOffset.UtcNow);
        if (!transition.Succeeded)
        {
            return Conflict("Uma tarefa concluída deve ser reaberta antes de receber outro estado.");
        }

        await SaveAsync(db, transition, cancellationToken);
        return TypedResults.Ok(TaskResponse.From(task));
    }

    private static async Task<IResult> CompleteAsync(
        Guid taskId,
        TodoListDbContext db,
        CancellationToken cancellationToken)
    {
        var task = await db.Tasks.SingleOrDefaultAsync(item => item.Id == taskId, cancellationToken);
        if (task is null)
        {
            return NotFound();
        }

        var transition = TaskStateTransitions.Complete(task, DateTimeOffset.UtcNow);
        await SaveAsync(db, transition, cancellationToken);
        return TypedResults.Ok(TaskResponse.From(task));
    }

    private static async Task<IResult> ReopenAsync(
        Guid taskId,
        TodoListDbContext db,
        CancellationToken cancellationToken)
    {
        var task = await db.Tasks
            .Include(item => item.StatusEvents)
            .SingleOrDefaultAsync(item => item.Id == taskId, cancellationToken);
        if (task is null)
        {
            return NotFound();
        }

        var transition = TaskStateTransitions.Reopen(task, DateTimeOffset.UtcNow);
        if (!transition.Succeeded)
        {
            var detail = transition.Failure == TaskTransitionFailure.TaskIsNotCompleted
                ? "Somente uma tarefa concluída pode ser reaberta."
                : "Não foi encontrada a transição que originou a conclusão atual.";
            return Conflict(detail);
        }

        await SaveAsync(db, transition, cancellationToken);
        return TypedResults.Ok(TaskResponse.From(task));
    }

    private static async Task SaveAsync(
        TodoListDbContext db,
        TaskTransitionResult transition,
        CancellationToken cancellationToken)
    {
        if (!transition.Changed)
        {
            return;
        }

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private static bool TryParseActiveStatus(string? value, out TodoTaskStatus status)
    {
        status = value switch
        {
            "not_started" => TodoTaskStatus.NotStarted,
            "in_progress" => TodoTaskStatus.InProgress,
            "blocked" => TodoTaskStatus.Blocked,
            _ => default,
        };
        return value is "not_started" or "in_progress" or "blocked";
    }

    private static IResult NotFound() => TypedResults.Problem(
        statusCode: StatusCodes.Status404NotFound,
        title: "Tarefa não encontrada.",
        type: "https://httpstatuses.com/404");

    private static IResult Conflict(string detail) => TypedResults.Problem(
        statusCode: StatusCodes.Status409Conflict,
        title: "A transição de estado não pode ser concluída.",
        detail: detail,
        type: "https://httpstatuses.com/409");
}
