using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using King.Nexa.Platform.TenantManagement.Domain.Model.Aggregates;
using King.Nexa.Platform.TenantManagement.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace King.Nexa.Platform.TenantManagement.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class TenantRepository(AppDbContext context) : BaseRepository<Tenant>(context), ITenantRepository
{
    public Task<Tenant?> FindBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        Context.Set<Tenant>().FirstOrDefaultAsync(tenant => tenant.Slug == slug.ToLower(), cancellationToken);
}
