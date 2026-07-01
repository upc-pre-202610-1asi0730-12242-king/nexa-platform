using King.Nexa.Platform.Invoicing.Domain.Model.ValueObjects;

namespace King.Nexa.Platform.Invoicing.Domain.Model.Commands;

public record RegisterPaymentCommand(
    int? InvoiceId,
    int? OrderId,
    int? ClientAccountId,
    int? PaymentOptionId,
    int? PaymentMethodRecordId,
    BillingAmount BillingAmount,
    string ReferenceCode);
