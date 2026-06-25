using King.Nexa.Platform.TenantManagement.Application.QueryServices;
using King.Nexa.Platform.TenantManagement.Domain.Model.Entities;
using King.Nexa.Platform.TenantManagement.Domain.Repositories;

namespace King.Nexa.Platform.TenantManagement.Application.Internal.QueryServices;

public class OrganizationRegistrationQueryService(
    IOrganizationRegistrationRequestRepository registrationRepository) : IOrganizationRegistrationQueryService
{
    public Task<IEnumerable<OrganizationRegistrationRequest>> ListNewestAsync(CancellationToken cancellationToken = default) =>
        registrationRepository.ListNewestAsync(cancellationToken);

    public Task<OrganizationRegistrationRequest?> FindByExternalIdAsync(
        string externalId,
        CancellationToken cancellationToken = default) =>
        registrationRepository.FindByExternalIdAsync(externalId, cancellationToken);
}

