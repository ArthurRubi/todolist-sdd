using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TodoList.Api.Data;

public sealed class TodoListDbContextFactory : IDesignTimeDbContextFactory<TodoListDbContext>
{
    public TodoListDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__TodoList")
            ?? "Host=localhost;Port=5432;Database=todolist;Username=todolist;Password=todolist-local";
        var options = new DbContextOptionsBuilder<TodoListDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new TodoListDbContext(options);
    }
}
