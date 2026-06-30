using King.Nexa.Platform.Invoicing.Domain.Model.Entities;
using King.Nexa.Platform.Invoicing.Interfaces.Rest.Resources;

namespace King.Nexa.Platform.Invoicing.Interfaces.Rest.Transform;

public static class PaymentMethodRecordResourceAssembler
{
    public static PaymentMethodRecordResource ToResourceFromEntity(PaymentMethodRecord entity) =>
        new(
            entity.Id,
            entity.TenantId,
            entity.ClientAccountId,
            entity.Type,
            entity.Label,
            entity.Status,
            entity.IsDefault,
            entity.CreatedAt,
            entity.UpdatedAt);

    public static PaymentMethodRecord ToEntityFromResource(CreatePaymentMethodRecordResource resource) =>
        new()
        {
            ClientAccountId = resource.ClientAccountId,
            Type = resource.Type,
            Label = resource.Label,
            IsDefault = resource.IsDefault
        };
}

