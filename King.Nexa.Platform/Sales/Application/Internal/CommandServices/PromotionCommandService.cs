using King.Nexa.Platform.Sales.Application.CommandServices;
using King.Nexa.Platform.Sales.Domain.Model.Entities;
using King.Nexa.Platform.Sales.Domain.Repositories;
using King.Nexa.Platform.Shared.Application.Security;
using King.Nexa.Platform.Shared.Domain.Repositories;

namespace King.Nexa.Platform.Sales.Application.Internal.CommandServices;

public class PromotionCommandService(
    IPromotionRepository promotionRepository,
    IUnitOfWork unitOfWork,
    ICurrentWorkspaceContext workspaceContext) : IPromotionCommandService
{
    private static readonly HashSet<string> SupportedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "active", "scheduled", "draft", "standby", "inactive", "paused", "archived"
    };

    public async Task<Promotion> CreateAsync(PromotionDraft draft, CancellationToken cancellationToken = default)
    {
        var tenantId = workspaceContext.TenantId ?? throw new InvalidOperationException("Current tenant is required.");
        if (string.IsNullOrWhiteSpace(draft.Name)) throw new InvalidOperationException("Promotion name is required.");
        var promotion = new Promotion { TenantId = tenantId };
        Apply(promotion, draft, true);
        await promotionRepository.AddAsync(promotion, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        await SyncProductsAsync(promotion, draft.ProductIds ?? [], cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return promotion;
    }

    public async Task<Promotion?> UpdateAsync(int id, PromotionDraft draft, CancellationToken cancellationToken = default)
    {
        var promotion = await FindScopedAsync(id, cancellationToken);
        if (promotion is null) return null;
        Apply(promotion, draft, false);
        if (draft.ProductIds is not null)
            await SyncProductsAsync(promotion, draft.ProductIds, cancellationToken);
        promotion.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.CompleteAsync(cancellationToken);
        return promotion;
    }

    public async Task<Promotion?> ChangeStatusAsync(int id, string status, CancellationToken cancellationToken = default)
    {
        var promotion = await FindScopedAsync(id, cancellationToken);
        if (promotion is null) return null;
        var nextStatus = NormalizeStatus(status);
        ValidateDates(promotion.StartsOn, promotion.EndsOn);
        promotion.Status = nextStatus;
        promotion.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.CompleteAsync(cancellationToken);
        return promotion;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var promotion = await FindScopedAsync(id, cancellationToken);
        if (promotion is null) return false;
        promotionRepository.Remove(promotion);
        await unitOfWork.CompleteAsync(cancellationToken);
        return true;
    }

    private Task<Promotion?> FindScopedAsync(int id, CancellationToken cancellationToken) =>
        promotionRepository.FindByIdAsync(id, cancellationToken);

    private static void Apply(Promotion promotion, PromotionDraft draft, bool creating)
    {
        promotion.Code = Value(draft.Code, promotion.Code, creating ? $"PROM-{Guid.NewGuid():N}"[..13].ToUpperInvariant() : promotion.Code);
        promotion.Name = Value(draft.Name, promotion.Name);
        promotion.Campaign = Value(draft.Campaign, promotion.Campaign);
        promotion.Description = Value(draft.Description, promotion.Description);
        promotion.DiscountLabel = Value(draft.DiscountLabel, promotion.DiscountLabel);
        promotion.Visibility = Value(draft.Visibility, promotion.Visibility, "buyer_portal").ToLowerInvariant();
        promotion.CommercialRule = Value(draft.CommercialRule, promotion.CommercialRule);
        promotion.AdjustmentType = Value(draft.AdjustmentType, promotion.AdjustmentType, "percentage_discount").ToLowerInvariant();
        promotion.TargetSegment = Value(draft.TargetSegment, promotion.TargetSegment);
        promotion.Notes = Value(draft.Notes, promotion.Notes);
        promotion.CatalogScope = Value(draft.CatalogScope, promotion.CatalogScope);
        promotion.StartsOn = draft.StartsOn ?? promotion.StartsOn;
        promotion.EndsOn = draft.EndsOn ?? promotion.EndsOn;
        promotion.Status = draft.Status is null ? promotion.Status : NormalizeStatus(draft.Status);
        ValidateDates(promotion.StartsOn, promotion.EndsOn);
    }

    private async Task SyncProductsAsync(Promotion promotion, IReadOnlyCollection<string> productIds, CancellationToken cancellationToken)
    {
        var normalizedIds = productIds
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var catalogItemIds = await promotionRepository.FindTenantCatalogItemIdsAsync(promotion.TenantId, normalizedIds, cancellationToken);
        if (catalogItemIds.Count != normalizedIds.Count)
            throw new InvalidOperationException("One or more promotion products do not belong to the current tenant catalog.");

        await promotionRepository.ReplaceCatalogItemsAsync(promotion, catalogItemIds, cancellationToken);
    }

    private static string NormalizeStatus(string status)
    {
        var value = status.Trim().ToLowerInvariant();
        if (!SupportedStatuses.Contains(value)) throw new InvalidOperationException("Promotion status is not supported.");
        return value;
    }

    private static void ValidateDates(DateOnly? startsOn, DateOnly? endsOn)
    {
        if (startsOn.HasValue && endsOn.HasValue && startsOn.Value > endsOn.Value)
            throw new InvalidOperationException("Promotion start date cannot be after end date.");
    }

    private static string Value(string? incoming, string current, string fallback = "") =>
        incoming is null ? (string.IsNullOrWhiteSpace(current) ? fallback : current) : incoming.Trim();
}

