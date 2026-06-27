using King.Nexa.Platform.Sales.Domain.Model.Aggregates;

namespace King.Nexa.Platform.Sales.Application.CommandServices;

public interface IClientAccountCommandService
{
    Task<ClientAccount> CreateAsync(ClientAccount client, CancellationToken cancellationToken = default);
    Task<ClientAccount?> UpdateAsync(int id, ClientAccount client, CancellationToken cancellationToken = default);
}

