using King.Nexa.Platform.Shared.Domain.Repositories;
using King.Nexa.Platform.TenantManagement.Application.CommandServices;
using King.Nexa.Platform.TenantManagement.Domain.Model.Aggregates;
using King.Nexa.Platform.TenantManagement.Domain.Repositories;

namespace King.Nexa.Platform.TenantManagement.Application.Internal.CommandServices;

public class TenantCommandService(ITenantRepository tenantRepository, IUnitOfWork unitOfWork) : ITenantCommandService
{
    public async Task<Tenant?> UpdateAsync(int id, Tenant tenant, CancellationToken cancellationToken = default)
    {
        var existing = await tenantRepository.FindByIdAsync(id, cancellationToken);
        if (existing is null) return null;

        existing.Update(tenant.Name, tenant.LegalName, tenant.WorkspaceUrl, tenant.EmailDomain, tenant.Plan, tenant.Status, tenant.Country);
        tenantRepository.Update(existing);
        await unitOfWork.CompleteAsync(cancellationToken);
        return existing;
    }
}

