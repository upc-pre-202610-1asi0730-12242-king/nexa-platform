namespace King.Nexa.Platform.Sales.Interfaces.Rest.Resources;

/// <summary>
/// Data required to update a pending order.
/// </summary>
public record UpdateOrderResource(
    string CustomerId,
    IReadOnlyCollection<CreateOrderItemResource> Items,
    string? Priority,
    string? Notes,
    DeliveryDetailsResource? Delivery,
    int? ClientAccountId = null);
