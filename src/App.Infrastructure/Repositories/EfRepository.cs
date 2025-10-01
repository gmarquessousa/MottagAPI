using App.Domain.Entities;
using App.Domain.Repositories;
using App.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Repositories;

public class EfRepository<T> : IRepository<T> where T : BaseEntity
{
    private readonly AppDbContext _ctx;
    private readonly DbSet<T> _set;
    public EfRepository(AppDbContext ctx)
    {
        _ctx = ctx;
        _set = ctx.Set<T>();
    }

    public async Task AddAsync(T entity, CancellationToken ct = default) => await _set.AddAsync(entity, ct);

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default) => await _set.FirstOrDefaultAsync(e => e.Id == id, ct);

    public IQueryable<T> Query() => _set.AsQueryable();

    public void Update(T entity) => _set.Update(entity);

    public void Remove(T entity) => _set.Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _ctx.SaveChangesAsync(ct);
}
