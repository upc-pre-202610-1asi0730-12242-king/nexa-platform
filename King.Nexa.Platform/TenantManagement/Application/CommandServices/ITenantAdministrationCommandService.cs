using King.Nexa.Platform.TenantManagement.Domain.Model.Entities;

namespace King.Nexa.Platform.TenantManagement.Application.CommandServices;

public interface ITenantAdministrationCommandService
{
    Task<TenantMember> CreateMemberAsync(TenantMember entity, CancellationToken cancellationToken = default);
    Task<TenantMember?> UpdateMemberAsync(int id, TenantMember entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteMemberAsync(int id, CancellationToken cancellationToken = default);
    Task<TenantRule> CreateRuleAsync(TenantRule entity, CancellationToken cancellationToken = default);
    Task<TenantRule?> UpdateRuleAsync(int id, TenantRule entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteRuleAsync(int id, CancellationToken cancellationToken = default);
    Task<TenantCustomField> CreateCustomFieldAsync(TenantCustomField entity, CancellationToken cancellationToken = default);
    Task<TenantCustomField?> UpdateCustomFieldAsync(int id, TenantCustomField entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteCustomFieldAsync(int id, CancellationToken cancellationToken = default);
    Task<TenantSubscription> CreateSubscriptionAsync(TenantSubscription entity, CancellationToken cancellationToken = default);
    Task<TenantSubscription?> UpdateSubscriptionAsync(int id, TenantSubscription entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteSubscriptionAsync(int id, CancellationToken cancellationToken = default);
    Task<WorkspaceFeature> CreateFeatureAsync(WorkspaceFeature entity, CancellationToken cancellationToken = default);
    Task<WorkspaceFeature?> UpdateFeatureAsync(int id, WorkspaceFeature entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteFeatureAsync(int id, CancellationToken cancellationToken = default);
    Task<Workspace> CreateWorkspaceAsync(Workspace entity, CancellationToken cancellationToken = default);
    Task<Workspace?> UpdateWorkspaceAsync(int id, Workspace entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteWorkspaceAsync(int id, CancellationToken cancellationToken = default);
    Task<UserWorkspaceMembership> CreateMembershipAsync(UserWorkspaceMembership entity, CancellationToken cancellationToken = default);
    Task<UserWorkspaceMembership?> UpdateMembershipAsync(int id, UserWorkspaceMembership entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteMembershipAsync(int id, CancellationToken cancellationToken = default);
    Task<WorkspacePreference> CreatePreferenceAsync(WorkspacePreference entity, CancellationToken cancellationToken = default);
    Task<WorkspacePreference?> UpdatePreferenceAsync(int id, WorkspacePreference entity, CancellationToken cancellationToken = default);
    Task<bool> DeletePreferenceAsync(int id, CancellationToken cancellationToken = default);
}

