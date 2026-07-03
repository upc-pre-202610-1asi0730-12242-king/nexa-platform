namespace King.Nexa.Platform.Sales.Interfaces.Rest.Resources;

public record PurchaseRequestResource(
    int Id,
    int TenantId,
    int ClientAccountId,
    string Code,
    string Origin,
    string Status,
    string Priority,
    DateOnly? RequestedDeliveryDate,
    string DeliveryAddress,
    string DeliveryDistrict,
    string DeliveryCity,
    string DeliveryProvince,
    string DeliveryReference,
    string PaymentOption,
    decimal? ShippingEstimate,
    string Comments,
    string CommercialOwner,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record PurchaseRequestLineResource(
    int Id,
    int TenantId,
    int PurchaseRequestId,
    int CatalogItemId,
    decimal Quantity,
    string Unit,
    decimal EstimatedWeightKg,
    string Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public class UpsertPurchaseRequestLineResource
{
    public int TenantId { get; init; }
    public int PurchaseRequestId { get; init; }
    public int CatalogItemId { get; init; }
    public decimal Quantity { get; init; }
    public string Unit { get; init; } = "UN";
    public decimal EstimatedWeightKg { get; init; }
    public string Notes { get; init; } = string.Empty;
}

public class UpsertPurchaseRequestResource
{
    public int TenantId { get; init; }
    public int ClientAccountId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Origin { get; init; } = "buyer_portal";
    public string Status { get; init; } = "submitted";
    public string Priority { get; init; } = "normal";
    public DateOnly? RequestedDeliveryDate { get; init; }
    public string DeliveryAddress { get; init; } = string.Empty;
    public string DeliveryDistrict { get; init; } = string.Empty;
    public string DeliveryCity { get; init; } = string.Empty;
    public string DeliveryProvince { get; init; } = string.Empty;
    public string DeliveryReference { get; init; } = string.Empty;
    public string PaymentOption { get; init; } = string.Empty;
    public decimal? ShippingEstimate { get; init; }
    public string Comments { get; init; } = string.Empty;
    public string CommercialOwner { get; init; } = string.Empty;
}

public record RequestNoteResource(string? Note);

public record RequestOwnerResource(string? CommercialOwner, string? Comments);

public record PurchaseRequestMessageResource(string Body, string? SenderRole, string? SenderName, bool? VisibleToBuyer);

public record ConversationMessageResource(
    int Id,
    int TenantId,
    int? ClientAccountId,
    int? PurchaseRequestId,
    int? OrderId,
    string SenderRole,
    string SenderName,
    string Body,
    bool VisibleToBuyer,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public class UpsertConversationMessageResource
{
    public int TenantId { get; init; }
    public int? ClientAccountId { get; init; }
    public int? PurchaseRequestId { get; init; }
    public int? OrderId { get; init; }
    public string SenderRole { get; init; } = "commercial";
    public string SenderName { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public bool VisibleToBuyer { get; init; } = true;
}

public record OrderAcceptanceResource(int PurchaseRequestId, int OrderId, int? DispatchOrderId, string Status);

public record ReservationRequestResource(string? Id, int? InventoryItemId, string? ProductId, string? LotCode, int Units);

public record ReservationResource(int Id, string ExternalId, string Status);
