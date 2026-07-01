using King.Nexa.Platform.Shared.Domain.Model.Entities;

namespace King.Nexa.Platform.Invoicing.Domain.Model.Entities;

public class BusinessDocument : AuditableEntity, ITenantScoped
{
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "pending",
        "uploaded",
        "ready",
        "missing",
        "accepted"
    };

    public int TenantId { get; set; }
    public int? OrderId { get; set; }
    public int? ClientAccountId { get; set; }
    public int? DocumentTypeId { get; set; }
    public string Type { get; set; } = "factura_pdf";
    public string Label { get; set; } = string.Empty;
    public string Status { get; set; } = "pending";
    public string FileName { get; set; } = string.Empty;
    public bool VisibleToBuyer { get; set; }
    public bool Required { get; set; } = true;

    public void ChangeStatus(string status, bool? visibleToBuyer = null)
    {
        if (string.IsNullOrWhiteSpace(status)) throw new ArgumentException("Document status is required.", nameof(status));
        var nextStatus = status.Trim().ToLowerInvariant();
        if (!AllowedStatuses.Contains(nextStatus)) throw new InvalidOperationException("Business document status is not supported.");
        if (Status is "accepted" && nextStatus is not "accepted")
            throw new InvalidOperationException("Accepted business documents cannot move backwards.");
        if (nextStatus is "ready" && Status is not ("pending" or "uploaded" or "missing" or "ready"))
            throw new InvalidOperationException("Business document cannot be marked ready from its current status.");
        if (nextStatus is "missing" && !Required)
            throw new InvalidOperationException("Only required business documents can be marked missing.");
        Status = nextStatus;
        if (visibleToBuyer.HasValue) VisibleToBuyer = visibleToBuyer.Value;
        UpdatedAt = DateTime.UtcNow;
    }
}
