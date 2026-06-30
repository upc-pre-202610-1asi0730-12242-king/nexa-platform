using King.Nexa.Platform.Invoicing.Domain.Model.Aggregates;
using King.Nexa.Platform.Invoicing.Interfaces.Rest.Resources;

namespace King.Nexa.Platform.Invoicing.Interfaces.Rest.Transform;

public static class PaymentResourceFromEntityAssembler
{
    public static PaymentResource ToResourceFromEntity(Payment entity) =>
        new(
            entity.Id,
            entity.TenantId,
            entity.InvoiceId,
            entity.OrderId,
            entity.ClientAccountId,
            entity.PaymentOptionId,
            entity.PaymentMethodRecordId,
            entity.BillingAmount.Amount,
            entity.BillingAmount.Currency,
            entity.ReferenceCode,
            entity.Status.ToString(),
            entity.ConfirmedAt,
            entity.RejectedAt);
}

