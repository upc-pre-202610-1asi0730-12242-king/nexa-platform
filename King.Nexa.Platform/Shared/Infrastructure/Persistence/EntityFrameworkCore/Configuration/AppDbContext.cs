using King.Nexa.Platform.CatalogManagement.Domain.Model.Aggregates;
using King.Nexa.Platform.CatalogManagement.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using King.Nexa.Platform.Iam.Domain.Model.Aggregates;
using King.Nexa.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using King.Nexa.Platform.Invoicing.Domain.Model.Aggregates;
using King.Nexa.Platform.Invoicing.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using King.Nexa.Platform.Logistics.Domain.Model.Aggregates;
using King.Nexa.Platform.Logistics.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using King.Nexa.Platform.Sales.Domain.Model.Aggregates;
using King.Nexa.Platform.Sales.Domain.Model.Entities;
using King.Nexa.Platform.Sales.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using King.Nexa.Platform.Warehouse.Domain.Model.Aggregates;
using King.Nexa.Platform.Warehouse.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using Microsoft.EntityFrameworkCore;
using WarehouseAggregate = King.Nexa.Platform.Warehouse.Domain.Model.Aggregates.Warehouse;

namespace King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<CatalogItem> CatalogItems => Set<CatalogItem>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Brand> Brands => Set<Brand>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();

    public DbSet<WarehouseAggregate> Warehouses => Set<WarehouseAggregate>();

    public DbSet<Shipment> Shipments => Set<Shipment>();

    public DbSet<Invoice> Invoices => Set<Invoice>();

    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyCatalogManagementConfiguration();
        builder.ApplySalesConfiguration();
        builder.ApplyWarehouseConfiguration();
        builder.ApplyLogisticsConfiguration();
        builder.ApplyInvoicingConfiguration();
        builder.ApplyIamConfiguration();

        builder.UseSnakeCasePluralNamingConvention();
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
