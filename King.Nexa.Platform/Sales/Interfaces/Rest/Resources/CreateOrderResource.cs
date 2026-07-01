namespace King.Nexa.Platform.Sales.Interfaces.Rest.Resources;

/// <summary>
/// Data required to create an order.
/// </summary>
public record CreateOrderResource(
    string OrderNumber,
    string CustomerId,
    IReadOnlyCollection<CreateOrderItemResource> Items,
    string? Priority,
    string? Notes,
    DeliveryDetailsResource? Delivery,
    int? ClientAccountId = null,
    decimal? ShippingEstimate = null);

public record DeliveryDetailsResource(
    string? AddressType,
    string? Address,
    string? District,
    string? City,
    string? Province,
    string? Reference,
    DateOnly? RequestedDate,
    string? DispatchNote);
