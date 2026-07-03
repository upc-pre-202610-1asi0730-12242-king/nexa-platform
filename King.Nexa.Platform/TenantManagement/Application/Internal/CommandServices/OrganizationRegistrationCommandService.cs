using King.Nexa.Platform.Shared.Domain.Repositories;
using King.Nexa.Platform.TenantManagement.Application.CommandServices;
using King.Nexa.Platform.TenantManagement.Domain.Model.Commands;
using King.Nexa.Platform.TenantManagement.Domain.Model.Entities;
using King.Nexa.Platform.TenantManagement.Domain.Repositories;

namespace King.Nexa.Platform.TenantManagement.Application.Internal.CommandServices;

public class OrganizationRegistrationCommandService(
    IOrganizationRegistrationRequestRepository registrationRepository,
    IUnitOfWork unitOfWork) : IOrganizationRegistrationCommandService
{
    public async Task<OrganizationRegistrationRequest> CreateAsync(
        CreateOrganizationRegistrationCommand command,
        CancellationToken cancellationToken = default)
    {
        if (await registrationRepository.ExistsByExternalIdAsync(command.ExternalId, cancellationToken))
            throw new InvalidOperationException("Organization registration already exists.");

        var registration = new OrganizationRegistrationRequest
        {
            ExternalId = command.ExternalId,
            Status = command.Status,
            CompanyName = command.CompanyName,
            WorkspaceName = command.WorkspaceName,
            WorkspaceSlug = command.WorkspaceSlug,
            AdminEmail = command.AdminEmail,
            PayloadJson = command.PayloadJson,
            SubmittedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
        };

        await registrationRepository.AddAsync(registration, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return registration;
    }
}
