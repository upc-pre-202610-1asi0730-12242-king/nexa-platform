using King.Nexa.Platform.Shared.Domain.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public static class TenantScopedQueryableExtensions
{
    public static IQueryable<TEntity> ApplyTenantScope<TEntity>(this IQueryable<TEntity> query, int? tenantId)
        where TEntity : class, ITenantScoped =>
        tenantId.HasValue
            ? query.Where(entity => EF.Property<int>(entity, nameof(ITenantScoped.TenantId)) == tenantId.Value)
            : query.Where(_ => false);
}

