using King.Nexa.Platform.Invoicing.Domain.Model.Commands;
using King.Nexa.Platform.Invoicing.Domain.Model.ValueObjects;
using King.Nexa.Platform.Invoicing.Interfaces.Rest.Resources;

namespace King.Nexa.Platform.Invoicing.Interfaces.Rest.Transform;

public static class UpdatePaymentCommandFromResourceAssembler
{
    public static UpdatePaymentCommand ToCommandFromResource(int id, UpdatePaymentResource resource) =>
        new(
            id,
            resource.InvoiceId,
            resource.OrderId,
            resource.ClientAccountId,
            resource.PaymentOptionId,
            resource.PaymentMethodRecordId,
            new BillingAmount(resource.Amount, resource.Currency),
            string.IsNullOrWhiteSpace(resource.ReferenceCode)
                ? $"PAY-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}"
                : resource.ReferenceCode);
}

