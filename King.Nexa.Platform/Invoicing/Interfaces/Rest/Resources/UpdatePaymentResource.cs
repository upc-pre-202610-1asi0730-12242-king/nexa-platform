namespace King.Nexa.Platform.Invoicing.Interfaces.Rest.Resources;

public record UpdatePaymentResource(
    int? InvoiceId,
    int? OrderId,
    int? ClientAccountId,
    int? PaymentOptionId,
    int? PaymentMethodRecordId,
    decimal Amount,
    string Currency,
    string? ReferenceCode = null);
