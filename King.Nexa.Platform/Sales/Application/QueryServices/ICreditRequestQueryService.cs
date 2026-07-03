using King.Nexa.Platform.Sales.Domain.Model.Entities;

namespace King.Nexa.Platform.Sales.Application.QueryServices;

public interface ICreditRequestQueryService
{
    Task<IReadOnlyCollection<CreditRequest>> ListAsync(CancellationToken cancellationToken = default);
    Task<CreditRequest?> FindAsync(int id, CancellationToken cancellationToken = default);
}
