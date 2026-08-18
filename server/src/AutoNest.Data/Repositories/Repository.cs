using Microsoft.EntityFrameworkCore;

namespace AutoNest.Data.Repositories;

public sealed class Repository<T>(AutoNestDbContext db) : IRepository<T> where T : class
{
    public IQueryable<T> Query()
        => db.Set<T>().AsQueryable();

    public ValueTask<T?> FindAsync(params object[] keys)
        => db.Set<T>().FindAsync(keys);

    public Task AddAsync(T entity, CancellationToken cancellationToken = default)
        => db.Set<T>().AddAsync(entity, cancellationToken).AsTask();

    public void Remove(T entity)
        => db.Set<T>().Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => db.SaveChangesAsync(cancellationToken);
}
