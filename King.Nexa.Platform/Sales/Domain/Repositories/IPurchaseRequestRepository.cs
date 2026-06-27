using King.Nexa.Platform.Sales.Domain.Model.Entities;

namespace King.Nexa.Platform.Sales.Domain.Repositories;

public interface IPurchaseRequestRepository
{
    Task AddAsync(PurchaseRequest request, CancellationToken cancellationToken = default);
    Task<PurchaseRequest?> FindByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<PurchaseRequest>> ListAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<PurchaseRequest>> ListCommercialInboxAsync(CancellationToken cancellationToken = default);
    void Remove(PurchaseRequest request);
    Task AddLineAsync(PurchaseRequestLine line, CancellationToken cancellationToken = default);
    Task<PurchaseRequestLine?> FindLineByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<PurchaseRequestLine>> ListLinesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PurchaseRequestLine>> ListLinesByRequestIdAsync(int requestId, CancellationToken cancellationToken = default);
    void RemoveLine(PurchaseRequestLine line);
    Task AddMessageAsync(ConversationMessage message, CancellationToken cancellationToken = default);
    Task<ConversationMessage?> FindMessageByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ConversationMessage>> ListMessagesAsync(CancellationToken cancellationToken = default);
    void RemoveMessage(ConversationMessage message);
    Task<bool> RequestBelongsToTenantAsync(int tenantId, int requestId, CancellationToken cancellationToken = default);
}
