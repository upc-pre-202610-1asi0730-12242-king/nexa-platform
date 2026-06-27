using King.Nexa.Platform.Sales.Domain.Model.Aggregates;
using King.Nexa.Platform.Shared.Domain.Repositories;

namespace King.Nexa.Platform.Sales.Domain.Repositories;

public interface IClientAccountRepository : IBaseRepository<ClientAccount>
{
    Task<ClientAccount?> FindByCodeAsync(string code, CancellationToken cancellationToken = default);
}

