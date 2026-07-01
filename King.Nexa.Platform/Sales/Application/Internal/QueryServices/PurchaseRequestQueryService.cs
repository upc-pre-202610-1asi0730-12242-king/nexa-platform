using King.Nexa.Platform.Sales.Application.QueryServices;
using King.Nexa.Platform.Sales.Domain.Model.Entities;
using King.Nexa.Platform.Sales.Domain.Model.Queries;
using King.Nexa.Platform.Sales.Domain.Repositories;
using King.Nexa.Platform.Shared.Application.Pagination;

namespace King.Nexa.Platform.Sales.Application.Internal.QueryServices;

public class PurchaseRequestQueryService(
    IPurchaseRequestRepository purchaseRequestRepository) : IPurchaseRequestQueryService
{
    public async Task<IEnumerable<PurchaseRequest>> ListAsync(CancellationToken cancellationToken = default) =>
        await purchaseRequestRepository.ListAsync(cancellationToken);

    public async Task<IEnumerable<PurchaseRequest>> ListCommercialInboxAsync(CancellationToken cancellationToken = default) =>
        await purchaseRequestRepository.ListCommercialInboxAsync(cancellationToken);

    public Task<PagedResult<PurchaseRequest>> SearchAsync(PurchaseRequestCollectionQuery query, CancellationToken cancellationToken = default) =>
        purchaseRequestRepository.SearchAsync(query, cancellationToken);

    public Task<PurchaseRequest?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        purchaseRequestRepository.FindByIdAsync(id, cancellationToken);

    public async Task<IEnumerable<PurchaseRequestLine>> ListLinesAsync(CancellationToken cancellationToken = default) =>
        await purchaseRequestRepository.ListLinesAsync(cancellationToken);

    public Task<PurchaseRequestLine?> GetLineByIdAsync(int id, CancellationToken cancellationToken = default) =>
        purchaseRequestRepository.FindLineByIdAsync(id, cancellationToken);

    public async Task<IEnumerable<ConversationMessage>> ListMessagesAsync(CancellationToken cancellationToken = default) =>
        await purchaseRequestRepository.ListMessagesAsync(cancellationToken);

    public Task<ConversationMessage?> GetMessageByIdAsync(int id, CancellationToken cancellationToken = default) =>
        purchaseRequestRepository.FindMessageByIdAsync(id, cancellationToken);
}
