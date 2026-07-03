using King.Nexa.Platform.Sales.Domain.Model.Aggregates;

namespace King.Nexa.Platform.Sales.Application.QueryServices;

public interface IClientAccountQueryService
{
    Task<IEnumerable<ClientAccount>> ListAsync(CancellationToken cancellationToken = default);
    Task<ClientAccount?> FindByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ClientAccount?> FindByCodeAsync(string code, CancellationToken cancellationToken = default);
}
