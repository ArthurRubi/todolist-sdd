using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TodoList.Api.Features.Tasks;

namespace TodoList.Api.Data;

public sealed class TodoListDbContext(DbContextOptions<TodoListDbContext> options) : DbContext(options)
{
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    public DbSet<TaskStatusEvent> TaskStatusEvents => Set<TaskStatusEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var statusConverter = new ValueConverter<TodoTaskStatus, string>(
            status => status.ToStorageValue(),
            value => TodoTaskStatusStorage.FromStorageValue(value));
        var priorityConverter = new ValueConverter<TaskPriority, string>(
            priority => priority.ToStorageValue(),
            value => TaskPriorityStorage.FromStorageValue(value));

        modelBuilder.Entity<TaskItem>(task =>
        {
            task.ToTable("tasks", table =>
            {
                table.HasCheckConstraint("ck_tasks_title_length", "char_length(title) BETWEEN 1 AND 200");
                table.HasCheckConstraint("ck_tasks_description_length", "description IS NULL OR char_length(description) <= 10000");
                table.HasCheckConstraint("ck_tasks_status", "status IN ('not_started', 'in_progress', 'blocked', 'completed')");
                table.HasCheckConstraint("ck_tasks_priority", "priority IN ('none', 'low', 'medium', 'high', 'urgent')");
                table.HasCheckConstraint("ck_tasks_completion", "(status = 'completed' AND completed_at IS NOT NULL) OR (status <> 'completed' AND completed_at IS NULL)");
                table.HasCheckConstraint("ck_tasks_updated_at", "updated_at >= created_at");
            });

            task.HasKey(item => item.Id).HasName("pk_tasks");
            task.Property(item => item.Id).HasColumnName("id").ValueGeneratedNever();
            task.Property(item => item.CreationIdempotencyKey).HasColumnName("creation_idempotency_key");
            task.Property(item => item.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
            task.Property(item => item.Description).HasColumnName("description").HasMaxLength(10000);
            task.Property(item => item.Status).HasColumnName("status").HasMaxLength(20).HasConversion(statusConverter).IsRequired();
            task.Property(item => item.Priority).HasColumnName("priority").HasMaxLength(10).HasConversion(priorityConverter).IsRequired();
            task.Property(item => item.DueDate).HasColumnName("due_date").HasColumnType("date");
            task.Property(item => item.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
            task.Property(item => item.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");
            task.Property(item => item.CompletedAt).HasColumnName("completed_at").HasColumnType("timestamp with time zone");
            task.HasIndex(item => item.CreationIdempotencyKey).IsUnique().HasDatabaseName("ux_tasks_creation_idempotency_key");
            task.HasIndex(item => new { item.Status, item.CreatedAt }).HasDatabaseName("ix_tasks_status_created_at");
            task.HasIndex(item => item.CompletedAt).HasFilter("status = 'completed'").HasDatabaseName("ix_tasks_completed_at");
        });

        modelBuilder.Entity<TaskStatusEvent>(statusEvent =>
        {
            statusEvent.ToTable("task_status_events", table =>
            {
                table.HasCheckConstraint("ck_task_status_events_from_status", "from_status IN ('not_started', 'in_progress', 'blocked', 'completed')");
                table.HasCheckConstraint("ck_task_status_events_to_status", "to_status IN ('not_started', 'in_progress', 'blocked', 'completed')");
                table.HasCheckConstraint("ck_task_status_events_changed", "from_status <> to_status");
            });

            statusEvent.HasKey(item => item.Id).HasName("pk_task_status_events");
            statusEvent.Property(item => item.Id).HasColumnName("id").ValueGeneratedNever();
            statusEvent.Property(item => item.TaskId).HasColumnName("task_id");
            statusEvent.Property(item => item.FromStatus).HasColumnName("from_status").HasMaxLength(20).HasConversion(statusConverter).IsRequired();
            statusEvent.Property(item => item.ToStatus).HasColumnName("to_status").HasMaxLength(20).HasConversion(statusConverter).IsRequired();
            statusEvent.Property(item => item.OccurredAt).HasColumnName("occurred_at").HasColumnType("timestamp with time zone");
            statusEvent.HasIndex(item => new { item.TaskId, item.OccurredAt, item.Id }).HasDatabaseName("ix_task_status_events_task_occurred_at");
            statusEvent.HasOne(item => item.Task)
                .WithMany(task => task.StatusEvents)
                .HasForeignKey(item => item.TaskId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_task_status_events_tasks");
        });
    }
}
