using King.Nexa.Platform.Shared.Domain.Model.Entities;

namespace King.Nexa.Platform.Sales.Domain.Model.Entities;

public class PurchaseRequestLine : AuditableEntity, ITenantScoped
{
    public int TenantId { get; set; }
    public int PurchaseRequestId { get; set; }
    public int CatalogItemId { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "box";
    public decimal EstimatedWeightKg { get; set; }
    public string Notes { get; set; } = string.Empty;
}
