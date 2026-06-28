using King.Nexa.Platform.Sales.Domain.Model.Entities;

namespace King.Nexa.Platform.Sales.Application.CommandServices;

public interface IPurchaseRequestCommandService
{
    Task<PurchaseRequest> CreateAsync(PurchaseRequest request, CancellationToken cancellationToken = default);
    Task<PurchaseRequest?> UpdateAsync(int id, PurchaseRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<PurchaseRequest?> SubmitAsync(int id, string? note, CancellationToken cancellationToken = default);
    Task<PurchaseRequest?> RequestAdjustmentAsync(int id, string? note, CancellationToken cancellationToken = default);
    Task<PurchaseRequest?> RejectAsync(int id, string? note, CancellationToken cancellationToken = default);
    Task<PurchaseRequest?> ValidateCommerciallyAsync(int id, string? commercialOwner, string? comments, CancellationToken cancellationToken = default);
    Task<PurchaseRequest?> CancelAsync(int id, string? note, CancellationToken cancellationToken = default);
    Task<OrderAcceptanceResult?> AcceptIntoOrderAsync(int id, string? note, CancellationToken cancellationToken = default);
    Task<ConversationMessage?> CreateMessageAsync(int id, PurchaseRequestMessageDraft draft, CancellationToken cancellationToken = default);
    Task<ConversationMessage> CreateMessageAsync(ConversationMessage draft, CancellationToken cancellationToken = default);
    Task<ConversationMessage?> UpdateMessageAsync(int id, ConversationMessage draft, CancellationToken cancellationToken = default);
    Task<bool> DeleteMessageAsync(int id, CancellationToken cancellationToken = default);
    Task<PurchaseRequestLine> CreateLineAsync(PurchaseRequestLine line, CancellationToken cancellationToken = default);
    Task<PurchaseRequestLine?> UpdateLineAsync(int id, PurchaseRequestLine draft, CancellationToken cancellationToken = default);
    Task<bool> DeleteLineAsync(int id, CancellationToken cancellationToken = default);
    Task<PurchaseRequestReservationResult?> CreateReservationAsync(int id, PurchaseRequestReservationDraft draft, CancellationToken cancellationToken = default);
}

public record OrderAcceptanceResult(int PurchaseRequestId, int OrderId, int? DispatchOrderId, string Status);

public record PurchaseRequestMessageDraft(string Body, string? SenderRole, string? SenderName, bool? VisibleToBuyer);

public record PurchaseRequestReservationDraft(string? ExternalId, int? InventoryItemId, string? ProductId, string? LotCode, int Units);

public record PurchaseRequestReservationResult(int Id, string ExternalId, string Status);
