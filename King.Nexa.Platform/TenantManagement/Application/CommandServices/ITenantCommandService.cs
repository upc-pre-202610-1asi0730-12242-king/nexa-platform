using King.Nexa.Platform.TenantManagement.Domain.Model.Aggregates;

namespace King.Nexa.Platform.TenantManagement.Application.CommandServices;

public interface ITenantCommandService
{
    Task<Tenant?> UpdateAsync(int id, Tenant tenant, CancellationToken cancellationToken = default);
}

