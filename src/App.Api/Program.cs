using App.Application;
using App.Infrastructure.Persistence;
using App.Api.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAppPersistence(builder.Configuration);
builder.Services.AddApplicationLayer();

builder.Services.AddApiVersioning(o =>
{
    o.AssumeDefaultVersionWhenUnspecified = true;
    o.DefaultApiVersion = new ApiVersion(1,0);
    o.ReportApiVersions = true;
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Mottag API", Version = "v1", Description = "API para gestão de Pátios, Motos e Tags." });
    var baseDir = AppContext.BaseDirectory;
    foreach (var xml in new[] { "App.Api.xml", "App.Application.xml" })
    {
        var path = Path.Combine(baseDir, xml);
        if (File.Exists(path)) c.IncludeXmlComments(path, includeControllerXmlComments: true);
    }
    c.ExampleFilters();
});

builder.Services.AddSwaggerExamplesFromAssemblies(Assembly.GetExecutingAssembly());

var app = builder.Build();

app.UseGlobalExceptionHandler();

Console.WriteLine($"[Startup][Diag] Environment={app.Environment.EnvironmentName}");
Console.WriteLine($"[Startup][Diag] Swagger:AccessKey set={(string.IsNullOrWhiteSpace(app.Configuration.GetValue<string>("Swagger:AccessKey")) ? "no" : "yes")}");


using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<App.Infrastructure.Persistence.AppDbContext>();
        db.Database.Migrate();
        Console.WriteLine("[Startup][Migrate] Banco atualizado");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Startup][Migrate][ERRO] {ex.Message}");
    }
}

var accessKey = app.Configuration.GetValue<string>("Swagger:AccessKey");
if (!string.IsNullOrWhiteSpace(accessKey))
{
    app.Use(async (ctx, next) =>
    {
        if (ctx.Request.Path.StartsWithSegments("/swagger"))
        {
            if (!ctx.Request.Headers.TryGetValue("X-Swagger-Key", out var provided) || provided != accessKey)
            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await ctx.Response.WriteAsync("Swagger unauthorized.");
                return;
            }
        }
        await next();
    });
}
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mottag API v1"));

app.MapGet("/", ctx => { ctx.Response.Redirect("/swagger"); return Task.CompletedTask; });

app.MapGet("/health", () => Results.Json(new { status = "ok", env = app.Environment.EnvironmentName, timeUtc = DateTime.UtcNow }));

app.MapControllers();

app.MapFallback(async ctx =>
{
    ctx.Response.StatusCode = StatusCodes.Status404NotFound;
    ctx.Response.ContentType = "application/json";
    await ctx.Response.WriteAsync("{\"error\":\"Endpoint não encontrado. Veja /swagger\"}");
});

app.Run();
