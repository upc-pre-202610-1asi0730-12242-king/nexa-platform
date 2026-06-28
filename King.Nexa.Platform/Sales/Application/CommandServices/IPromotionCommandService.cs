using King.Nexa.Platform.Sales.Domain.Model.Entities;

namespace King.Nexa.Platform.Sales.Application.CommandServices;

public interface IPromotionCommandService
{
    Task<Promotion> CreateAsync(PromotionDraft draft, CancellationToken cancellationToken = default);
    Task<Promotion?> UpdateAsync(int id, PromotionDraft draft, CancellationToken cancellationToken = default);
    Task<Promotion?> ChangeStatusAsync(int id, string status, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}

public record PromotionDraft(
    string? Code,
    string? Name,
    string? Campaign,
    string? Description,
    string? DiscountLabel,
    string? Visibility,
    string? CommercialRule,
    string? AdjustmentType,
    string? TargetSegment,
    string? Notes,
    string? CatalogScope,
    DateOnly? StartsOn,
    DateOnly? EndsOn,
    string? Status,
    IReadOnlyCollection<string>? ProductIds);
