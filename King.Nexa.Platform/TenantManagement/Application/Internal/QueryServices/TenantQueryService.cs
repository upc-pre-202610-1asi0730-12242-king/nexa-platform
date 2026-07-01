using King.Nexa.Platform.TenantManagement.Application.QueryServices;
using King.Nexa.Platform.TenantManagement.Domain.Model.Aggregates;
using King.Nexa.Platform.TenantManagement.Domain.Repositories;

namespace King.Nexa.Platform.TenantManagement.Application.Internal.QueryServices;

public class TenantQueryService(ITenantRepository tenantRepository) : ITenantQueryService
{
    public Task<IEnumerable<Tenant>> ListAsync(CancellationToken cancellationToken = default) =>
        tenantRepository.ListAsync(cancellationToken);

    public Task<Tenant?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        tenantRepository.FindByIdAsync(id, cancellationToken);

    public Task<Tenant?> FindBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        tenantRepository.FindBySlugAsync(slug, cancellationToken);
}

