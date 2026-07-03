using King.Nexa.Platform.Invoicing.Domain.Model.Entities;
using King.Nexa.Platform.Invoicing.Interfaces.Rest.Resources;

namespace King.Nexa.Platform.Invoicing.Interfaces.Rest.Transform;

public static class BusinessDocumentResourceFromEntityAssembler
{
    public static BusinessDocumentResource ToResourceFromEntity(BusinessDocument entity) =>
        new(
            entity.Id,
            entity.TenantId,
            entity.OrderId,
            entity.ClientAccountId,
            entity.DocumentTypeId,
            entity.Type,
            entity.Label,
            entity.Status,
            entity.FileName,
            entity.VisibleToBuyer,
            entity.Required,
            entity.CreatedAt,
            entity.UpdatedAt);

    public static BusinessDocument ToEntityFromResource(CreateBusinessDocumentResource resource) =>
        new()
        {
            OrderId = resource.OrderId,
            ClientAccountId = resource.ClientAccountId,
            DocumentTypeId = resource.DocumentTypeId,
            Type = resource.Type,
            Label = resource.Label,
            FileName = resource.FileName ?? string.Empty,
            VisibleToBuyer = resource.VisibleToBuyer,
            Required = resource.Required
        };
}
