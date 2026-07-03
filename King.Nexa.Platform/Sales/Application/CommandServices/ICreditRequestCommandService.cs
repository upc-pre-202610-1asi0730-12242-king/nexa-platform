using King.Nexa.Platform.Sales.Domain.Model.Entities;

namespace King.Nexa.Platform.Sales.Application.CommandServices;

public interface ICreditRequestCommandService
{
    Task<CreditRequest> CreateAsync(CreditRequest entity, CancellationToken cancellationToken = default);
    Task<CreditRequest?> ResolveAsync(int id, string status, string reviewedBy, string note, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
