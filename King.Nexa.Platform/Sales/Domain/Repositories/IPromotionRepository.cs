using King.Nexa.Platform.Sales.Domain.Model.Entities;
using King.Nexa.Platform.Shared.Domain.Repositories;

namespace King.Nexa.Platform.Sales.Domain.Repositories;

public interface IPromotionRepository : IBaseRepository<Promotion>
{
    Task<IReadOnlyCollection<int>> FindTenantCatalogItemIdsAsync(int tenantId, IReadOnlyCollection<string> productIds, CancellationToken cancellationToken = default);
    Task ReplaceCatalogItemsAsync(Promotion promotion, IReadOnlyCollection<int> catalogItemIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<int, IReadOnlyCollection<string>>> ListProductIdsByPromotionIdAsync(IReadOnlyCollection<int> promotionIds, CancellationToken cancellationToken = default);
}

