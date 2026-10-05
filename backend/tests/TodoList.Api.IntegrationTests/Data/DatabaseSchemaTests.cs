using Microsoft.EntityFrameworkCore;
using Npgsql;
using TodoList.Api.Data;
using TodoList.Api.Features.Tasks;
using TodoList.Api.IntegrationTests.Infrastructure;

namespace TodoList.Api.IntegrationTests.Data;

[Collection(PostgresCollection.Name)]
public sealed class DatabaseSchemaTests(PostgresContainerFixture postgres) : IAsyncLifetime
{
    private TodoListDbContext _dbContext = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<TodoListDbContext>()
            .UseNpgsql(postgres.ConnectionString)
            .Options;

        _dbContext = new TodoListDbContext(options);
        await _dbContext.Database.MigrateAsync();
        await postgres.ResetAsync();
    }

    public async Task DisposeAsync() => await _dbContext.DisposeAsync();

    [Fact]
    public async Task Initial_migration_creates_tasks_and_status_events()
    {
        await using var connection = new NpgsqlConnection(postgres.ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT table_name
            FROM information_schema.tables
            WHERE table_schema = 'public'
              AND table_name IN ('tasks', 'task_status_events')
            ORDER BY table_name;
            """;

        await using var reader = await command.ExecuteReaderAsync();
        var tables = new List<string>();
        while (await reader.ReadAsync())
        {
            tables.Add(reader.GetString(0));
        }

        Assert.Equal(["task_status_events", "tasks"], tables);
    }

    [Fact]
    public async Task Due_date_round_trips_without_time_zone_conversion()
    {
        var task = NewTask(Guid.NewGuid(), new DateOnly(2026, 1, 20));
        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        var persisted = await _dbContext.Tasks.SingleAsync(item => item.Id == task.Id);

        Assert.Equal(new DateOnly(2026, 1, 20), persisted.DueDate);
        Assert.Equal(TimeSpan.Zero, persisted.CreatedAt.Offset);
    }

    [Fact]
    public async Task Creation_idempotency_key_is_unique()
    {
        var key = Guid.NewGuid();
        _dbContext.Tasks.AddRange(NewTask(key), NewTask(key));

        await Assert.ThrowsAsync<DbUpdateException>(() => _dbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task Rolling_back_task_and_event_leaves_neither_record()
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        var task = NewTask(Guid.NewGuid());
        _dbContext.Tasks.Add(task);
        _dbContext.TaskStatusEvents.Add(new TaskStatusEvent
        {
            Id = Guid.NewGuid(),
            TaskId = task.Id,
            FromStatus = TodoTaskStatus.NotStarted,
            ToStatus = TodoTaskStatus.InProgress,
            OccurredAt = DateTimeOffset.UtcNow,
        });
        await _dbContext.SaveChangesAsync();
        await transaction.RollbackAsync();
        _dbContext.ChangeTracker.Clear();

        Assert.Empty(await _dbContext.Tasks.ToListAsync());
        Assert.Empty(await _dbContext.TaskStatusEvents.ToListAsync());
    }

    private static TaskItem NewTask(Guid idempotencyKey, DateOnly? dueDate = null)
    {
        var now = DateTimeOffset.UtcNow;
        return new TaskItem
        {
            Id = Guid.NewGuid(),
            CreationIdempotencyKey = idempotencyKey,
            Title = "Tarefa de teste",
            Status = TodoTaskStatus.NotStarted,
            Priority = TaskPriority.None,
            DueDate = dueDate,
            CreatedAt = now,
            UpdatedAt = now,
        };
    }
}
