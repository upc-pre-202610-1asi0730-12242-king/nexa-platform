using King.Nexa.Platform.Shared.Domain.Model.Entities;

namespace King.Nexa.Platform.TenantManagement.Domain.Model.Entities;

public class TenantMember : AuditableEntity
{
    public int TenantId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "Viewer";
    public string Department { get; set; } = string.Empty;
    public string Status { get; set; } = "active";
    public bool PortalAccess { get; set; }
}

public class TenantRule : AuditableEntity
{
    public int TenantId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "operations";
    public bool Enabled { get; set; } = true;
}

public class TenantCustomField : AuditableEntity
{
    public int TenantId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string TargetResource { get; set; } = string.Empty;
    public string FieldType { get; set; } = "text";
    public bool Required { get; set; }
    public bool Enabled { get; set; } = true;
}

public class TenantSubscription : AuditableEntity
{
    public int TenantId { get; set; }
    public string Plan { get; set; } = "Standard";
    public int Seats { get; set; }
    public int Warehouses { get; set; }
    public string PaymentStatus { get; set; } = "review_active";
    public DateOnly? NextBillingDate { get; set; }
    public string BillingContact { get; set; } = string.Empty;
}

public class WorkspaceFeature : AuditableEntity
{
    public int TenantId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Segment { get; set; } = "workspace";
    public bool Enabled { get; set; } = true;
    public string PlanRequired { get; set; } = "Starter";
}
