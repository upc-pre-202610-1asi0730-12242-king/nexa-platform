using King.Nexa.Platform.TenantManagement.Domain.Model.Entities;

namespace King.Nexa.Platform.TenantManagement.Application.QueryServices;

public interface ITenantAdministrationQueryService
{
    Task<IEnumerable<TenantMember>> ListMembersAsync(CancellationToken cancellationToken = default);
    Task<TenantMember?> GetMemberAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TenantRule>> ListRulesAsync(CancellationToken cancellationToken = default);
    Task<TenantRule?> GetRuleAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TenantCustomField>> ListCustomFieldsAsync(CancellationToken cancellationToken = default);
    Task<TenantCustomField?> GetCustomFieldAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TenantSubscription>> ListSubscriptionsAsync(CancellationToken cancellationToken = default);
    Task<TenantSubscription?> GetSubscriptionAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkspaceFeature>> ListFeaturesAsync(CancellationToken cancellationToken = default);
    Task<WorkspaceFeature?> GetFeatureAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Workspace>> ListWorkspacesAsync(CancellationToken cancellationToken = default);
    Task<Workspace?> GetWorkspaceAsync(int id, CancellationToken cancellationToken = default);
    Task<Workspace?> FindWorkspaceBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserWorkspaceMembership>> ListMembershipsAsync(CancellationToken cancellationToken = default);
    Task<UserWorkspaceMembership?> GetMembershipAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkspacePreference>> ListPreferencesAsync(CancellationToken cancellationToken = default);
    Task<WorkspacePreference?> GetPreferenceAsync(int id, CancellationToken cancellationToken = default);
}

