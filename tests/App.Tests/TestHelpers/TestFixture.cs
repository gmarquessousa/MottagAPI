using App.Application.Mappings;
using App.Domain.Repositories;
using App.Infrastructure.Persistence;
using App.Infrastructure.Repositories;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace App.Tests.TestHelpers;

public sealed class TestFixture : IAsyncDisposable
{
    public AppDbContext Db { get; }
    public IMapper Mapper { get; }
    public IRepositoryFactory RepoFactory { get; }

    public TestFixture()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"testdb_{Guid.NewGuid()}")
            .EnableSensitiveDataLogging()
            .Options;
        Db = new AppDbContext(opts);

        var cfg = new MapperConfiguration(c => c.AddProfile<ApplicationProfile>());
        Mapper = cfg.CreateMapper();

        RepoFactory = new RepositoryFactory(Db);
    }

    public async ValueTask DisposeAsync()
    {
        await Db.Database.EnsureDeletedAsync();
        await Db.DisposeAsync();
    }
}

public interface IRepositoryFactory
{
    IRepository<T> Get<T>() where T: App.Domain.Entities.BaseEntity; // alinhar constraint
}

public class RepositoryFactory : IRepositoryFactory
{
    private readonly AppDbContext _ctx;
    public RepositoryFactory(AppDbContext ctx) => _ctx = ctx;
    public IRepository<T> Get<T>() where T: App.Domain.Entities.BaseEntity => new EfRepository<T>(_ctx);
}