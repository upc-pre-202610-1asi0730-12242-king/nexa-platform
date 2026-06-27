using King.Nexa.Platform.Sales.Application.CommandServices;
using King.Nexa.Platform.Sales.Application.QueryServices;
using King.Nexa.Platform.Sales.Interfaces.Rest.Resources;

namespace King.Nexa.Platform.Sales.Interfaces.Rest.Transform;

public static class PromotionResourceAssembler
{
    public static PromotionResource ToResourceFromSnapshot(PromotionSnapshot snapshot)
    {
        var promotion = snapshot.Promotion;
        return new PromotionResource(
            promotion.Id,
            promotion.TenantId,
            promotion.Code,
            promotion.Name,
            promotion.Campaign,
            promotion.Description,
            promotion.DiscountLabel,
            promotion.Visibility,
            promotion.CommercialRule,
            promotion.AdjustmentType,
            promotion.TargetSegment,
            promotion.Notes,
            promotion.CatalogScope,
            promotion.StartsOn,
            promotion.EndsOn,
            promotion.Status,
            snapshot.ProductIds,
            promotion.CreatedAt,
            promotion.UpdatedAt);
    }

    public static PromotionDraft ToDraftFromResource(UpsertPromotionResource resource) =>
        new(
            resource.Code,
            resource.Name,
            resource.Campaign,
            resource.Description,
            resource.DiscountLabel,
            resource.Visibility,
            resource.CommercialRule,
            resource.AdjustmentType,
            resource.TargetSegment,
            resource.Notes,
            resource.CatalogScope,
            resource.StartsOn ?? ParseDate(resource.StartDate),
            resource.EndsOn ?? ParseDate(resource.EndDate),
            resource.Status,
            resource.ProductIds);

    private static DateOnly? ParseDate(string? value) =>
        DateOnly.TryParse(value, out var date) ? date : null;
}

