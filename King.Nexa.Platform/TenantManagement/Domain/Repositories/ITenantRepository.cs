using King.Nexa.Platform.Shared.Domain.Repositories;
using King.Nexa.Platform.TenantManagement.Domain.Model.Aggregates;

namespace King.Nexa.Platform.TenantManagement.Domain.Repositories;

public interface ITenantRepository : IBaseRepository<Tenant>
{
    Task<Tenant?> FindBySlugAsync(string slug, CancellationToken cancellationToken = default);
}
