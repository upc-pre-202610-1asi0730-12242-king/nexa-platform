namespace King.Nexa.Platform.Sales.Interfaces.Rest.Resources;

public record CreditRequestResource(
    int Id,
    int TenantId,
    int ClientAccountId,
    string Code,
    decimal RequestedAmount,
    string Reason,
    string Status,
    int? CreatedByUserId,
    string ReviewedBy,
    string ResolutionNote,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CreateCreditRequestResource(
    int TenantId,
    int ClientAccountId,
    string Code,
    decimal RequestedAmount,
    string Reason,
    int? CreatedByUserId);

public record ResolveCreditRequestResource(string Status, string ReviewedBy, string Note);
