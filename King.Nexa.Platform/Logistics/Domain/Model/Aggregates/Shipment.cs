using King.Nexa.Platform.Logistics.Domain.Model.Commands;
using King.Nexa.Platform.Logistics.Domain.Model.ValueObjects;
using King.Nexa.Platform.Shared.Domain.Model.Entities;

namespace King.Nexa.Platform.Logistics.Domain.Model.Aggregates;

public class Shipment : AuditableEntity
{
    protected Shipment()
    {
        ShipmentCode = null!;
    }

    public Shipment(ScheduleShipmentCommand command)
    {
        ShipmentCode = command.ShipmentCode;
        OrderId = command.OrderId;
        ScheduledAt = command.ScheduledAt;
        Status = DeliveryStatus.Scheduled;
    }

    public ShipmentCode ShipmentCode { get; private set; }

    public int OrderId { get; private set; }

    public DateTimeOffset ScheduledAt { get; private set; }

    public DateTimeOffset? DeliveredAt { get; private set; }

    public DeliveryStatus Status { get; private set; }

    public TemperatureRecord? LastTemperatureRecord { get; private set; }

    public void RegisterTemperature(decimal celsius) => LastTemperatureRecord = new TemperatureRecord(celsius);

    public void Reschedule(RescheduleShipmentCommand command)
    {
        if (Status == DeliveryStatus.Delivered) throw new InvalidOperationException("Delivered shipments cannot be rescheduled.");
        if (Status == DeliveryStatus.Cancelled) throw new InvalidOperationException("Cancelled shipments cannot be rescheduled.");

        ScheduledAt = command.ScheduledAt;
    }

    public void Cancel()
    {
        if (Status == DeliveryStatus.Delivered) throw new InvalidOperationException("Delivered shipments cannot be cancelled.");

        Status = DeliveryStatus.Cancelled;
    }

    public void MarkDelivered()
    {
        if (Status == DeliveryStatus.Cancelled) throw new InvalidOperationException("Cancelled shipments cannot be delivered.");

        Status = DeliveryStatus.Delivered;
        DeliveredAt = DateTimeOffset.UtcNow;
    }
}


// ===========================================================================
// TEMPORARY DEVELOPMENT DRAFT & WORK IN PROGRESS NOTES
// Nexa Architecture Alignment - Bounded Context Validation
// Sprint backlog verification and code quality checklist
// 
// TODO Checklist:
// - Review EF Core DbSet schema mapping constraints.
// - Harden JWT token handler lifetime policies.
// - Test workspace role authorization handler edge cases.
// - Implement outbox pattern for transactional event dispatching.
// - Clean up mock panels and initial-data JSON files.
// - Ensure Cold Chain temperature monitors are correctly mapped.
// - Validate payment process records state machine transitions.
// - Check for performance bottlenecks in database queries.
// - Review API Rest guidelines traceability matrix.
// - Verify tenant capability guards routing policies.
// 
// Draft Helper Snippet (Deprecated - To be removed before release):
// public static class DraftHelper {
//     public static bool CheckStatus(string status) {
//         if (string.IsNullOrEmpty(status)) return false;
//         return status.Equals('Active', System.StringComparison.OrdinalIgnoreCase);
//     }
//     public static void LogTrace(string msg) {
//         System.Console.WriteLine('[TRACE] ' + msg);
//     }
// }
// 
// NOTES:
// - This draft is subject to refactoring in the final iteration.
// - Ensure all diagnostic console writes are replaced with EF logger.
// ===========================================================================
