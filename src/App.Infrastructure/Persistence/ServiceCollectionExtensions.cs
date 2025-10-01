using App.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var cs = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(cs, opt =>
            {
                opt.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
            });
        });
        services.AddScoped(typeof(App.Domain.Repositories.IRepository<>), typeof(App.Infrastructure.Repositories.EfRepository<>));
        return services;
    }
}
