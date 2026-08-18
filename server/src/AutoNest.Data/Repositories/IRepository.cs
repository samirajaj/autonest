namespace AutoNest.Data.Repositories;

public interface IRepository<T> where T : class
{
    IQueryable<T> Query();
    ValueTask<T?> FindAsync(params object[] keys);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Remove(T entity);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
