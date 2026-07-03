namespace King.Nexa.Platform.Sales.Interfaces.Rest.Resources;

/// <summary>
/// Order response resource.
/// </summary>
public record OrderResource(
    int Id,
    string OrderNumber,
    string CustomerId,
    int? ClientAccountId,
    string Status,
    string Priority,
    string Notes,
    DeliveryDetailsResource Delivery,
    decimal TotalAmount,
    string TotalCurrency,
    string? PaymentConfirmation,
    string? InventoryReservation,
    string? RejectionReason,
    DateTimeOffset? ConfirmedAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyCollection<OrderItemResource> Items);
