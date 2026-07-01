namespace King.Nexa.Platform.TenantManagement.Interfaces.Rest.Resources;

public record TenantMemberResource(int Id, int TenantId, string FullName, string Email, string Role, string Department, string Status, bool PortalAccess, DateTime CreatedAt, DateTime? UpdatedAt);
public record UpsertTenantMemberResource(int TenantId, string FullName, string Email, string Role, string Department, string Status, bool PortalAccess);
public record TenantRuleResource(int Id, int TenantId, string Code, string Name, string Description, string Category, bool Enabled, DateTime CreatedAt, DateTime? UpdatedAt);
public record UpsertTenantRuleResource(int TenantId, string Code, string Name, string Description, string Category, bool Enabled);
public record TenantCustomFieldResource(int Id, int TenantId, string Code, string Label, string TargetResource, string FieldType, bool Required, bool Enabled, DateTime CreatedAt, DateTime? UpdatedAt);
public record UpsertTenantCustomFieldResource(int TenantId, string Code, string Label, string TargetResource, string FieldType, bool Required, bool Enabled);
public record TenantSubscriptionResource(int Id, int TenantId, string Plan, int Seats, int Warehouses, string PaymentStatus, DateOnly? NextBillingDate, string BillingContact, DateTime CreatedAt, DateTime? UpdatedAt);
public record UpsertTenantSubscriptionResource(int TenantId, string Plan, int Seats, int Warehouses, string PaymentStatus, DateOnly? NextBillingDate, string BillingContact);
public record WorkspaceFeatureResource(int Id, int TenantId, string Code, string Name, string Segment, bool Enabled, string PlanRequired, DateTime CreatedAt, DateTime? UpdatedAt);
public record UpsertWorkspaceFeatureResource(int TenantId, string Code, string Name, string Segment, bool Enabled, string PlanRequired);
public record WorkspaceResource(int Id, int TenantId, string Name, string Slug, string Url, string EmailDomain, string Status, bool IsPrimary, DateTime CreatedAt, DateTime? UpdatedAt);
public record UpsertWorkspaceResource(int TenantId, string Name, string Slug, string Url, string EmailDomain, string Status, bool IsPrimary);
public record UserWorkspaceMembershipResource(int Id, int TenantId, int WorkspaceId, int UserId, string Email, string FullName, string Role, string Department, string Status, bool PortalAccess, int? ClientAccountId, DateTime CreatedAt, DateTime? UpdatedAt);
public record UpsertUserWorkspaceMembershipResource(int TenantId, int WorkspaceId, int UserId, string Email, string FullName, string Role, string Department, string Status, bool PortalAccess, int? ClientAccountId);
public record WorkspacePreferenceResource(int Id, int TenantId, int WorkspaceId, string Key, string Value, string ValueType, DateTime CreatedAt, DateTime? UpdatedAt);
public record UpsertWorkspacePreferenceResource(int TenantId, int WorkspaceId, string Key, string Value, string ValueType);

