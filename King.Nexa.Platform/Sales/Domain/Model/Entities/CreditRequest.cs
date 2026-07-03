using King.Nexa.Platform.Shared.Domain.Model.Entities;

namespace King.Nexa.Platform.Sales.Domain.Model.Entities;

public class CreditRequest : AuditableEntity, ITenantScoped
{
    public int TenantId { get; set; }
    public int ClientAccountId { get; set; }
    public string Code { get; set; } = string.Empty;
    public decimal RequestedAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "submitted";
    public int? CreatedByUserId { get; set; }
    public string ReviewedBy { get; set; } = string.Empty;
    public string ResolutionNote { get; set; } = string.Empty;

    public void Validate()
    {
        if (ClientAccountId <= 0) throw new InvalidOperationException("Client account is required.");
        if (RequestedAmount <= 0) throw new InvalidOperationException("Requested credit amount must be positive.");
        if (string.IsNullOrWhiteSpace(Reason)) throw new InvalidOperationException("Credit request reason is required.");
        Code = string.IsNullOrWhiteSpace(Code) ? $"CRQ-{Guid.NewGuid():N}"[..16].ToUpperInvariant() : Code.Trim().ToUpperInvariant();
        Reason = Reason.Trim();
    }

    public void Resolve(string status, string reviewedBy, string note)
    {
        var next = status.Trim().ToLowerInvariant();
        if (Status != "submitted") throw new InvalidOperationException("Only submitted credit requests can be resolved.");
        if (next is not ("approved" or "rejected" or "cancelled")) throw new InvalidOperationException("Unsupported credit request resolution.");
        Status = next;
        ReviewedBy = reviewedBy?.Trim() ?? string.Empty;
        ResolutionNote = note?.Trim() ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
    }
}
