using King.Nexa.Platform.Shared.Domain.Model.Entities;

namespace King.Nexa.Platform.Sales.Domain.Model.Entities;

public class PromotionCatalogItem : AuditableEntity, ITenantScoped
{
    public int TenantId { get; set; }
    public int PromotionId { get; set; }
    public int CatalogItemId { get; set; }
}
