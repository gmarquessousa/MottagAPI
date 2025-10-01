using App.Application;
using App.Infrastructure.Persistence;
using App.Api.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.EntityFrameworkCore; // Necessário para Database.Migrate()

var builder = WebApplication.CreateBuilder(args);

// Persistence (usa chave DefaultConnection conforme extension method)
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
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Mottag API", Version = "v1", Description = "API simplificada para gestão de Pátios, Motos e Tags." });
    // XML docs de todos os assemblies relevantes (API + Application)
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

// Logs de diagnóstico iniciais
Console.WriteLine($"[Startup][Diag] Environment={app.Environment.EnvironmentName}");
Console.WriteLine($"[Startup][Diag] Swagger:AccessKey set={(string.IsNullOrWhiteSpace(app.Configuration.GetValue<string>("Swagger:AccessKey")) ? "no" : "yes")}");


// Aplica migrations automaticamente em qualquer ambiente (opcional: restringir via config)
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
        // Re-lançar se quiser impedir start quando schema falha:
        // throw;
    }
}

// Swagger sempre habilitado (produção e desenvolvimento). Proteção opcional via AccessKey.
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

// Redirect da raiz para swagger
app.MapGet("/", ctx => { ctx.Response.Redirect("/swagger"); return Task.CompletedTask; });

// Health endpoint simples
app.MapGet("/health", () => Results.Json(new { status = "ok", env = app.Environment.EnvironmentName, timeUtc = DateTime.UtcNow }));

app.MapControllers();

// Fallback explicativo (para evitar 403/404 genérico do front) – só se nenhuma rota casar
app.MapFallback(async ctx =>
{
    ctx.Response.StatusCode = StatusCodes.Status404NotFound;
    ctx.Response.ContentType = "application/json";
    await ctx.Response.WriteAsync("{\"error\":\"Endpoint não encontrado. Veja /swagger\"}");
});

app.Run();
