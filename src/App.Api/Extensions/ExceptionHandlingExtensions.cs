using App.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace App.Api.Extensions;

public static class ExceptionHandlingExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var feature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
                var ex = feature?.Error;
                var (status, title) = ex switch
                {
                    NotFoundException => (StatusCodes.Status404NotFound, "Recurso não encontrado"),
                    ConflictException => (StatusCodes.Status409Conflict, "Conflito"),
                    AppValidationException => (StatusCodes.Status400BadRequest, "Erro de validação"),
                    _ => (StatusCodes.Status500InternalServerError, "Erro interno")
                };
                context.Response.StatusCode = status;
                context.Response.ContentType = "application/problem+json";
                var problem = new ProblemDetails
                {
                    Status = status,
                    Title = title,
                    Detail = ex?.Message,
                    Type = "about:blank"
                };
                if (ex is AppValidationException vex)
                {
                    problem.Extensions["errors"] = vex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                }
                await context.Response.WriteAsJsonAsync(problem);
            });
        });
        return app;
    }
}