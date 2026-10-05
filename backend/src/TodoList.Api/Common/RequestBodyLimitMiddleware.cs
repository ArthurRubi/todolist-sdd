using Microsoft.AspNetCore.Mvc;

namespace TodoList.Api.Common;

public sealed class RequestBodyLimitMiddleware(RequestDelegate next)
{
    public const long MaximumBodySize = 64 * 1024;

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.ContentLength is > MaximumBodySize)
        {
            context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
            var problemDetailsService = context.RequestServices.GetRequiredService<IProblemDetailsService>();
            await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = context,
                ProblemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status413PayloadTooLarge,
                    Title = "O corpo da requisição excede o limite permitido.",
                    Type = "https://httpstatuses.com/413",
                },
            });
            return;
        }

        await next(context);
    }
}
