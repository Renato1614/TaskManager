using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Abstractions;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure.Persistence.Repositories;

public abstract class Repository<TEntity>(TaskManagerDbContext dbContext) : IRepository<TEntity>
    where TEntity : class, IEntity
{
    protected TaskManagerDbContext DbContext { get; } = dbContext;
    protected DbSet<TEntity> Set => DbContext.Set<TEntity>();

    public Task AddAsync(TEntity entity, CancellationToken cancellationToken) =>
        Set.AddAsync(entity, cancellationToken).AsTask();

    public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Set.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public void Remove(TEntity entity) => Set.Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        DbContext.SaveChangesAsync(cancellationToken);
}
