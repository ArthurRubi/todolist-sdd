using Microsoft.EntityFrameworkCore;
using TodoList.Api.Data;

namespace TodoList.Api.Features.Tasks.Read;

public static class ReadTaskEndpoints
{
    public static RouteGroupBuilder MapReadTaskEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", ListAsync)
            .WithName("listTasks")
            .WithSummary("Lista tarefas ativas ou concluídas")
            .Produces<List<TaskResponse>>()
            .ProducesValidationProblem();

        group.MapGet("/{taskId:guid}", GetAsync)
            .WithName("getTask")
            .WithSummary("Obtém os detalhes essenciais de uma tarefa")
            .Produces<TaskResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> ListAsync(
        string? view,
        TodoListDbContext db,
        CancellationToken cancellationToken)
    {
        var selectedView = view ?? "active";
        if (selectedView is not ("active" or "completed"))
        {
            return TypedResults.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["view"] = ["A visualização deve ser 'active' ou 'completed'."],
                },
                title: "Um ou mais campos são inválidos.");
        }

        IQueryable<TaskItem> query = db.Tasks.AsNoTracking();
        query = selectedView == "completed"
            ? query.Where(task => task.Status == TodoTaskStatus.Completed)
                .OrderByDescending(task => task.CompletedAt)
                .ThenBy(task => task.Id)
            : query.Where(task => task.Status != TodoTaskStatus.Completed)
                .OrderByDescending(task => task.CreatedAt)
                .ThenBy(task => task.Id);

        var tasks = await query.Take(1_000).ToListAsync(cancellationToken);
        return TypedResults.Ok(tasks.Select(TaskResponse.From).ToList());
    }

    private static async Task<IResult> GetAsync(
        Guid taskId,
        TodoListDbContext db,
        CancellationToken cancellationToken)
    {
        var task = await db.Tasks.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == taskId, cancellationToken);

        return task is null
            ? TypedResults.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Tarefa não encontrada.",
                type: "https://httpstatuses.com/404")
            : TypedResults.Ok(TaskResponse.From(task));
    }
}
