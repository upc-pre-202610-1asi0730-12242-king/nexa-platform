using King.Nexa.Platform.Sales.Domain.Model.Commands;
using King.Nexa.Platform.Sales.Domain.Model.ValueObjects;
using King.Nexa.Platform.Sales.Interfaces.Rest.Resources;

namespace King.Nexa.Platform.Sales.Interfaces.Rest.Transform;

public static class CreateOrderCommandFromResourceAssembler
{
    public static CreateOrderCommand ToCommandFromResource(CreateOrderResource resource) =>
        new(
            new OrderNumber(resource.OrderNumber),
            new CustomerId(resource.CustomerId),
            resource.Items.Select(item => new CreateOrderItemCommand(
                new ProductId(item.ProductId),
                new CatalogItemId(item.CatalogItemId),
                new ItemName(item.ItemName),
                new Quantity(item.Quantity),
                new Money(item.UnitPriceAmount, item.UnitPriceCurrency))).ToList(),
            resource.Priority ?? "medium",
            resource.Notes ?? string.Empty,
            ToValueObject(resource.Delivery),
            resource.ClientAccountId,
            resource.ShippingEstimate);

    public static DeliveryDetails ToValueObject(DeliveryDetailsResource? resource) => resource is null
        ? DeliveryDetails.Empty()
        : new DeliveryDetails(
            resource.AddressType,
            resource.Address,
            resource.District,
            resource.City,
            resource.Province,
            resource.Reference,
            resource.RequestedDate,
            resource.DispatchNote);
}
