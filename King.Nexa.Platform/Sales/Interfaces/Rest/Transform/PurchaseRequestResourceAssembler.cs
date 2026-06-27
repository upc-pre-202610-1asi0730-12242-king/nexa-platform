using King.Nexa.Platform.Sales.Domain.Model.Entities;
using King.Nexa.Platform.Sales.Interfaces.Rest.Resources;

namespace King.Nexa.Platform.Sales.Interfaces.Rest.Transform;

public static class PurchaseRequestResourceAssembler
{
    public static PurchaseRequestResource ToResourceFromEntity(PurchaseRequest request) =>
        new(
            request.Id,
            request.TenantId,
            request.ClientAccountId,
            request.Code,
            request.Origin,
            request.Status,
            request.Priority,
            request.RequestedDeliveryDate,
            request.DeliveryAddress,
            request.DeliveryDistrict,
            request.DeliveryCity,
            request.DeliveryProvince,
            request.DeliveryReference,
            request.PaymentOption,
            request.ShippingEstimate,
            request.Comments,
            request.CommercialOwner,
            request.CreatedAt,
            request.UpdatedAt);

    public static PurchaseRequest ToEntityFromResource(UpsertPurchaseRequestResource resource) =>
        new()
        {
            TenantId = resource.TenantId,
            ClientAccountId = resource.ClientAccountId,
            Code = resource.Code,
            Origin = resource.Origin,
            Status = resource.Status,
            Priority = resource.Priority,
            RequestedDeliveryDate = resource.RequestedDeliveryDate,
            DeliveryAddress = resource.DeliveryAddress,
            DeliveryDistrict = resource.DeliveryDistrict,
            DeliveryCity = resource.DeliveryCity,
            DeliveryProvince = resource.DeliveryProvince,
            DeliveryReference = resource.DeliveryReference,
            PaymentOption = resource.PaymentOption,
            ShippingEstimate = resource.ShippingEstimate,
            Comments = resource.Comments,
            CommercialOwner = resource.CommercialOwner
        };

    public static PurchaseRequestLineResource ToResourceFromEntity(PurchaseRequestLine line) =>
        new(
            line.Id,
            line.TenantId,
            line.PurchaseRequestId,
            line.CatalogItemId,
            line.Quantity,
            line.Unit,
            line.EstimatedWeightKg,
            line.Notes,
            line.CreatedAt,
            line.UpdatedAt);

    public static PurchaseRequestLine ToEntityFromResource(UpsertPurchaseRequestLineResource resource) =>
        new()
        {
            TenantId = resource.TenantId,
            PurchaseRequestId = resource.PurchaseRequestId,
            CatalogItemId = resource.CatalogItemId,
            Quantity = resource.Quantity,
            Unit = resource.Unit,
            EstimatedWeightKg = resource.EstimatedWeightKg,
            Notes = resource.Notes
        };

    public static ConversationMessageResource ToResourceFromEntity(ConversationMessage message) =>
        new(
            message.Id,
            message.TenantId,
            message.ClientAccountId,
            message.PurchaseRequestId,
            message.OrderId,
            message.SenderRole,
            message.SenderName,
            message.Body,
            message.VisibleToBuyer,
            message.CreatedAt,
            message.UpdatedAt);

    public static ConversationMessage ToEntityFromResource(UpsertConversationMessageResource resource) =>
        new()
        {
            TenantId = resource.TenantId,
            ClientAccountId = resource.ClientAccountId,
            PurchaseRequestId = resource.PurchaseRequestId,
            OrderId = resource.OrderId,
            SenderRole = resource.SenderRole,
            SenderName = resource.SenderName,
            Body = resource.Body,
            VisibleToBuyer = resource.VisibleToBuyer
        };
}

