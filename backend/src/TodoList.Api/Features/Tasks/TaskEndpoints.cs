using TodoList.Api.Features.Tasks.Create;
using TodoList.Api.Features.Tasks.Read;

namespace TodoList.Api.Features.Tasks;

public static class TaskEndpoints
{
    public static IEndpointRouteBuilder MapTaskEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/tasks").WithTags("Tasks");
        group.MapCreateTask();
        group.MapReadTaskEndpoints();
        return endpoints;
    }
}
