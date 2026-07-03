using King.Nexa.Platform.Shared.Domain.Model.Entities;

namespace King.Nexa.Platform.Invoicing.Domain.Model.Entities;

public class PaymentProcessRecord : AuditableEntity, ITenantScoped
{
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "pending",
        "confirmed",
        "failed"
    };

    public int TenantId { get; set; }
    public int? OrderId { get; set; }
    public int? ClientAccountId { get; set; }
    public int? PaymentId { get; set; }
    public int? PaymentMethodRecordId { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Shipping { get; set; }
    public decimal Igv { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = "pending";

    public void ChangeStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status)) throw new ArgumentException("Payment status is required.", nameof(status));
        var nextStatus = status.Trim().ToLowerInvariant();
        if (!AllowedStatuses.Contains(nextStatus)) throw new InvalidOperationException("Payment status is not supported.");
        if (Status is "confirmed" && nextStatus is not "confirmed")
            throw new InvalidOperationException("Confirmed payment processes cannot move backwards.");
        if (Status is "failed" && nextStatus is "confirmed")
            throw new InvalidOperationException("Failed payment processes must be reviewed before confirmation.");

        Status = nextStatus;
        UpdatedAt = DateTime.UtcNow;
    }
}
