using King.Nexa.Platform.Shared.Domain.Model.Entities;

namespace King.Nexa.Platform.Logistics.Domain.Model.Entities;

public class DispatchOrder : AuditableEntity
{
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
            case "in_route":
                StartRoute();
                break;
            case "delivered":
                Complete();
                break;
            case "incident":
                Incident();
                break;
            case "reprogrammed":
                SetStatus("reprogrammed");
                break;
            default:
                SetStatus(status);
                break;
        }
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
        Status = status.Trim().ToLowerInvariant();
        UpdatedAt = DateTime.UtcNow;
    }
}

public class DispatchEvent : AuditableEntity
{
    public int TenantId { get; set; }
    public int DispatchOrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool VisibleToBuyer { get; set; } = true;
}

public class ProofOfDeliveryRecord : AuditableEntity
{
    public int TenantId { get; set; }
    public int DispatchOrderId { get; set; }
    public string ReceivedBy { get; set; } = string.Empty;
    public DateTime? CompletedAt { get; set; }
    public bool PhotoReference { get; set; }
    public bool SignatureReference { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string Status { get; set; } = "pending";
}

public class TemperatureLog : AuditableEntity
{
    public int TenantId { get; set; }
    public int? DispatchOrderId { get; set; }
    public int? OrderId { get; set; }
    public decimal Celsius { get; set; }
    public string Zone { get; set; } = string.Empty;
    public string Status { get; set; } = "ok";
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}

// Retained only to keep the historical database model migration-compatible.
public class CustomerPortalTask : AuditableEntity
{
    public int TenantId { get; set; }
    public int ClientAccountId { get; set; }
    public string PortalName { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string UploadChannel { get; set; } = "manual";
    public string RequiredDocuments { get; set; } = string.Empty;
    public string Status { get; set; } = "pending";
    public string Owner { get; set; } = string.Empty;
}
