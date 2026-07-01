using King.Nexa.Platform.Shared.Domain.Model.Entities;

namespace King.Nexa.Platform.Shared.Application.Auditing;

public interface IAuditLogQueryService
{
    Task<IReadOnlyCollection<AuditLog>> ListAsync(int limit = 100, CancellationToken cancellationToken = default);
    Task<AuditLog?> FindByIdAsync(int id, CancellationToken cancellationToken = default);
}
