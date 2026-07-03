using King.Nexa.Platform.Invoicing.Domain.Model.Entities;
using King.Nexa.Platform.Invoicing.Interfaces.Rest.Resources;

namespace King.Nexa.Platform.Invoicing.Interfaces.Rest.Transform;

public static class PaymentProcessRecordResourceFromEntityAssembler
{
    public static PaymentProcessRecordResource ToResourceFromEntity(PaymentProcessRecord entity) =>
        new(
            entity.Id,
            entity.TenantId,
            entity.OrderId,
            entity.ClientAccountId,
            entity.PaymentId,
            entity.PaymentMethodRecordId,
            entity.Subtotal,
            entity.Discount,
            entity.Shipping,
            entity.Igv,
            entity.Total,
            entity.Status,
            entity.CreatedAt,
            entity.UpdatedAt);

    public static PaymentProcessRecord ToEntityFromResource(CreatePaymentProcessRecordResource resource) =>
        new()
        {
            OrderId = resource.OrderId,
            ClientAccountId = resource.ClientAccountId,
            PaymentId = resource.PaymentId,
            PaymentMethodRecordId = resource.PaymentMethodRecordId,
            Subtotal = resource.Subtotal,
            Discount = resource.Discount,
            Shipping = resource.Shipping,
            Igv = resource.Igv,
            Total = resource.Total,
            Status = resource.Status ?? "pending"
        };
}
