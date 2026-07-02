using King.Nexa.Platform.Shared.Domain.Model.Entities;
using King.Nexa.Platform.Shared.Domain.Model.Events;

namespace King.Nexa.Platform.Logistics.Domain.Model.Entities;

public class DispatchOrder : AuditableEntity, ITenantScoped
{
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "ready_for_operations",
        "preparing",
        "assigned",
        "scheduled",
        "ready_for_route",
        "in_route",
        "delivered",
        "incident",
        "reprogrammed",
        "cancelled"
    };

    public int TenantId { get; set; }
    public int OrderId { get; set; }
    public int ClientAccountId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Status { get; set; } = "ready_for_operations";
    public string RouteName { get; set; } = string.Empty;
    public string Responsible { get; set; } = string.Empty;
    public DateTime? Eta { get; set; }
    public string DeliveryWindow { get; set; } = string.Empty;

    public void Assign(string responsible)
    {
        if (string.IsNullOrWhiteSpace(responsible)) throw new ArgumentException("Responsible user is required.", nameof(responsible));
        EnsureNotTerminal();
        if (Status is "in_route") throw new InvalidOperationException("Dispatch already started.");
        Responsible = responsible.Trim();
        SetStatus("assigned");
    }

    public void Schedule(DateTime eta, string deliveryWindow, string status)
    {
        if (string.IsNullOrWhiteSpace(deliveryWindow)) throw new ArgumentException("Delivery window is required.", nameof(deliveryWindow));
        if (Status is not ("ready_for_operations" or "assigned" or "scheduled" or "reprogrammed"))
            throw new InvalidOperationException("Dispatch can only be scheduled before route start.");
        Eta = eta;
        DeliveryWindow = deliveryWindow.Trim();
        SetStatus(status);
    }

    public void ChangeStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status)) throw new ArgumentException("Dispatch status is required.", nameof(status));
        switch (status.Trim().ToLowerInvariant())
        {
            case "preparing":
                StartPreparation();
                break;
            case "in_route":
                StartRoute();
                break;
            case "delivered":
                Complete();
                break;
            case "incident":
                Incident();
                break;
            default:
                SetStatus(status);
                break;
        }
    }

    public void StartPreparation()
    {
        if (Status != "ready_for_operations")
            throw new InvalidOperationException("Dispatch preparation can only start from the operations queue.");
        SetStatus("preparing");
    }

    public void StartRoute()
    {
        if (Status is not ("assigned" or "scheduled" or "ready_for_route"))
            throw new InvalidOperationException("Dispatch route can only start after assignment or schedule.");
        SetStatus("in_route");
    }

    public void Complete()
    {
        if (Status != "in_route")
            throw new InvalidOperationException("Dispatch can only be completed while in route.");
        SetStatus("delivered");
    }

    public void Incident()
    {
        EnsureNotTerminal();
        SetStatus("incident");
    }

    private void EnsureNotTerminal()
    {
        if (Status is "delivered" or "cancelled")
            throw new InvalidOperationException("Dispatch is already closed.");
    }

    private void SetStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status)) throw new ArgumentException("Dispatch status is required.", nameof(status));
        var normalized = status.Trim().ToLowerInvariant();
        if (!AllowedStatuses.Contains(normalized)) throw new InvalidOperationException("Dispatch status is not supported.");
        Status = normalized;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new DispatchStatusChanged(Id, TenantId, normalized));
    }
}
