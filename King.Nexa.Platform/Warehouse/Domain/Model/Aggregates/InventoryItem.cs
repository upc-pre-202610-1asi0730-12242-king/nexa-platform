using King.Nexa.Platform.Shared.Domain.Model.Entities;
using King.Nexa.Platform.Warehouse.Domain.Model.Commands;
using King.Nexa.Platform.Warehouse.Domain.Model.ValueObjects;

namespace King.Nexa.Platform.Warehouse.Domain.Model.Aggregates;

/// <summary>
/// Aggregate root for stock availability and reservation state.
/// </summary>
public class InventoryItem : AuditableEntity
{
    protected InventoryItem()
    {
        ProductId = null!;
        CatalogItemId = null!;
        AvailableQuantity = null!;
        ReservedQuantity = null!;
        WarehouseLocation = null!;
        TemperatureRange = null!;
    }

    public InventoryItem(CreateInventoryItemCommand command)
    {
        ProductId = command.ProductId;
        CatalogItemId = command.CatalogItemId;
        AvailableQuantity = command.AvailableQuantity;
        ReservedQuantity = new StockQuantity(0);
        WarehouseLocation = command.WarehouseLocation;
        TemperatureRange = command.TemperatureRange;
    }

    public ProductId ProductId { get; private set; }

    public CatalogItemId CatalogItemId { get; private set; }

    public StockQuantity AvailableQuantity { get; private set; }

    public StockQuantity ReservedQuantity { get; private set; }

    public WarehouseLocation WarehouseLocation { get; private set; }

    public TemperatureRange TemperatureRange { get; private set; }

    public void Update(UpdateInventoryItemCommand command)
    {
        ProductId = command.ProductId;
        CatalogItemId = command.CatalogItemId;
        AvailableQuantity = command.AvailableQuantity;
        WarehouseLocation = command.WarehouseLocation;
        TemperatureRange = command.TemperatureRange;
    }

    public void Reserve(InventoryReservation reservation)
    {
        if (reservation.Units > AvailableQuantity.Value)
            throw new InvalidOperationException("Requested units exceed available stock.");

        AvailableQuantity = new StockQuantity(AvailableQuantity.Value - reservation.Units);
        ReservedQuantity = new StockQuantity(ReservedQuantity.Value + reservation.Units);
    }

    public void Release(InventoryReservation reservation)
    {
        if (reservation.Units > ReservedQuantity.Value)
            throw new InvalidOperationException("Released units exceed reserved stock.");

        AvailableQuantity = new StockQuantity(AvailableQuantity.Value + reservation.Units);
        ReservedQuantity = new StockQuantity(ReservedQuantity.Value - reservation.Units);
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
