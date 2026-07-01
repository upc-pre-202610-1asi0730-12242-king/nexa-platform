using King.Nexa.Platform.TenantManagement.Application.QueryServices;
using King.Nexa.Platform.TenantManagement.Domain.Model.Entities;
using King.Nexa.Platform.TenantManagement.Domain.Repositories;

namespace King.Nexa.Platform.TenantManagement.Application.Internal.QueryServices;

public class TenantAdministrationQueryService(
    ITenantAdministrationRepository repository,
    King.Nexa.Platform.Shared.Application.Security.ICurrentWorkspaceContext workspaceContext) : ITenantAdministrationQueryService
{
    public Task<IEnumerable<TenantMember>> ListMembersAsync(CancellationToken cancellationToken = default) => ListAsync<TenantMember>(cancellationToken);
    public Task<TenantMember?> GetMemberAsync(int id, CancellationToken cancellationToken = default) => GetAsync<TenantMember>(id, cancellationToken);
    public Task<IEnumerable<TenantRule>> ListRulesAsync(CancellationToken cancellationToken = default) => ListAsync<TenantRule>(cancellationToken);
    public Task<TenantRule?> GetRuleAsync(int id, CancellationToken cancellationToken = default) => GetAsync<TenantRule>(id, cancellationToken);
    public Task<IEnumerable<TenantCustomField>> ListCustomFieldsAsync(CancellationToken cancellationToken = default) => ListAsync<TenantCustomField>(cancellationToken);
    public Task<TenantCustomField?> GetCustomFieldAsync(int id, CancellationToken cancellationToken = default) => GetAsync<TenantCustomField>(id, cancellationToken);
    public Task<IEnumerable<TenantSubscription>> ListSubscriptionsAsync(CancellationToken cancellationToken = default) => ListAsync<TenantSubscription>(cancellationToken);
    public Task<TenantSubscription?> GetSubscriptionAsync(int id, CancellationToken cancellationToken = default) => GetAsync<TenantSubscription>(id, cancellationToken);
    public Task<IEnumerable<WorkspaceFeature>> ListFeaturesAsync(CancellationToken cancellationToken = default) => ListAsync<WorkspaceFeature>(cancellationToken);
    public Task<WorkspaceFeature?> GetFeatureAsync(int id, CancellationToken cancellationToken = default) => GetAsync<WorkspaceFeature>(id, cancellationToken);
    public Task<IEnumerable<Workspace>> ListWorkspacesAsync(CancellationToken cancellationToken = default) => ListAsync<Workspace>(cancellationToken);
    public Task<Workspace?> GetWorkspaceAsync(int id, CancellationToken cancellationToken = default) => GetAsync<Workspace>(id, cancellationToken);
    public Task<IEnumerable<UserWorkspaceMembership>> ListMembershipsAsync(CancellationToken cancellationToken = default) => ListAsync<UserWorkspaceMembership>(cancellationToken);
    public Task<UserWorkspaceMembership?> GetMembershipAsync(int id, CancellationToken cancellationToken = default) => GetAsync<UserWorkspaceMembership>(id, cancellationToken);
    public Task<IEnumerable<WorkspacePreference>> ListPreferencesAsync(CancellationToken cancellationToken = default) => ListAsync<WorkspacePreference>(cancellationToken);
    public Task<WorkspacePreference?> GetPreferenceAsync(int id, CancellationToken cancellationToken = default) => GetAsync<WorkspacePreference>(id, cancellationToken);

    public Task<Workspace?> FindWorkspaceBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        repository.FindWorkspaceBySlugAsync(slug, cancellationToken);

    private async Task<IEnumerable<T>> ListAsync<T>(CancellationToken cancellationToken) where T : class =>
        workspaceContext.TenantId is { } tenantId
            ? await repository.ListAsync<T>(tenantId, cancellationToken)
            : [];

    private Task<T?> GetAsync<T>(int id, CancellationToken cancellationToken) where T : class =>
        workspaceContext.TenantId is { } tenantId
            ? repository.FindByIdAsync<T>(tenantId, id, cancellationToken)
            : Task.FromResult<T?>(null);
}

