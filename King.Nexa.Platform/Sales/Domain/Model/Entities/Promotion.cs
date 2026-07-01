using King.Nexa.Platform.Shared.Domain.Model.Entities;

namespace King.Nexa.Platform.Sales.Domain.Model.Entities;

public class Promotion : AuditableEntity, ITenantScoped
{
    public int TenantId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Campaign { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DiscountLabel { get; set; } = string.Empty;
    public string Visibility { get; set; } = "buyer_portal";
    public string CommercialRule { get; set; } = string.Empty;
    public string AdjustmentType { get; set; } = "percentage_discount";
    public string TargetSegment { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string CatalogScope { get; set; } = string.Empty;
    public DateOnly? StartsOn { get; set; }
    public DateOnly? EndsOn { get; set; }
    public string Status { get; set; } = "draft";
}
