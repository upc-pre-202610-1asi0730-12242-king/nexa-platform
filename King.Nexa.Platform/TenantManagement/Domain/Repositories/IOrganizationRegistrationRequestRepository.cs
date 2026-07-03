using King.Nexa.Platform.Shared.Domain.Repositories;
using King.Nexa.Platform.TenantManagement.Domain.Model.Entities;

namespace King.Nexa.Platform.TenantManagement.Domain.Repositories;

public interface IOrganizationRegistrationRequestRepository : IBaseRepository<OrganizationRegistrationRequest>
{
    Task<IEnumerable<OrganizationRegistrationRequest>> ListNewestAsync(CancellationToken cancellationToken = default);
    Task<OrganizationRegistrationRequest?> FindByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);
}
