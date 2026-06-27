namespace King.Nexa.Platform.Sales.Interfaces.Rest.Resources;

public record PromotionResource(
    int Id,
    int TenantId,
    string Code,
    string Name,
    string Campaign,
    string Description,
    string DiscountLabel,
    string Visibility,
    string CommercialRule,
    string AdjustmentType,
    string TargetSegment,
    string Notes,
    string CatalogScope,
    DateOnly? StartsOn,
    DateOnly? EndsOn,
    string Status,
    IReadOnlyCollection<string> ProductIds,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public class UpsertPromotionResource
{
    public string? Code { get; init; }
    public string? Name { get; init; }
    public string? Campaign { get; init; }
    public string? Description { get; init; }
    public string? DiscountLabel { get; init; }
    public string? Visibility { get; init; }
    public string? CommercialRule { get; init; }
    public string? AdjustmentType { get; init; }
    public string? TargetSegment { get; init; }
    public string? Notes { get; init; }
    public string? CatalogScope { get; init; }
    public DateOnly? StartsOn { get; init; }
    public DateOnly? EndsOn { get; init; }
    public string? StartDate { get; init; }
    public string? EndDate { get; init; }
    public string? Status { get; init; }
    public IReadOnlyCollection<string>? ProductIds { get; init; }
}

