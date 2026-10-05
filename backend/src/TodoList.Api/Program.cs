using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoList.Api.Common;
using TodoList.Api.Data;
using TodoList.Api.Features.Tasks;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("TodoList")
    ?? "Host=localhost;Port=5432;Database=todolist;Username=todolist;Password=todolist-local";
var allowedOrigin = builder.Configuration["Cors:AllowedOrigin"] ?? "http://localhost:5173";
if (!Uri.TryCreate(allowedOrigin, UriKind.Absolute, out var allowedOriginUri)
    || allowedOriginUri.Scheme is not ("http" or "https")
    || allowedOrigin.Contains('*', StringComparison.Ordinal))
{
    throw new InvalidOperationException("Cors:AllowedOrigin deve ser uma origem HTTP(S) absoluta, sem curingas.");
}

builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = RequestBodyLimitMiddleware.MaximumBodySize);

builder.Services.AddDbContext<TodoListDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] = Activity.Current?.Id
            ?? context.HttpContext.TraceIdentifier;
    };
});
builder.Services.AddOpenApi(options =>
{
    options.AddOperationTransformer((operation, _, _) =>
    {
        if (operation.RequestBody is not null)
        {
            operation.RequestBody = new Microsoft.OpenApi.OpenApiRequestBody
            {
                Content = operation.RequestBody.Content,
                Required = true,
            };
        }

        return Task.CompletedTask;
    });
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(allowedOrigin).AllowAnyHeader().AllowAnyMethod());
});
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database", tags: ["ready"]);

var app = builder.Build();

app.UseExceptionHandler();
app.UseMiddleware<RequestBodyLimitMiddleware>();
app.UseMiddleware<SafeRequestLoggingMiddleware>();
app.UseStatusCodePages(async statusCodeContext =>
{
    var problemDetailsService = statusCodeContext.HttpContext.RequestServices
        .GetRequiredService<IProblemDetailsService>();
    await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
    {
        HttpContext = statusCodeContext.HttpContext,
        ProblemDetails = new ProblemDetails
        {
            Status = statusCodeContext.HttpContext.Response.StatusCode,
            Title = "Recurso não encontrado.",
            Type = "https://httpstatuses.com/404",
        },
    });
});
app.UseCors("Frontend");

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    app.MapOpenApi();
}

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("ready"),
});
app.MapTaskEndpoints();

app.Run();

public partial class Program;
