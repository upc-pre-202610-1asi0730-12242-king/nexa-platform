using King.Nexa.Platform.Shared.Domain.Repositories;
using King.Nexa.Platform.Shared.Domain.Model.Entities;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Microsoft.EntityFrameworkCore;

namespace King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class BaseRepository<TEntity>(AppDbContext context) : IBaseRepository<TEntity> where TEntity : class
{
    protected readonly AppDbContext Context = context;

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) =>
        await Context.Set<TEntity>().AddAsync(entity, cancellationToken);

    public virtual async Task<TEntity?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        IsTenantScoped
            ? throw TenantScopedGenericReadException()
            : await Context.Set<TEntity>().FindAsync([id], cancellationToken);

    public virtual async Task<IEnumerable<TEntity>> ListAsync(CancellationToken cancellationToken = default) =>
        IsTenantScoped
            ? throw TenantScopedGenericReadException()
            : await Context.Set<TEntity>().ToListAsync(cancellationToken);

    public virtual void Update(TEntity entity) => Context.Set<TEntity>().Update(entity);

    public virtual void Remove(TEntity entity) => Context.Set<TEntity>().Remove(entity);

    private static bool IsTenantScoped => typeof(ITenantScoped).IsAssignableFrom(typeof(TEntity));

    private static InvalidOperationException TenantScopedGenericReadException() =>
        new("Tenant-scoped entities must be read through repositories that apply the current tenant/workspace scope.");
}
