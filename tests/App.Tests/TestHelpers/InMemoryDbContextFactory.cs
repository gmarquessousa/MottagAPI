using App.Infrastructure.Persistence;using Microsoft.EntityFrameworkCore;using Microsoft.Extensions.DependencyInjection;using App.Domain.Repositories;using App.Infrastructure.Repositories;using App.Domain.Entities;

namespace App.Tests.TestHelpers;

public static class InMemoryDbContextFactory
{
    public static (AppDbContext ctx, IRepository<T> repo) Create<T>() where T: BaseEntity
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        var provider = services.BuildServiceProvider();
        var ctx = provider.GetRequiredService<AppDbContext>();
        var repo = (IRepository<T>)provider.GetRequiredService<IRepository<T>>();
        return (ctx, repo);
    }
}