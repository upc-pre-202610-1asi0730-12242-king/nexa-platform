using King.Nexa.Platform.Sales.Domain.Model.Entities;
using King.Nexa.Platform.Sales.Domain.Model.Queries;
using King.Nexa.Platform.Shared.Application.Pagination;

namespace King.Nexa.Platform.Sales.Application.QueryServices;

public interface IPurchaseRequestQueryService
{
    Task<IEnumerable<PurchaseRequest>> ListAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<PurchaseRequest>> ListCommercialInboxAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<PurchaseRequest>> SearchAsync(PurchaseRequestCollectionQuery query, CancellationToken cancellationToken = default);
    Task<PurchaseRequest?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<PurchaseRequestLine>> ListLinesAsync(CancellationToken cancellationToken = default);
    Task<PurchaseRequestLine?> GetLineByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ConversationMessage>> ListMessagesAsync(CancellationToken cancellationToken = default);
    Task<ConversationMessage?> GetMessageByIdAsync(int id, CancellationToken cancellationToken = default);
}
