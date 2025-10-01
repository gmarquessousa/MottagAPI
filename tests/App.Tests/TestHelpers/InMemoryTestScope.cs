using App.Domain.Entities;
using App.Domain.Repositories;
using App.Infrastructure.Persistence;
using App.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace App.Tests.TestHelpers;

public class InMemoryTestScope : IAsyncDisposable
{
    private readonly ServiceProvider _root;
    private readonly IServiceScope _scope;
    public AppDbContext Context { get; }

    private InMemoryTestScope(ServiceProvider root, IServiceScope scope)
    {
        _root = root;
        _scope = scope;
        Context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }

    public static InMemoryTestScope Create(string? dbName = null)
    {
        dbName ??= Guid.NewGuid().ToString();
        var services = new ServiceCollection();
    services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName), ServiceLifetime.Singleton, ServiceLifetime.Singleton);
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        var root = services.BuildServiceProvider();
        var scope = root.CreateScope();
        return new InMemoryTestScope(root, scope);
    }

    public IRepository<T> GetRepo<T>() where T: BaseEntity => _scope.ServiceProvider.GetRequiredService<IRepository<T>>();

    public async Task AddAndSaveAsync(params BaseEntity[] entities)
    {
        foreach (var e in entities)
        {
            Context.Add(e);
        }
        await Context.SaveChangesAsync();
    }

    public async ValueTask DisposeAsync()
    {
        _scope.Dispose();
        await _root.DisposeAsync();
    }
}