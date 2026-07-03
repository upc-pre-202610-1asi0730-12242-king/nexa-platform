using King.Nexa.Platform.Sales.Domain.Model.Entities;

namespace King.Nexa.Platform.Sales.Application.QueryServices;

public interface IPromotionQueryService
{
    Task<IEnumerable<PromotionSnapshot>> ListAsync(CancellationToken cancellationToken = default);
    Task<PromotionSnapshot?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}

public record PromotionSnapshot(Promotion Promotion, IReadOnlyCollection<string> ProductIds);
