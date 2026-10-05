using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using TodoList.Api.Data;

namespace TodoList.Api.Features.Tasks.Create;

public static class CreateTaskEndpoint
{
    public static RouteHandlerBuilder MapCreateTask(this RouteGroupBuilder group) => group
        .MapPost("/", HandleAsync)
        .WithName("createTask")
        .WithSummary("Cria uma tarefa de forma idempotente")
        .Produces<TaskResponse>(StatusCodes.Status201Created)
        .Produces<TaskResponse>(StatusCodes.Status200OK)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .AddOpenApiOperationTransformer((operation, _, _) =>
        {
            operation.Parameters ??= [];
            operation.Parameters.Add(new Microsoft.OpenApi.OpenApiParameter
            {
                Name = "Idempotency-Key",
                In = Microsoft.OpenApi.ParameterLocation.Header,
                Required = true,
                Description = "UUID estável para uma tentativa lógica de criação.",
                Schema = new Microsoft.OpenApi.OpenApiSchema
                {
                    Type = Microsoft.OpenApi.JsonSchemaType.String,
                    Format = "uuid",
                },
            });
            return Task.CompletedTask;
        });

    private static async Task<IResult> HandleAsync(
        [FromBody] CreateTaskRequest request,
        HttpContext httpContext,
        TodoListDbContext db,
        CancellationToken cancellationToken)
    {
        if (!TryGetIdempotencyKey(httpContext, out var idempotencyKey))
        {
            return TypedResults.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["idempotencyKey"] = ["O cabeçalho Idempotency-Key deve conter um UUID válido."],
                },
                title: "Um ou mais campos são inválidos.");
        }

        var validation = TaskInputValidator.Validate(request);
        if (!validation.IsValid)
        {
            return TypedResults.ValidationProblem(
                validation.Errors,
                title: "Um ou mais campos são inválidos.");
        }

        var input = validation.Value!;
        var existing = await db.Tasks
            .AsNoTracking()
            .SingleOrDefaultAsync(task => task.CreationIdempotencyKey == idempotencyKey, cancellationToken);
        if (existing is not null)
        {
            return ReplayResult(existing, input);
        }

        var now = DateTimeOffset.UtcNow;
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            CreationIdempotencyKey = idempotencyKey,
            Title = input.Title,
            Description = input.Description,
            Status = TodoTaskStatus.NotStarted,
            Priority = input.Priority,
            DueDate = input.DueDate,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        db.Tasks.Add(task);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            await transaction.RollbackAsync(cancellationToken);
            db.ChangeTracker.Clear();
            existing = await db.Tasks
                .AsNoTracking()
                .SingleAsync(item => item.CreationIdempotencyKey == idempotencyKey, cancellationToken);
            return ReplayResult(existing, input);
        }

        return TypedResults.Created($"/api/tasks/{task.Id}", TaskResponse.From(task));
    }

    private static IResult ReplayResult(TaskItem existing, NormalizedTaskInput input)
    {
        if (HasSameContent(existing, input))
        {
            return TypedResults.Ok(TaskResponse.From(existing));
        }

        return TypedResults.Problem(
            statusCode: StatusCodes.Status409Conflict,
            title: "A chave de idempotência já foi utilizada com outro conteúdo.",
            type: "https://httpstatuses.com/409");
    }

    private static bool HasSameContent(TaskItem task, NormalizedTaskInput input) =>
        task.Title == input.Title
        && task.Description == input.Description
        && task.Priority == input.Priority
        && task.DueDate == input.DueDate;

    private static bool TryGetIdempotencyKey(HttpContext context, out Guid key) =>
        Guid.TryParse(context.Request.Headers["Idempotency-Key"].ToString(), out key);

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
}
