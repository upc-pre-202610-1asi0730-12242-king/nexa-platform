using King.Nexa.Platform.Sales.Domain.Model.Commands;
using King.Nexa.Platform.Sales.Domain.Model.Entities;
using King.Nexa.Platform.Sales.Domain.Model.ValueObjects;
using King.Nexa.Platform.Shared.Domain.Model.Entities;

namespace King.Nexa.Platform.Sales.Domain.Model.Aggregates;

/// <summary>
/// Aggregate root for customer orders and their order items.
/// </summary>
public class Order : AuditableEntity, ITenantScoped
{
    protected Order()
    {
        OrderNumber = null!;
        CustomerId = null!;
        Total = null!;
        Delivery = DeliveryDetails.Empty();
    }

    public Order(CreateOrderCommand command)
    {
        if (command.Items.Count == 0)
            throw new ArgumentException("An order must contain at least one item.", nameof(command));

        OrderNumber = command.OrderNumber;
        CustomerId = command.CustomerId;
        ClientAccountId = command.ClientAccountId;
        Items = command.Items.Select(item => new OrderItem(item)).ToList();
        Total = CalculateTotal(Items);
        Status = OrderStatus.Pending;
        Priority = NormalizePriority(command.Priority);
        Notes = command.Notes?.Trim() ?? string.Empty;
        Delivery = command.Delivery ?? DeliveryDetails.Empty();
    }

    public OrderNumber OrderNumber { get; private set; }

    public int TenantId { get; private set; }

    public CustomerId CustomerId { get; private set; }

    public int? ClientAccountId { get; private set; }

    public OrderStatus Status { get; private set; }

    public Money Total { get; private set; }

    public string Priority { get; private set; } = "medium";

    public string Notes { get; private set; } = string.Empty;

    public DeliveryDetails Delivery { get; private set; }

    public PaymentConfirmation? PaymentConfirmation { get; private set; }

    public InventoryReservation? InventoryReservation { get; private set; }

    public RejectionReason? RejectionReason { get; private set; }

    public DateTimeOffset? ConfirmedAt { get; private set; }

    public List<OrderItem> Items { get; private set; } = [];

    public void AssignTenant(int tenantId)
    {
        if (tenantId <= 0) throw new ArgumentException("Tenant id must be positive.", nameof(tenantId));
        TenantId = tenantId;
        foreach (var item in Items) item.AssignTenant(tenantId);
    }

    public void AssignClientAccount(int clientAccountId)
    {
        if (clientAccountId <= 0) throw new ArgumentException("Client account id must be positive.", nameof(clientAccountId));
        ClientAccountId = clientAccountId;
    }

    public void Update(UpdateOrderCommand command)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Only pending orders can be updated.");
        if (command.Items.Count == 0)
            throw new ArgumentException("An order must contain at least one item.", nameof(command));

        CustomerId = command.CustomerId;
        ClientAccountId = command.ClientAccountId ?? ClientAccountId;
        Items.Clear();
        Items.AddRange(command.Items.Select(item => new OrderItem(item)));
        Total = CalculateTotal(Items);
        Priority = NormalizePriority(command.Priority);
        Notes = command.Notes?.Trim() ?? string.Empty;
        Delivery = command.Delivery ?? DeliveryDetails.Empty();
    }

    public void Confirm(PaymentConfirmation paymentConfirmation, InventoryReservation inventoryReservation)
    {
        if (Status == OrderStatus.Cancelled) throw new InvalidOperationException("Cancelled orders cannot be confirmed.");
        if (Status == OrderStatus.Rejected) throw new InvalidOperationException("Rejected orders cannot be confirmed.");

        Status = OrderStatus.Confirmed;
        PaymentConfirmation = paymentConfirmation;
        InventoryReservation = inventoryReservation;
        ConfirmedAt = DateTimeOffset.UtcNow;
    }

    public void Reject(RejectionReason rejectionReason)
    {
        if (Status == OrderStatus.Confirmed || Status == OrderStatus.Paid)
            throw new InvalidOperationException("Confirmed or paid orders cannot be rejected.");

        Status = OrderStatus.Rejected;
        RejectionReason = rejectionReason;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Confirmed || Status == OrderStatus.Paid)
            throw new InvalidOperationException("Confirmed or paid orders cannot be cancelled.");

        Status = OrderStatus.Cancelled;
    }

    private static Money CalculateTotal(IReadOnlyCollection<OrderItem> items)
    {
        var first = items.First();
        var initial = new Money(first.Subtotal.Amount, first.Subtotal.Currency);
        return items.Skip(1).Aggregate(initial, (total, item) => total.Add(item.Subtotal));
    }

    private static string NormalizePriority(string? value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? "medium" : value.Trim().ToLowerInvariant();
        return normalized is "low" or "medium" or "high" ? normalized : "medium";
    }
}
