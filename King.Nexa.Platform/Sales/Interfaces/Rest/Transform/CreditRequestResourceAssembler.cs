using King.Nexa.Platform.Sales.Domain.Model.Entities;
using King.Nexa.Platform.Sales.Interfaces.Rest.Resources;

namespace King.Nexa.Platform.Sales.Interfaces.Rest.Transform;

public static class CreditRequestResourceAssembler
{
    public static CreditRequestResource ToResource(CreditRequest entity) => new(
        entity.Id, entity.TenantId, entity.ClientAccountId, entity.Code, entity.RequestedAmount,
        entity.Reason, entity.Status, entity.CreatedByUserId, entity.ReviewedBy,
        entity.ResolutionNote, entity.CreatedAt, entity.UpdatedAt);

    public static CreditRequest ToEntity(CreateCreditRequestResource resource) => new()
    {
        TenantId = resource.TenantId,
        ClientAccountId = resource.ClientAccountId,
        Code = resource.Code,
        RequestedAmount = resource.RequestedAmount,
        Reason = resource.Reason,
        CreatedByUserId = resource.CreatedByUserId,
    };
}

