using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using King.Nexa.Platform.TenantManagement.Domain.Model.Aggregates;
using King.Nexa.Platform.TenantManagement.Domain.Model.Entities;
using King.Nexa.Platform.TenantManagement.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace King.Nexa.Platform.TenantManagement.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class TenantAdministrationRepository(AppDbContext context) : ITenantAdministrationRepository
{
    public async Task AddAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class =>
        await context.Set<T>().AddAsync(entity, cancellationToken);

    public Task<T?> FindByIdAsync<T>(int tenantId, int id, CancellationToken cancellationToken = default) where T : class =>
        context.Set<T>().FirstOrDefaultAsync(row =>
            EF.Property<int>(row, "Id") == id &&
            EF.Property<int>(row, "TenantId") == tenantId,
            cancellationToken);

    public async Task<IEnumerable<T>> ListAsync<T>(int tenantId, CancellationToken cancellationToken = default) where T : class =>
        await context.Set<T>().AsNoTracking()
            .Where(row => EF.Property<int>(row, "TenantId") == tenantId)
            .OrderBy(row => EF.Property<int>(row, "Id"))
            .ToListAsync(cancellationToken);

    public void Remove<T>(T entity) where T : class => context.Set<T>().Remove(entity);

    public Task<bool> WorkspaceBelongsToTenantAsync(int tenantId, int workspaceId, CancellationToken cancellationToken = default) =>
        context.Workspaces.AsNoTracking().AnyAsync(row => row.Id == workspaceId && row.TenantId == tenantId, cancellationToken);

    public Task<bool> WorkspaceSlugExistsAsync(int tenantId, string slug, int? exceptWorkspaceId = null, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeSlug(slug);
        return context.Workspaces.AsNoTracking().AnyAsync(row =>
            row.TenantId == tenantId &&
            row.Slug == normalized &&
            (!exceptWorkspaceId.HasValue || row.Id != exceptWorkspaceId.Value),
            cancellationToken);
    }

    public Task<bool> ClientAccountBelongsToTenantAsync(int tenantId, int clientAccountId, CancellationToken cancellationToken = default) =>
        context.ClientAccounts.AsNoTracking().AnyAsync(row => row.Id == clientAccountId && row.TenantId == tenantId, cancellationToken);

    public async Task<TenantUserReference?> FindUserAsync(int userId, CancellationToken cancellationToken = default) =>
        await context.Users.AsNoTracking()
            .Where(row => row.Id == userId)
            .Select(row => new TenantUserReference(row.Id, row.Email))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<Workspace?> FindWorkspaceBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeSlug(slug);
        var candidates = await context.Workspaces.AsNoTracking()
            .Where(row => row.Status == "active")
            .Join(context.Tenants.AsNoTracking(), workspace => workspace.TenantId, tenant => tenant.Id, (workspace, tenant) => new { Workspace = workspace, Tenant = tenant })
            .ToListAsync(cancellationToken);
        return candidates
            .Where(row =>
                row.Workspace.Slug == normalized ||
                row.Tenant.Slug == normalized ||
                NormalizeSlug(row.Workspace.Name) == normalized ||
                NormalizeSlug(row.Tenant.Name) == normalized ||
                NormalizeSlug(row.Tenant.LegalName) == normalized)
            .OrderBy(row => WorkspaceMatchRank(row.Workspace, row.Tenant, normalized))
            .FirstOrDefault()?.Workspace;
    }

    private static int WorkspaceMatchRank(Workspace workspace, Tenant tenant, string normalized)
    {
        if (workspace.Slug == normalized) return 0;
        if (workspace.IsPrimary && tenant.Slug == normalized) return 1;
        if (workspace.IsPrimary && NormalizeSlug(tenant.Name) == normalized) return 2;
        if (workspace.IsPrimary && NormalizeSlug(tenant.LegalName) == normalized) return 3;
        if (NormalizeSlug(workspace.Name) == normalized) return 4;
        return 5;
    }

    private static string NormalizeSlug(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant().Replace(".", "-").Replace("_", "-").Replace(" ", "-");
}
