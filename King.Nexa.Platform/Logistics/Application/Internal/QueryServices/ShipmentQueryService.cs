using King.Nexa.Platform.Logistics.Application.CommandServices;
using King.Nexa.Platform.Logistics.Application.QueryServices;
using King.Nexa.Platform.Logistics.Domain.Model.Aggregates;
using King.Nexa.Platform.Logistics.Domain.Model.Queries;
using King.Nexa.Platform.Logistics.Domain.Repositories;

namespace King.Nexa.Platform.Logistics.Application.Internal.QueryServices;

public class ShipmentQueryService(IShipmentRepository shipmentRepository) : IShipmentQueryService
{
    public async Task<IEnumerable<Shipment>> Handle(GetAllShipmentsQuery query, CancellationToken cancellationToken = default) =>
        await shipmentRepository.ListAsync(cancellationToken);

    public async Task<Shipment?> Handle(GetShipmentByIdQuery query, CancellationToken cancellationToken = default) =>
        await shipmentRepository.FindByIdAsync(query.ShipmentId, cancellationToken);

    public async Task<IEnumerable<Shipment>> Handle(GetShipmentsByOrderIdQuery query, CancellationToken cancellationToken = default) =>
        await shipmentRepository.ListByOrderIdAsync(query.OrderId, cancellationToken);

    public async Task<IEnumerable<Shipment>> Handle(GetShipmentsByStatusQuery query, CancellationToken cancellationToken = default) =>
        await shipmentRepository.ListByStatusAsync(query.Status, cancellationToken);
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
