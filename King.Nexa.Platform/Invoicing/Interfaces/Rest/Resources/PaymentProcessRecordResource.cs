namespace King.Nexa.Platform.Invoicing.Interfaces.Rest.Resources;

public record PaymentProcessRecordResource(
    int Id,
    int TenantId,
    int? OrderId,
    int? ClientAccountId,
    int? PaymentId,
    int? PaymentMethodRecordId,
    decimal Subtotal,
    decimal Discount,
    decimal Shipping,
    decimal Igv,
    decimal Total,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CreatePaymentProcessRecordResource(
    int? OrderId,
    int? ClientAccountId,
    int? PaymentId,
    int? PaymentMethodRecordId,
    decimal Subtotal,
    decimal Discount,
    decimal Shipping,
    decimal Igv,
    decimal Total,
    string? Status);
