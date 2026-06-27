using King.Nexa.Platform.Sales.Domain.Model.Entities;
using King.Nexa.Platform.Sales.Domain.Repositories;
using King.Nexa.Platform.Shared.Application.Security;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace King.Nexa.Platform.Sales.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class PromotionRepository(AppDbContext context, ICurrentWorkspaceContext workspaceContext) : BaseRepository<Promotion>(context), IPromotionRepository
{
    public override async Task<Promotion?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await Scoped().FirstOrDefaultAsync(row => row.Id == id, cancellationToken);

    public override async Task<IEnumerable<Promotion>> ListAsync(CancellationToken cancellationToken = default) =>
        await Scoped()
            .OrderByDescending(row => row.UpdatedAt ?? row.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<int>> FindTenantCatalogItemIdsAsync(
        int tenantId,
        IReadOnlyCollection<string> productIds,
        CancellationToken cancellationToken = default)
    {
        var normalizedIds = productIds
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (normalizedIds.Count == 0) return [];

        return await Context.CatalogItems.AsNoTracking()
            .Where(row => row.TenantId == tenantId)
            .Where(row =>
                normalizedIds.Contains(row.ProductId.Value) ||
                normalizedIds.Contains(row.CatalogItemId.Value) ||
                normalizedIds.Contains(row.Id.ToString()))
            .Select(row => row.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task ReplaceCatalogItemsAsync(Promotion promotion, IReadOnlyCollection<int> catalogItemIds, CancellationToken cancellationToken = default)
    {
        var existing = await Context.PromotionCatalogItems
            .Where(row => row.TenantId == promotion.TenantId && row.PromotionId == promotion.Id)
            .ToListAsync(cancellationToken);
        Context.PromotionCatalogItems.RemoveRange(existing);

        foreach (var catalogItemId in catalogItemIds.Distinct())
        {
            await Context.PromotionCatalogItems.AddAsync(new PromotionCatalogItem
            {
                TenantId = promotion.TenantId,
                PromotionId = promotion.Id,
                CatalogItemId = catalogItemId
            }, cancellationToken);
        }
    }

    public async Task<IReadOnlyDictionary<int, IReadOnlyCollection<string>>> ListProductIdsByPromotionIdAsync(
        IReadOnlyCollection<int> promotionIds,
        CancellationToken cancellationToken = default)
    {
        if (promotionIds.Count == 0) return new Dictionary<int, IReadOnlyCollection<string>>();

        var mappings = await Context.PromotionCatalogItems.AsNoTracking()
            .Where(row => promotionIds.Contains(row.PromotionId))
            .ToListAsync(cancellationToken);
        var catalogIds = mappings.Select(row => row.CatalogItemId).Distinct().ToList();
        var products = await Context.CatalogItems.AsNoTracking()
            .Where(row => catalogIds.Contains(row.Id))
            .ToDictionaryAsync(row => row.Id, row => row.ProductId.Value, cancellationToken);

        return mappings
            .GroupBy(row => row.PromotionId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyCollection<string>)group
                    .Select(row => products.GetValueOrDefault(row.CatalogItemId))
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Cast<string>()
                    .ToArray());
    }

    private IQueryable<Promotion> Scoped()
    {
        var query = Context.Promotions.AsQueryable();
        return workspaceContext.TenantId is { } tenantId
            ? query.Where(row => row.TenantId == tenantId)
            : query.Where(_ => false);
    }
}
