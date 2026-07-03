using King.Nexa.Platform.TenantManagement.Domain.Model.Aggregates;

namespace King.Nexa.Platform.TenantManagement.Application.QueryServices;

public interface ITenantQueryService
{
    Task<IEnumerable<Tenant>> ListAsync(CancellationToken cancellationToken = default);
    Task<Tenant?> FindByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Tenant?> FindBySlugAsync(string slug, CancellationToken cancellationToken = default);
}
