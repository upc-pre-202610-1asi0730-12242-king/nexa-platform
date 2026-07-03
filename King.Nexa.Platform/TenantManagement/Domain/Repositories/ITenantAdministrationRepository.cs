using King.Nexa.Platform.TenantManagement.Domain.Model.Aggregates;
using King.Nexa.Platform.TenantManagement.Domain.Model.Entities;

namespace King.Nexa.Platform.TenantManagement.Domain.Repositories;

public interface ITenantAdministrationRepository
{
    Task AddAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class;
    Task<T?> FindByIdAsync<T>(int tenantId, int id, CancellationToken cancellationToken = default) where T : class;
    Task<IEnumerable<T>> ListAsync<T>(int tenantId, CancellationToken cancellationToken = default) where T : class;
    void Remove<T>(T entity) where T : class;
    Task<bool> WorkspaceBelongsToTenantAsync(int tenantId, int workspaceId, CancellationToken cancellationToken = default);
    Task<bool> WorkspaceSlugExistsAsync(int tenantId, string slug, int? exceptWorkspaceId = null, CancellationToken cancellationToken = default);
    Task<bool> ClientAccountBelongsToTenantAsync(int tenantId, int clientAccountId, CancellationToken cancellationToken = default);
    Task<TenantUserReference?> FindUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<Workspace?> FindWorkspaceBySlugAsync(string slug, CancellationToken cancellationToken = default);
}

public record TenantUserReference(int Id, string Email);
