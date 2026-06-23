using King.Nexa.Platform.Iam.Domain.Model.Aggregates;
using King.Nexa.Platform.Iam.Domain.Model.Queries;

namespace King.Nexa.Platform.Iam.Application.QueryServices;

public interface IUserQueryService
{
    Task<User?> Handle(GetUserByUsernameQuery query, CancellationToken cancellationToken = default);
    Task<User?> FindByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<User>> ListByTenantAsync(int tenantId, CancellationToken cancellationToken = default);
}

