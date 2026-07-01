using King.Nexa.Platform.Sales.Application.CommandServices;
using King.Nexa.Platform.Sales.Domain.Model.Aggregates;
using King.Nexa.Platform.Sales.Domain.Repositories;
using King.Nexa.Platform.Shared.Application.Security;
using King.Nexa.Platform.Shared.Domain.Repositories;

namespace King.Nexa.Platform.Sales.Application.Internal.CommandServices;

public class ClientAccountCommandService(
    IClientAccountRepository clientAccountRepository,
    IUnitOfWork unitOfWork,
    ICurrentWorkspaceContext workspaceContext) : IClientAccountCommandService
{
    public async Task<ClientAccount> CreateAsync(ClientAccount client, CancellationToken cancellationToken = default)
    {
        client.AssignTenant(CurrentTenantId());
        await clientAccountRepository.AddAsync(client, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return client;
    }

    public async Task<ClientAccount?> UpdateAsync(int id, ClientAccount client, CancellationToken cancellationToken = default)
    {
        var existing = await clientAccountRepository.FindByIdAsync(id, cancellationToken);
        if (existing is null) return null;

        existing.UpdateFrom(client);
        clientAccountRepository.Update(existing);
        await unitOfWork.CompleteAsync(cancellationToken);
        return existing;
    }

    private int CurrentTenantId() =>
        workspaceContext.TenantId ?? throw new InvalidOperationException("Current tenant is required.");
}
