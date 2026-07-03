using King.Nexa.Platform.Sales.Application.QueryServices;
using King.Nexa.Platform.Sales.Domain.Model.Aggregates;
using King.Nexa.Platform.Sales.Domain.Repositories;

namespace King.Nexa.Platform.Sales.Application.Internal.QueryServices;

public class ClientAccountQueryService(IClientAccountRepository clientAccountRepository) : IClientAccountQueryService
{
    public Task<IEnumerable<ClientAccount>> ListAsync(CancellationToken cancellationToken = default) =>
        clientAccountRepository.ListAsync(cancellationToken);

    public Task<ClientAccount?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        clientAccountRepository.FindByIdAsync(id, cancellationToken);

    public Task<ClientAccount?> FindByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        clientAccountRepository.FindByCodeAsync(code, cancellationToken);
}
