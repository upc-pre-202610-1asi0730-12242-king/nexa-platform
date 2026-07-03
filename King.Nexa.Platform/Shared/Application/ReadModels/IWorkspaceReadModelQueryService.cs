using King.Nexa.Platform.Shared.Application.Pagination;

namespace King.Nexa.Platform.Shared.Application.ReadModels;

public interface IWorkspaceReadModelQueryService
{
    Task<BuyerDashboardSummaryReadModel> GetBuyerDashboardSummaryAsync(CancellationToken cancellationToken = default);
    Task<OrderLifecycleReadModel?> GetBuyerOrderLifecycleAsync(int orderId, CancellationToken cancellationToken = default);
    Task<ClientFinancialProfileReadModel?> GetBuyerFinancialProfileAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<OrderSummaryReadModel>> GetSalesOrderSummariesAsync(PaginationRequest pagination, CancellationToken cancellationToken = default);
    Task<PagedResult<PurchaseRequestInboxReadModel>> GetSalesPurchaseRequestInboxAsync(PaginationRequest pagination, CancellationToken cancellationToken = default);
    Task<ClientFinancialProfileReadModel?> GetClientFinancialProfileAsync(int clientAccountId, CancellationToken cancellationToken = default);
    Task<DispatchOrderSummaryReadModel?> GetDispatchOrderSummaryAsync(int dispatchOrderId, CancellationToken cancellationToken = default);
    Task<OrderTimelineReadModel?> GetOrderTimelineAsync(int orderId, CancellationToken cancellationToken = default);
    Task<CatalogItemAvailabilityReadModel?> GetCatalogItemAvailabilityAsync(int catalogItemId, CancellationToken cancellationToken = default);
    Task<PagedResult<PromotionalCatalogItemReadModel>> GetPromotionalCatalogAsync(PaginationRequest pagination, CancellationToken cancellationToken = default);
}

public sealed record ClientSummaryReadModel(int Id, string Code, string BusinessName, string CommercialName);

public sealed record OrderLineReadModel(int Id, string ProductId, string CatalogItemId, string ItemName, int Quantity, decimal UnitPrice, decimal Subtotal);

public sealed record OrderSummaryReadModel(
    int Id,
    string OrderNumber,
    string Status,
    ClientSummaryReadModel? Client,
    decimal Total,
    string Currency,
    DateTime CreatedAt,
    DateOnly? RequestedDeliveryDate,
    string? DispatchStatus,
    string? PaymentStatus,
    int ItemCount);

public sealed record PurchaseRequestInboxReadModel(
    int Id,
    string Code,
    ClientSummaryReadModel? Client,
    string Status,
    string Priority,
    DateTime CreatedAt,
    DateOnly? RequestedDeliveryDate,
    int LineCount,
    string? LastMessagePreview,
    string CommercialOwner);

public sealed record NotificationPreviewReadModel(int Id, string Title, string Body, bool Read, DateTime CreatedAt);

public sealed record BuyerDashboardSummaryReadModel(
    int ActivePurchaseRequestsCount,
    int ActiveOrdersCount,
    int PendingDocumentsCount,
    int PendingInvoicesCount,
    IReadOnlyCollection<PurchaseRequestInboxReadModel> RecentRequests,
    IReadOnlyCollection<OrderSummaryReadModel> RecentOrders,
    IReadOnlyCollection<NotificationPreviewReadModel> Notifications,
    CreditSummaryReadModel? CreditSummary);

public sealed record CreditSummaryReadModel(decimal CreditLimit, decimal UsedCredit, decimal AvailableCredit, string Status, bool Estimated);

public sealed record BusinessDocumentPreviewReadModel(int Id, string Type, string Label, string Status, bool VisibleToBuyer, bool Required);

public sealed record InvoicePreviewReadModel(int Id, string InvoiceNumber, decimal Amount, string Currency, string PaymentStatus);

public sealed record PaymentPreviewReadModel(int Id, string ReferenceCode, decimal Amount, string Currency, string Status);

public sealed record DispatchEventTimelineReadModel(int Id, string Status, string Description, bool VisibleToBuyer, DateTime CreatedAt);

public sealed record TemperatureReadingReadModel(int Id, decimal Celsius, string Zone, string Status, DateTime RecordedAt);

public sealed record OrderLifecycleReadModel(
    OrderSummaryReadModel Order,
    IReadOnlyCollection<OrderLineReadModel> Items,
    IReadOnlyCollection<DispatchOrderSummaryReadModel> Dispatches,
    IReadOnlyCollection<DispatchEventTimelineReadModel> DispatchEvents,
    IReadOnlyCollection<TemperatureReadingReadModel> TemperatureLogs,
    IReadOnlyCollection<BusinessDocumentPreviewReadModel> BusinessDocuments,
    IReadOnlyCollection<InvoicePreviewReadModel> Invoices,
    IReadOnlyCollection<PaymentPreviewReadModel> Payments);

public sealed record ClientFinancialProfileReadModel(
    ClientSummaryReadModel Client,
    CreditSummaryReadModel Credit,
    int OpenOrders,
    IReadOnlyCollection<InvoicePreviewReadModel> PendingInvoices,
    IReadOnlyCollection<PaymentPreviewReadModel> RecentPayments,
    int PaymentMethodsCount,
    int DocumentsCount);

public sealed record DispatchOrderSummaryReadModel(
    int Id,
    string Code,
    string Status,
    string RouteName,
    string Responsible,
    DateTime? Eta,
    string DeliveryWindow,
    OrderSummaryReadModel? LinkedOrder,
    ClientSummaryReadModel? Client,
    DispatchEventTimelineReadModel? LastEvent,
    IReadOnlyCollection<DispatchEventTimelineReadModel> Events,
    string? ProofOfDeliveryStatus,
    TemperatureReadingReadModel? LatestTemperatureReading);

public sealed record TimelineEventReadModel(string Source, string Status, string Description, DateTime OccurredAt);

public sealed record OrderTimelineReadModel(int OrderId, string OrderNumber, IReadOnlyCollection<TimelineEventReadModel> Events);

public sealed record LotSummaryReadModel(int Id, string LotCode, int Quantity, int ReservedQuantity, DateOnly? ExpirationDate, string Status);

public sealed record CatalogItemAvailabilityReadModel(
    int CatalogItemId,
    string ProductId,
    string ItemName,
    int CatalogAvailableStock,
    int? InventoryItemId,
    int? AvailableStock,
    int? ReservedStock,
    IReadOnlyCollection<LotSummaryReadModel> Lots,
    string ColdChainRequirement,
    string? LastMovementType);

public sealed record PromotionalCatalogItemReadModel(
    int Id,
    string ProductId,
    string ItemName,
    string BrandName,
    string CategoryName,
    decimal UnitPrice,
    string Currency,
    string? ActivePromotionCode,
    string? ActivePromotionLabel,
    int AvailableStock);
