namespace King.Nexa.Platform.Sales.Interfaces.Rest.Resources;

public record CreateClientAccountResource(
    string Code,
    string BusinessName,
    string CommercialName,
    string Ruc,
    string Segment,
    string Contact,
    string ContactEmail,
    string Phone,
    string? Address,
    string? District,
    string? Province,
    string? DeliveryReference,
    string? DocumentProfile,
    string PaymentCondition,
    decimal MonthlyCreditLimit,
    decimal MonthlyCreditUsed,
    string MonthlyCreditStatus,
    string DeliveryPreference,
    bool PortalAccess,
    string SellerWorkspaceEmail,
    string Status);

