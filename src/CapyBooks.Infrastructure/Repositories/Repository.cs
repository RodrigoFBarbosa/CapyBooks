using CapyBooks.Domain.Common;
using CapyBooks.Domain.Interfaces;
using CapyBooks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CapyBooks.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly CapyBooksDbContext Context;
    protected readonly DbSet<T> DbSet;

    public Repository(CapyBooksDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public virtual Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        DbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        DbSet.Add(entity);
        return Task.CompletedTask;
    }

    public void Update(T entity) => DbSet.Update(entity);

    public void Remove(T entity) => DbSet.Remove(entity);
}
