using King.Nexa.Platform.TenantManagement.Domain.Model.Entities;

namespace King.Nexa.Platform.TenantManagement.Application.QueryServices;

public interface IOrganizationRegistrationQueryService
{
    Task<IEnumerable<OrganizationRegistrationRequest>> ListNewestAsync(CancellationToken cancellationToken = default);
    Task<OrganizationRegistrationRequest?> FindByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);
}
