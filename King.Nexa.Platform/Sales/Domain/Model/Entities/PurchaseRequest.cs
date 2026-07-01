using King.Nexa.Platform.Shared.Domain.Model.Entities;

namespace King.Nexa.Platform.Sales.Domain.Model.Entities;

public class PurchaseRequest : AuditableEntity, ITenantScoped
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
