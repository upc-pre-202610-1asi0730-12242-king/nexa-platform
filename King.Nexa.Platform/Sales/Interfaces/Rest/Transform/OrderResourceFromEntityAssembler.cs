using King.Nexa.Platform.Sales.Domain.Model.Aggregates;
using King.Nexa.Platform.Sales.Interfaces.Rest.Resources;

namespace King.Nexa.Platform.Sales.Interfaces.Rest.Transform;

public static class OrderResourceFromEntityAssembler
{
    public static OrderResource ToResourceFromEntity(Order entity) =>
        new(
            entity.Id,
            entity.OrderNumber.Value,
            entity.CustomerId.Value,
            entity.ClientAccountId,
            entity.Status.ToString(),
            entity.Priority,
            entity.Notes,
            new DeliveryDetailsResource(
                entity.Delivery.AddressType,
                entity.Delivery.Address,
                entity.Delivery.District,
                entity.Delivery.City,
                entity.Delivery.Province,
                entity.Delivery.Reference,
                entity.Delivery.RequestedDate,
                entity.Delivery.DispatchNote),
            entity.Total.Amount,
            entity.Total.Currency,
            entity.PaymentConfirmation?.Value,
            entity.InventoryReservation?.Value,
            entity.RejectionReason?.Value,
            entity.ConfirmedAt,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.Items.Select(item => new OrderItemResource(
                item.Id,
                item.ProductId.Value,
                item.CatalogItemId.Value,
                item.ItemName.Value,
                item.Quantity.Value,
                item.UnitPrice.Amount,
                item.UnitPrice.Currency,
                item.Subtotal.Amount,
                item.Subtotal.Currency)).ToList());
}
