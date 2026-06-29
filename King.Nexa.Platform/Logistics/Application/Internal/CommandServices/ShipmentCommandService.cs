using King.Nexa.Platform.Logistics.Application.CommandServices;
using King.Nexa.Platform.Logistics.Application.QueryServices;
using King.Nexa.Platform.Logistics.Domain.Model.Aggregates;
using King.Nexa.Platform.Logistics.Domain.Model.Commands;
using King.Nexa.Platform.Logistics.Domain.Repositories;
using King.Nexa.Platform.Shared.Domain.Repositories;

namespace King.Nexa.Platform.Logistics.Application.Internal.CommandServices;

public class ShipmentCommandService(IShipmentRepository shipmentRepository, IUnitOfWork unitOfWork) : IShipmentCommandService
{
    public async Task<Shipment> ScheduleAsync(ScheduleShipmentCommand command, CancellationToken cancellationToken = default)
    {
        var shipment = new Shipment(command);
        await shipmentRepository.AddAsync(shipment, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return shipment;
    }

    public async Task<Shipment?> RescheduleAsync(RescheduleShipmentCommand command, CancellationToken cancellationToken = default)
    {
        var shipment = await shipmentRepository.FindByIdAsync(command.ShipmentId, cancellationToken);
        if (shipment is null) return null;

        shipment.Reschedule(command);
        shipmentRepository.Update(shipment);
        await unitOfWork.CompleteAsync(cancellationToken);
        return shipment;
    }

    public async Task<Shipment?> MarkDeliveredAsync(MarkShipmentDeliveredCommand command, CancellationToken cancellationToken = default)
    {
        var shipment = await shipmentRepository.FindByIdAsync(command.ShipmentId, cancellationToken);
        if (shipment is null) return null;

        shipment.MarkDelivered();
        shipmentRepository.Update(shipment);
        await unitOfWork.CompleteAsync(cancellationToken);
        return shipment;
    }

    public async Task<bool> CancelAsync(CancelShipmentCommand command, CancellationToken cancellationToken = default)
    {
        var shipment = await shipmentRepository.FindByIdAsync(command.ShipmentId, cancellationToken);
        if (shipment is null) return false;

        shipment.Cancel();
        shipmentRepository.Update(shipment);
        await unitOfWork.CompleteAsync(cancellationToken);
        return true;
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
