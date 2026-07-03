using King.Nexa.Platform.Sales.Domain.Model.Entities;

namespace King.Nexa.Platform.Sales.Domain.Repositories;

public interface ICreditRequestRepository
{
    Task<IReadOnlyCollection<CreditRequest>> ListAsync(int tenantId, int? clientAccountId = null, CancellationToken cancellationToken = default);
    Task<CreditRequest?> FindAsync(int tenantId, int id, int? clientAccountId = null, CancellationToken cancellationToken = default);
    Task<bool> ClientBelongsToTenantAsync(int tenantId, int clientAccountId, CancellationToken cancellationToken = default);
    Task AddAsync(CreditRequest entity, CancellationToken cancellationToken = default);
    void Remove(CreditRequest entity);
}
