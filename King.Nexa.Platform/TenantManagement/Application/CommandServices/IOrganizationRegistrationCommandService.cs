using King.Nexa.Platform.TenantManagement.Domain.Model.Commands;
using King.Nexa.Platform.TenantManagement.Domain.Model.Entities;

namespace King.Nexa.Platform.TenantManagement.Application.CommandServices;

public interface IOrganizationRegistrationCommandService
{
    Task<OrganizationRegistrationRequest> CreateAsync(
        CreateOrganizationRegistrationCommand command,
        CancellationToken cancellationToken = default);
}
