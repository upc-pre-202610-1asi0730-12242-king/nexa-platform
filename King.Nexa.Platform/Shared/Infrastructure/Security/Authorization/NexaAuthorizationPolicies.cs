namespace King.Nexa.Platform.Shared.Infrastructure.Security.Authorization;

public static class NexaAuthorizationPolicies
{
    public const string WorkspaceMember = "WorkspaceMember";
    public const string CanManageWorkspace = "CanManageWorkspace";
    public const string CanCreateOrder = "CanCreateOrder";
    public const string CanAcceptPurchaseRequest = "CanAcceptPurchaseRequest";
    public const string CanStartDispatch = "CanStartDispatch";
    public const string CanManageDocuments = "CanManageDocuments";
    public const string CanManagePaymentMethods = "CanManagePaymentMethods";
    public const string CanManageCatalog = "CanManageCatalog";
    public const string CanManagePromotions = "CanManagePromotions";
    public const string CanManageInventory = "CanManageInventory";
    public const string CanManageSharedReferenceData = "CanManageSharedReferenceData";

    public static readonly HashSet<string> WorkspaceManagers = new(StringComparer.OrdinalIgnoreCase)
    {
        "Company Owner",
        "owner",
        "admin",
        "Administrator"
    };

    public static readonly HashSet<string> SalesRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Company Owner",
        "Sales",
        "sales",
        "commercial"
    };

    public static readonly HashSet<string> LogisticsRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Company Owner",
        "Logistics Manager",
        "logistics",
        "operations"
    };

    public static readonly HashSet<string> DocumentRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Company Owner",
        "Sales",
        "Logistics Manager",
        "sales",
        "commercial",
        "logistics"
    };

    public static readonly HashSet<string> SharedReferenceDataManagers = new(StringComparer.OrdinalIgnoreCase)
    {
        "Platform Admin",
        "SaaS Provider"
    };
}

