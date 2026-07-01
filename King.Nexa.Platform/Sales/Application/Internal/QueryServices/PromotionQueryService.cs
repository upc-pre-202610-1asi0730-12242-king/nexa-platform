using King.Nexa.Platform.Sales.Application.QueryServices;
using King.Nexa.Platform.Sales.Domain.Model.Entities;
using King.Nexa.Platform.Sales.Domain.Repositories;

namespace King.Nexa.Platform.Sales.Application.Internal.QueryServices;

public class PromotionQueryService(
    IPromotionRepository promotionRepository) : IPromotionQueryService
{
    public async Task<IEnumerable<PromotionSnapshot>> ListAsync(CancellationToken cancellationToken = default)
    {
        var promotions = (await promotionRepository.ListAsync(cancellationToken)).ToArray();
        return await BuildSnapshotsAsync(promotions, cancellationToken);
    }

    public async Task<PromotionSnapshot?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var promotion = await promotionRepository.FindByIdAsync(id, cancellationToken);
        if (promotion is null) return null;
        return (await BuildSnapshotsAsync([promotion], cancellationToken)).Single();
    }

    private async Task<IReadOnlyCollection<PromotionSnapshot>> BuildSnapshotsAsync(
        IReadOnlyCollection<Promotion> promotions,
        CancellationToken cancellationToken)
    {
        if (promotions.Count == 0) return [];
        var promotionIds = promotions.Select(row => row.Id).ToList();
        var productIdsByPromotion = await promotionRepository.ListProductIdsByPromotionIdAsync(promotionIds, cancellationToken);
        return promotions.Select(promotion => new PromotionSnapshot(
                promotion,
                productIdsByPromotion.GetValueOrDefault(promotion.Id, [])))
            .ToArray();
    }
}
