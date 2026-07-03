namespace King.Nexa.Platform.Invoicing.Interfaces.Rest.Resources;

public record PaymentResource(
    int Id,
    int TenantId,
    int? InvoiceId,
    int? OrderId,
    int? ClientAccountId,
    int? PaymentOptionId,
    int? PaymentMethodRecordId,
    decimal Amount,
    string Currency,
    string ReferenceCode,
    string Status,
    DateTimeOffset? ConfirmedAt,
    DateTimeOffset? RejectedAt);
