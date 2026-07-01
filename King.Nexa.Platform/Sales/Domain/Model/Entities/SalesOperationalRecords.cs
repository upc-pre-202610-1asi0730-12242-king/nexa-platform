using King.Nexa.Platform.Shared.Domain.Model.Entities;

namespace King.Nexa.Platform.Sales.Domain.Model.Entities;

public class PurchaseRequest : AuditableEntity
{
    private static readonly HashSet<string> AllowedPaymentOptions = new(StringComparer.OrdinalIgnoreCase)
    {
        string.Empty,
        "credit_line",
        "bank_transfer",
        "cash",
        "cash_on_delivery"
    };

    public int TenantId { get; set; }
    public int ClientAccountId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Origin { get; set; } = "buyer_portal";
    public string Status { get; set; } = "submitted";
    public string Priority { get; set; } = "normal";
    public DateOnly? RequestedDeliveryDate { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    public string DeliveryDistrict { get; set; } = string.Empty;
    public string DeliveryCity { get; set; } = string.Empty;
    public string DeliveryProvince { get; set; } = string.Empty;
    public string DeliveryReference { get; set; } = string.Empty;
    public string PaymentOption { get; set; } = string.Empty;
    public decimal? ShippingEstimate { get; set; }
    public string Comments { get; set; } = string.Empty;
    public string CommercialOwner { get; set; } = string.Empty;

    public void ValidateStructuredFields()
    {
        PaymentOption = PaymentOption.Trim().ToLowerInvariant();
        if (!AllowedPaymentOptions.Contains(PaymentOption))
            throw new InvalidOperationException($"Unsupported payment option '{PaymentOption}'.");

        Priority = string.IsNullOrWhiteSpace(Priority) ? "normal" : Priority.Trim().ToLowerInvariant();
        if (Priority is not ("low" or "normal" or "high" or "urgent"))
            throw new InvalidOperationException($"Unsupported purchase request priority '{Priority}'.");

        var minimumDeliveryDate = DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(3);
        if (RequestedDeliveryDate.HasValue && RequestedDeliveryDate.Value < minimumDeliveryDate)
            throw new InvalidOperationException($"Requested delivery date must be on or after {minimumDeliveryDate:yyyy-MM-dd}.");
        if (ShippingEstimate < 0)
            throw new InvalidOperationException("Shipping estimate cannot be negative.");
    }

    public void ChangeStatus(string status, string? note = null, string? commercialOwner = null)
    {
        if (string.IsNullOrWhiteSpace(status)) throw new ArgumentException("Purchase request status is required.", nameof(status));
        var nextStatus = status.Trim().ToLowerInvariant();
        ValidateTransition(nextStatus);
        Status = nextStatus;
        if (!string.IsNullOrWhiteSpace(note)) Comments = note.Trim();
        if (!string.IsNullOrWhiteSpace(commercialOwner)) CommercialOwner = commercialOwner.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAcceptedIntoOrder(string orderNumber, string? note = null)
    {
        if (string.IsNullOrWhiteSpace(orderNumber)) throw new ArgumentException("Order number is required.", nameof(orderNumber));
        if (Status != "commercially_validated")
            throw new InvalidOperationException("Only commercially validated purchase requests can be accepted into an order.");
        Status = "converted_to_order";
        Comments = string.IsNullOrWhiteSpace(note) ? $"Accepted into order {orderNumber}." : note.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    private void ValidateTransition(string nextStatus)
    {
        if (Status is "converted_to_order")
            throw new InvalidOperationException("Accepted purchase requests already have an order and cannot change status.");

        if (nextStatus is "cancelled")
            return;

        var allowed = (Status, nextStatus) switch
        {
            ("submitted", "submitted") => true,
            ("submitted", "buyer_adjustment_requested") => true,
            ("submitted", "commercially_validated") => true,
            ("submitted", "rejected") => true,
            ("buyer_adjustment_requested", "submitted") => true,
            ("buyer_adjustment_requested", "commercially_validated") => true,
            ("buyer_adjustment_requested", "rejected") => true,
            ("commercially_validated", "rejected") => true,
            _ => false
        };

        if (!allowed)
            throw new InvalidOperationException($"Invalid purchase request transition from {Status} to {nextStatus}.");
    }
}

public class PurchaseRequestLine : AuditableEntity
{
    public int TenantId { get; set; }
    public int PurchaseRequestId { get; set; }
    public int CatalogItemId { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "box";
    public decimal EstimatedWeightKg { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class Promotion : AuditableEntity
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

public class PromotionCatalogItem : AuditableEntity
{
    public int TenantId { get; set; }
    public int PromotionId { get; set; }
    public int CatalogItemId { get; set; }
}

public class ConversationMessage : AuditableEntity
{
    public int TenantId { get; set; }
    public int? ClientAccountId { get; set; }
    public int? PurchaseRequestId { get; set; }
    public int? OrderId { get; set; }
    public string SenderRole { get; set; } = "commercial";
    public string SenderName { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool VisibleToBuyer { get; set; } = true;
}

public class CreditRequest : AuditableEntity
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
