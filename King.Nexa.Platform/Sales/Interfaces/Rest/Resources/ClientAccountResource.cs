namespace King.Nexa.Platform.Sales.Interfaces.Rest.Resources;

public record ClientAccountResource(
    int Id,
    int TenantId,
    string Code,
    string BusinessName,
    string CommercialName,
    string Ruc,
    string Segment,
    string Contact,
    string ContactEmail,
    string Phone,
    string Address,
    string District,
    string Province,
    string DeliveryReference,
    string DocumentProfile,
    string PaymentCondition,
    decimal MonthlyCreditLimit,
    decimal MonthlyCreditUsed,
    decimal MonthlyCreditAvailable,
    string MonthlyCreditStatus,
    string DeliveryPreference,
    bool PortalAccess,
    string SellerWorkspaceEmail,
    string Status);

