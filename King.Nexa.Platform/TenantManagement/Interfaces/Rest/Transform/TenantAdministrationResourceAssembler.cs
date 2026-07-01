using King.Nexa.Platform.TenantManagement.Domain.Model.Entities;
using King.Nexa.Platform.TenantManagement.Interfaces.Rest.Resources;

namespace King.Nexa.Platform.TenantManagement.Interfaces.Rest.Transform;

public static class TenantAdministrationResourceAssembler
{
    public static TenantMemberResource ToResource(TenantMember x) => new(x.Id, x.TenantId, x.FullName, x.Email, x.Role, x.Department, x.Status, x.PortalAccess, x.CreatedAt, x.UpdatedAt);
    public static TenantMember ToEntity(UpsertTenantMemberResource x) => new() { TenantId = x.TenantId, FullName = x.FullName, Email = x.Email, Role = x.Role, Department = x.Department, Status = x.Status, PortalAccess = x.PortalAccess };
    public static TenantRuleResource ToResource(TenantRule x) => new(x.Id, x.TenantId, x.Code, x.Name, x.Description, x.Category, x.Enabled, x.CreatedAt, x.UpdatedAt);
    public static TenantRule ToEntity(UpsertTenantRuleResource x) => new() { TenantId = x.TenantId, Code = x.Code, Name = x.Name, Description = x.Description, Category = x.Category, Enabled = x.Enabled };
    public static TenantCustomFieldResource ToResource(TenantCustomField x) => new(x.Id, x.TenantId, x.Code, x.Label, x.TargetResource, x.FieldType, x.Required, x.Enabled, x.CreatedAt, x.UpdatedAt);
    public static TenantCustomField ToEntity(UpsertTenantCustomFieldResource x) => new() { TenantId = x.TenantId, Code = x.Code, Label = x.Label, TargetResource = x.TargetResource, FieldType = x.FieldType, Required = x.Required, Enabled = x.Enabled };
    public static TenantSubscriptionResource ToResource(TenantSubscription x) => new(x.Id, x.TenantId, x.Plan, x.Seats, x.Warehouses, x.PaymentStatus, x.NextBillingDate, x.BillingContact, x.CreatedAt, x.UpdatedAt);
    public static TenantSubscription ToEntity(UpsertTenantSubscriptionResource x) => new() { TenantId = x.TenantId, Plan = x.Plan, Seats = x.Seats, Warehouses = x.Warehouses, PaymentStatus = x.PaymentStatus, NextBillingDate = x.NextBillingDate, BillingContact = x.BillingContact };
    public static WorkspaceFeatureResource ToResource(WorkspaceFeature x) => new(x.Id, x.TenantId, x.Code, x.Name, x.Segment, x.Enabled, x.PlanRequired, x.CreatedAt, x.UpdatedAt);
    public static WorkspaceFeature ToEntity(UpsertWorkspaceFeatureResource x) => new() { TenantId = x.TenantId, Code = x.Code, Name = x.Name, Segment = x.Segment, Enabled = x.Enabled, PlanRequired = x.PlanRequired };
    public static WorkspaceResource ToResource(Workspace x) => new(x.Id, x.TenantId, x.Name, x.Slug, x.Url, x.EmailDomain, x.Status, x.IsPrimary, x.CreatedAt, x.UpdatedAt);
    public static Workspace ToEntity(UpsertWorkspaceResource x) => new() { TenantId = x.TenantId, Name = x.Name, Slug = x.Slug, Url = x.Url, EmailDomain = x.EmailDomain, Status = x.Status, IsPrimary = x.IsPrimary };
    public static UserWorkspaceMembershipResource ToResource(UserWorkspaceMembership x) => new(x.Id, x.TenantId, x.WorkspaceId, x.UserId, x.Email, x.FullName, x.Role, x.Department, x.Status, x.PortalAccess, x.ClientAccountId, x.CreatedAt, x.UpdatedAt);
    public static UserWorkspaceMembership ToEntity(UpsertUserWorkspaceMembershipResource x) => new() { TenantId = x.TenantId, WorkspaceId = x.WorkspaceId, UserId = x.UserId, Email = x.Email, FullName = x.FullName, Role = x.Role, Department = x.Department, Status = x.Status, PortalAccess = x.PortalAccess, ClientAccountId = x.ClientAccountId };
    public static WorkspacePreferenceResource ToResource(WorkspacePreference x) => new(x.Id, x.TenantId, x.WorkspaceId, x.Key, x.Value, x.ValueType, x.CreatedAt, x.UpdatedAt);
    public static WorkspacePreference ToEntity(UpsertWorkspacePreferenceResource x) => new() { TenantId = x.TenantId, WorkspaceId = x.WorkspaceId, Key = x.Key, Value = x.Value, ValueType = x.ValueType };
}

