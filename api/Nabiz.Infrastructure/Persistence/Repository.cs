using Microsoft.EntityFrameworkCore;
using Nabiz.Application.Abstractions;

namespace Nabiz.Infrastructure.Persistence;

public class Repository<T>(NabizDbContext db) : IRepository<T> where T : class
{
    private readonly DbSet<T> _set = db.Set<T>();

    public IQueryable<T> Query => _set;
    public async Task<T?> FindAsync(object id, CancellationToken ct = default) => await _set.FindAsync([id], ct);
    public void Add(T entity) => _set.Add(entity);
    public void AddRange(params T[] entities) => _set.AddRange(entities);
    public void Remove(T entity) => _set.Remove(entity);
    public void RemoveRange(IEnumerable<T> entities) => _set.RemoveRange(entities);
}
