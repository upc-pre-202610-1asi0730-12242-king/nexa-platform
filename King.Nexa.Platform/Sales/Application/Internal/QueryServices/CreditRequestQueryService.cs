using King.Nexa.Platform.Sales.Application.QueryServices;
using King.Nexa.Platform.Sales.Domain.Model.Entities;
using King.Nexa.Platform.Sales.Domain.Repositories;
using King.Nexa.Platform.Shared.Application.Security;

namespace King.Nexa.Platform.Sales.Application.Internal.QueryServices;

public class CreditRequestQueryService(ICreditRequestRepository repository, ICurrentWorkspaceContext workspaceContext) : ICreditRequestQueryService
{
    public Task<IReadOnlyCollection<CreditRequest>> ListAsync(CancellationToken cancellationToken = default) =>
        repository.ListAsync(TenantId(), workspaceContext.ClientAccountId, cancellationToken);

    public Task<CreditRequest?> FindAsync(int id, CancellationToken cancellationToken = default) =>
        repository.FindAsync(TenantId(), id, workspaceContext.ClientAccountId, cancellationToken);

    private int TenantId() => workspaceContext.TenantId ?? throw new InvalidOperationException("Current tenant is required.");
}

