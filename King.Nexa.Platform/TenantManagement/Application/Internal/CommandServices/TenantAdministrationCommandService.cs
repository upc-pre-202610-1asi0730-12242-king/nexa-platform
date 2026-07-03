using King.Nexa.Platform.Shared.Application.Security;
using King.Nexa.Platform.Shared.Domain.Repositories;
using King.Nexa.Platform.TenantManagement.Application.CommandServices;
using King.Nexa.Platform.TenantManagement.Domain.Model.Entities;
using King.Nexa.Platform.TenantManagement.Domain.Repositories;

namespace King.Nexa.Platform.TenantManagement.Application.Internal.CommandServices;

public class TenantAdministrationCommandService(
    ITenantAdministrationRepository repository,
    IUnitOfWork unitOfWork,
    ICurrentWorkspaceContext workspaceContext) : ITenantAdministrationCommandService
{
    public async Task<TenantMember> CreateMemberAsync(TenantMember entity, CancellationToken cancellationToken = default)
    {
        entity.TenantId = TenantId(); Normalize(entity); return await AddAsync(entity, cancellationToken);
    }

    public async Task<TenantMember?> UpdateMemberAsync(int id, TenantMember draft, CancellationToken cancellationToken = default)
    {
        var entity = await FindAsync<TenantMember>(id, cancellationToken); if (entity is null) return null;
        entity.FullName = draft.FullName; entity.Email = draft.Email; entity.Role = draft.Role; entity.Department = draft.Department; entity.Status = draft.Status; entity.PortalAccess = draft.PortalAccess;
        Normalize(entity); return await SaveAsync(entity, cancellationToken);
    }

    public Task<bool> DeleteMemberAsync(int id, CancellationToken cancellationToken = default) => DeleteAsync<TenantMember>(id, cancellationToken);

    public async Task<TenantRule> CreateRuleAsync(TenantRule entity, CancellationToken cancellationToken = default)
    {
        entity.TenantId = TenantId(); Normalize(entity); return await AddAsync(entity, cancellationToken);
    }

    public async Task<TenantRule?> UpdateRuleAsync(int id, TenantRule draft, CancellationToken cancellationToken = default)
    {
        var entity = await FindAsync<TenantRule>(id, cancellationToken); if (entity is null) return null;
        entity.Code = draft.Code; entity.Name = draft.Name; entity.Description = draft.Description; entity.Category = draft.Category; entity.Enabled = draft.Enabled;
        Normalize(entity); return await SaveAsync(entity, cancellationToken);
    }

    public Task<bool> DeleteRuleAsync(int id, CancellationToken cancellationToken = default) => DeleteAsync<TenantRule>(id, cancellationToken);

    public async Task<TenantCustomField> CreateCustomFieldAsync(TenantCustomField entity, CancellationToken cancellationToken = default)
    {
        entity.TenantId = TenantId(); Normalize(entity); return await AddAsync(entity, cancellationToken);
    }

    public async Task<TenantCustomField?> UpdateCustomFieldAsync(int id, TenantCustomField draft, CancellationToken cancellationToken = default)
    {
        var entity = await FindAsync<TenantCustomField>(id, cancellationToken); if (entity is null) return null;
        entity.Code = draft.Code; entity.Label = draft.Label; entity.TargetResource = draft.TargetResource; entity.FieldType = draft.FieldType; entity.Required = draft.Required; entity.Enabled = draft.Enabled;
        Normalize(entity); return await SaveAsync(entity, cancellationToken);
    }

    public Task<bool> DeleteCustomFieldAsync(int id, CancellationToken cancellationToken = default) => DeleteAsync<TenantCustomField>(id, cancellationToken);

    public async Task<TenantSubscription> CreateSubscriptionAsync(TenantSubscription entity, CancellationToken cancellationToken = default)
    {
        entity.TenantId = TenantId(); Normalize(entity); return await AddAsync(entity, cancellationToken);
    }

    public async Task<TenantSubscription?> UpdateSubscriptionAsync(int id, TenantSubscription draft, CancellationToken cancellationToken = default)
    {
        var entity = await FindAsync<TenantSubscription>(id, cancellationToken); if (entity is null) return null;
        entity.Plan = draft.Plan; entity.Seats = draft.Seats; entity.Warehouses = draft.Warehouses; entity.PaymentStatus = draft.PaymentStatus; entity.NextBillingDate = draft.NextBillingDate; entity.BillingContact = draft.BillingContact;
        Normalize(entity); return await SaveAsync(entity, cancellationToken);
    }

    public Task<bool> DeleteSubscriptionAsync(int id, CancellationToken cancellationToken = default) => DeleteAsync<TenantSubscription>(id, cancellationToken);

    public async Task<WorkspaceFeature> CreateFeatureAsync(WorkspaceFeature entity, CancellationToken cancellationToken = default)
    {
        entity.TenantId = TenantId(); Normalize(entity); return await AddAsync(entity, cancellationToken);
    }

    public async Task<WorkspaceFeature?> UpdateFeatureAsync(int id, WorkspaceFeature draft, CancellationToken cancellationToken = default)
    {
        var entity = await FindAsync<WorkspaceFeature>(id, cancellationToken); if (entity is null) return null;
        entity.Code = draft.Code; entity.Name = draft.Name; entity.Segment = draft.Segment; entity.Enabled = draft.Enabled; entity.PlanRequired = draft.PlanRequired;
        Normalize(entity); return await SaveAsync(entity, cancellationToken);
    }

    public Task<bool> DeleteFeatureAsync(int id, CancellationToken cancellationToken = default) => DeleteAsync<WorkspaceFeature>(id, cancellationToken);

    public async Task<Workspace> CreateWorkspaceAsync(Workspace entity, CancellationToken cancellationToken = default)
    {
        entity.TenantId = TenantId();
        Normalize(entity);
        await EnsureWorkspaceSlugAvailableAsync(entity.TenantId, entity.Slug, null, cancellationToken);
        entity.RecordCreation();
        return await AddAsync(entity, cancellationToken);
    }

    public async Task<Workspace?> UpdateWorkspaceAsync(int id, Workspace draft, CancellationToken cancellationToken = default)
    {
        var entity = await FindAsync<Workspace>(id, cancellationToken); if (entity is null) return null;
        var normalizedDraft = new Workspace { TenantId = entity.TenantId, Name = draft.Name, Slug = draft.Slug, Url = draft.Url, EmailDomain = draft.EmailDomain, Status = draft.Status, IsPrimary = draft.IsPrimary };
        Normalize(normalizedDraft);
        await EnsureWorkspaceSlugAvailableAsync(entity.TenantId, normalizedDraft.Slug, entity.Id, cancellationToken);
        entity.Rename(normalizedDraft.Name); entity.Slug = normalizedDraft.Slug; entity.Url = normalizedDraft.Url; entity.EmailDomain = normalizedDraft.EmailDomain; entity.Status = normalizedDraft.Status; entity.IsPrimary = normalizedDraft.IsPrimary;
        return await SaveAsync(entity, cancellationToken);
    }

    public Task<bool> DeleteWorkspaceAsync(int id, CancellationToken cancellationToken = default) => DeleteAsync<Workspace>(id, cancellationToken);

    public async Task<UserWorkspaceMembership> CreateMembershipAsync(UserWorkspaceMembership entity, CancellationToken cancellationToken = default)
    {
        entity.TenantId = TenantId(); await ValidateMembershipAsync(entity, cancellationToken); Normalize(entity); return await AddAsync(entity, cancellationToken);
    }

    public async Task<UserWorkspaceMembership?> UpdateMembershipAsync(int id, UserWorkspaceMembership draft, CancellationToken cancellationToken = default)
    {
        var entity = await FindAsync<UserWorkspaceMembership>(id, cancellationToken); if (entity is null) return null;
        entity.WorkspaceId = draft.WorkspaceId; entity.UserId = draft.UserId; entity.Email = draft.Email; entity.FullName = draft.FullName; entity.Role = draft.Role; entity.Department = draft.Department; entity.Status = draft.Status; entity.PortalAccess = draft.PortalAccess; entity.ClientAccountId = draft.ClientAccountId;
        await ValidateMembershipAsync(entity, cancellationToken); Normalize(entity); return await SaveAsync(entity, cancellationToken);
    }

    public Task<bool> DeleteMembershipAsync(int id, CancellationToken cancellationToken = default) => DeleteAsync<UserWorkspaceMembership>(id, cancellationToken);

    public async Task<WorkspacePreference> CreatePreferenceAsync(WorkspacePreference entity, CancellationToken cancellationToken = default)
    {
        entity.TenantId = TenantId(); await ValidateWorkspaceAsync(entity.WorkspaceId, cancellationToken); Normalize(entity); return await AddAsync(entity, cancellationToken);
    }

    public async Task<WorkspacePreference?> UpdatePreferenceAsync(int id, WorkspacePreference draft, CancellationToken cancellationToken = default)
    {
        var entity = await FindAsync<WorkspacePreference>(id, cancellationToken); if (entity is null) return null;
        entity.WorkspaceId = draft.WorkspaceId; entity.Key = draft.Key; entity.Value = draft.Value; entity.ValueType = draft.ValueType;
        await ValidateWorkspaceAsync(entity.WorkspaceId, cancellationToken); Normalize(entity); return await SaveAsync(entity, cancellationToken);
    }

    public Task<bool> DeletePreferenceAsync(int id, CancellationToken cancellationToken = default) => DeleteAsync<WorkspacePreference>(id, cancellationToken);

    private int TenantId() => workspaceContext.TenantId ?? throw new InvalidOperationException("Current tenant is required.");

    private async Task ValidateMembershipAsync(UserWorkspaceMembership entity, CancellationToken cancellationToken)
    {
        var tenantId = TenantId();
        await ValidateWorkspaceAsync(entity.WorkspaceId, cancellationToken);
        if (entity.ClientAccountId.HasValue && !await repository.ClientAccountBelongsToTenantAsync(tenantId, entity.ClientAccountId.Value, cancellationToken))
            throw new InvalidOperationException("Client account must belong to the current tenant.");
        var user = await repository.FindUserAsync(entity.UserId, cancellationToken);
        if (user is null || !user.Email.Equals(entity.Email, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("User identity does not match the workspace membership.");
    }

    private async Task ValidateWorkspaceAsync(int workspaceId, CancellationToken cancellationToken)
    {
        var tenantId = TenantId();
        if (!await repository.WorkspaceBelongsToTenantAsync(tenantId, workspaceId, cancellationToken))
            throw new InvalidOperationException("Workspace does not belong to the current tenant.");
    }

    private async Task EnsureWorkspaceSlugAvailableAsync(int tenantId, string slug, int? exceptWorkspaceId, CancellationToken cancellationToken)
    {
        if (await repository.WorkspaceSlugExistsAsync(tenantId, slug, exceptWorkspaceId, cancellationToken))
            throw new InvalidOperationException("Workspace slug is already in use.");
    }

    private async Task<T> AddAsync<T>(T entity, CancellationToken cancellationToken) where T : class
    {
        await repository.AddAsync(entity, cancellationToken); await unitOfWork.CompleteAsync(cancellationToken); return entity;
    }

    private async Task<T?> SaveAsync<T>(T entity, CancellationToken cancellationToken) where T : class
    {
        if (entity is King.Nexa.Platform.Shared.Domain.Model.Entities.AuditableEntity auditable)
            auditable.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.CompleteAsync(cancellationToken); return entity;
    }

    private Task<T?> FindAsync<T>(int id, CancellationToken cancellationToken) where T : class =>
        repository.FindByIdAsync<T>(TenantId(), id, cancellationToken);

    private async Task<bool> DeleteAsync<T>(int id, CancellationToken cancellationToken) where T : class
    {
        var entity = await FindAsync<T>(id, cancellationToken); if (entity is null) return false;
        repository.Remove(entity); await unitOfWork.CompleteAsync(cancellationToken); return true;
    }

    private static void Normalize(TenantMember entity)
    {
        if (string.IsNullOrWhiteSpace(entity.FullName) || string.IsNullOrWhiteSpace(entity.Email)) throw new InvalidOperationException("Member name and email are required.");
        entity.FullName = entity.FullName.Trim(); entity.Email = entity.Email.Trim().ToLowerInvariant(); entity.Role = Value(entity.Role, "Viewer"); entity.Department = entity.Department?.Trim() ?? string.Empty; entity.Status = Value(entity.Status, "active").ToLowerInvariant();
    }

    private static void Normalize(TenantRule entity)
    {
        if (string.IsNullOrWhiteSpace(entity.Code) || string.IsNullOrWhiteSpace(entity.Name)) throw new InvalidOperationException("Rule code and name are required.");
        entity.Code = entity.Code.Trim().ToLowerInvariant(); entity.Name = entity.Name.Trim(); entity.Description = entity.Description?.Trim() ?? string.Empty; entity.Category = Value(entity.Category, "operations").ToLowerInvariant();
    }

    private static void Normalize(TenantCustomField entity)
    {
        if (string.IsNullOrWhiteSpace(entity.Code) || string.IsNullOrWhiteSpace(entity.Label) || string.IsNullOrWhiteSpace(entity.TargetResource)) throw new InvalidOperationException("Custom field code, label and target are required.");
        entity.Code = entity.Code.Trim().ToLowerInvariant(); entity.Label = entity.Label.Trim(); entity.TargetResource = entity.TargetResource.Trim().ToLowerInvariant(); entity.FieldType = Value(entity.FieldType, "text").ToLowerInvariant();
    }

    private static void Normalize(TenantSubscription entity)
    {
        if (entity.Seats < 0 || entity.Warehouses < 0) throw new InvalidOperationException("Subscription limits cannot be negative.");
        entity.Plan = Value(entity.Plan, "Standard"); entity.PaymentStatus = Value(entity.PaymentStatus, "review_active").ToLowerInvariant(); entity.BillingContact = entity.BillingContact?.Trim().ToLowerInvariant() ?? string.Empty;
    }

    private static void Normalize(WorkspaceFeature entity)
    {
        if (string.IsNullOrWhiteSpace(entity.Code) || string.IsNullOrWhiteSpace(entity.Name)) throw new InvalidOperationException("Feature code and name are required.");
        entity.Code = entity.Code.Trim().ToLowerInvariant(); entity.Name = entity.Name.Trim(); entity.Segment = Value(entity.Segment, "workspace").ToLowerInvariant(); entity.PlanRequired = Value(entity.PlanRequired, "Starter");
    }

    private static void Normalize(Workspace entity)
    {
        if (string.IsNullOrWhiteSpace(entity.Name) || string.IsNullOrWhiteSpace(entity.Slug)) throw new InvalidOperationException("Workspace name and slug are required.");
        entity.Name = entity.Name.Trim(); entity.Slug = Slug(entity.Slug); entity.Url = string.IsNullOrWhiteSpace(entity.Url) ? $"{entity.Slug}.nexa.com.pe" : entity.Url.Trim().ToLowerInvariant(); entity.EmailDomain = entity.EmailDomain?.Trim().ToLowerInvariant() ?? string.Empty; entity.Status = Value(entity.Status, "active").ToLowerInvariant();
    }

    private static void Normalize(UserWorkspaceMembership entity)
    {
        if (string.IsNullOrWhiteSpace(entity.Email)) throw new InvalidOperationException("Membership email is required.");
        entity.Email = entity.Email.Trim().ToLowerInvariant(); entity.FullName = Value(entity.FullName, entity.Email.Split('@')[0]); entity.Role = Value(entity.Role, "Viewer"); entity.Department = entity.Department?.Trim() ?? string.Empty; entity.Status = Value(entity.Status, "active").ToLowerInvariant();
    }

    private static void Normalize(WorkspacePreference entity)
    {
        if (string.IsNullOrWhiteSpace(entity.Key)) throw new InvalidOperationException("Preference key is required.");
        entity.Key = entity.Key.Trim(); entity.Value = entity.Value?.Trim() ?? string.Empty; entity.ValueType = Value(entity.ValueType, "string").ToLowerInvariant();
        if (entity.ValueType is not ("string" or "boolean" or "number")) throw new InvalidOperationException("Preference value type must be string, boolean or number.");
    }

    private static string Value(string? value, string fallback) => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    private static string Slug(string value) => value.Trim().ToLowerInvariant().Replace(".", "-").Replace("_", "-").Replace(" ", "-");
}
