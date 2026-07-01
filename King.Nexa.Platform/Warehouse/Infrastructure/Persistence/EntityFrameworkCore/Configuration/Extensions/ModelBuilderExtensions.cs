using King.Nexa.Platform.Warehouse.Domain.Model.Aggregates;
using King.Nexa.Platform.Warehouse.Domain.Model.ValueObjects;
using King.Nexa.Platform.Warehouse.Domain.Model.Entities;
using King.Nexa.Platform.Sales.Domain.Model.Aggregates;
using King.Nexa.Platform.Sales.Domain.Model.Entities;
using King.Nexa.Platform.TenantManagement.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;
using WarehouseAggregate = King.Nexa.Platform.Warehouse.Domain.Model.Aggregates.Warehouse;

namespace King.Nexa.Platform.Warehouse.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyWarehouseConfiguration(this ModelBuilder builder)
    {
        var inventoryItem = builder.Entity<InventoryItem>();

        inventoryItem.ToTable("inventory_items");
        inventoryItem.HasKey(item => item.Id);
        inventoryItem.HasAlternateKey(item => new { item.TenantId, item.Id });
        inventoryItem.Property(item => item.TenantId).HasColumnName("tenant_id").IsRequired();
        inventoryItem.Property(item => item.ProductId)
            .HasConversion(value => value.Value, value => new ProductId(value))
            .HasColumnName("product_id")
            .HasMaxLength(64)
            .IsRequired();
        inventoryItem.Property(item => item.CatalogItemId)
            .HasConversion(value => value.Value, value => new CatalogItemId(value))
            .HasColumnName("catalog_item_id")
            .HasMaxLength(64)
            .IsRequired();
        inventoryItem.Property(item => item.AvailableQuantity)
            .HasConversion(value => value.Value, value => new StockQuantity(value))
            .HasColumnName("available_quantity")
            .IsRequired();
        inventoryItem.Property(item => item.ReservedQuantity)
            .HasConversion(value => value.Value, value => new StockQuantity(value))
            .HasColumnName("reserved_quantity")
            .IsRequired();
        inventoryItem.Property(item => item.WarehouseLocation)
            .HasConversion(value => value.Value, value => new WarehouseLocation(value))
            .HasColumnName("warehouse_location")
            .HasMaxLength(120)
            .IsRequired();
        inventoryItem.OwnsOne(item => item.TemperatureRange, temperature =>
        {
            temperature.Property(value => value.MinimumTemperature).HasColumnName("minimum_temperature").HasPrecision(6, 2);
            temperature.Property(value => value.MaximumTemperature).HasColumnName("maximum_temperature").HasPrecision(6, 2);
        });
        inventoryItem.HasIndex(item => new { item.TenantId, item.CatalogItemId }).IsUnique();
        inventoryItem.HasIndex(item => new { item.TenantId, item.ProductId });
        inventoryItem.HasOne<Tenant>().WithMany().HasForeignKey(item => item.TenantId).OnDelete(DeleteBehavior.Cascade);

        var warehouse = builder.Entity<WarehouseAggregate>();
        warehouse.ToTable("warehouses");
        warehouse.HasKey(entity => entity.Id);
        warehouse.HasAlternateKey(entity => new { entity.TenantId, entity.Id });
        warehouse.Property(entity => entity.TenantId).HasColumnName("tenant_id").IsRequired();
        warehouse.Property(entity => entity.Name)
            .HasConversion(value => value.Value, value => new WarehouseName(value))
            .HasColumnName("name")
            .HasMaxLength(120)
            .IsRequired();
        warehouse.Property(entity => entity.Location)
            .HasConversion(value => value.Value, value => new WarehouseLocation(value))
            .HasColumnName("location")
            .HasMaxLength(120)
            .IsRequired();
        warehouse.OwnsOne(entity => entity.TemperatureRange, temperature =>
        {
            temperature.Property(value => value.MinimumTemperature).HasColumnName("minimum_temperature").HasPrecision(6, 2);
            temperature.Property(value => value.MaximumTemperature).HasColumnName("maximum_temperature").HasPrecision(6, 2);
        });
        warehouse.Property(entity => entity.IsActive).HasColumnName("is_active").IsRequired();
        warehouse.HasIndex(entity => new { entity.TenantId, entity.Location }).IsUnique();
        warehouse.HasOne<Tenant>().WithMany().HasForeignKey(entity => entity.TenantId).OnDelete(DeleteBehavior.Cascade);

        var lot = builder.Entity<InventoryLot>();
        lot.ToTable("inventory_lots");
        lot.HasKey(entity => entity.Id);
        lot.HasAlternateKey(entity => new { entity.TenantId, entity.Id });
        lot.Property(entity => entity.LotCode).HasMaxLength(80).IsRequired();
        lot.Property(entity => entity.Zone).HasMaxLength(120);
        lot.Property(entity => entity.Status).HasMaxLength(40).IsRequired();
        lot.Property(entity => entity.MinimumTemperature).HasPrecision(6, 2);
        lot.Property(entity => entity.MaximumTemperature).HasPrecision(6, 2);
        lot.HasIndex(entity => new { entity.TenantId, entity.LotCode }).IsUnique();
        lot.HasIndex(entity => new { entity.TenantId, entity.ExpirationDate });
        lot.HasOne<Tenant>().WithMany().HasForeignKey(entity => entity.TenantId).OnDelete(DeleteBehavior.Cascade);
        lot.HasOne<InventoryItem>()
            .WithMany()
            .HasForeignKey(entity => new { entity.TenantId, entity.InventoryItemId })
            .HasPrincipalKey(entity => new { entity.TenantId, entity.Id })
            .OnDelete(DeleteBehavior.Restrict);
        lot.HasOne<WarehouseAggregate>()
            .WithMany()
            .HasForeignKey(entity => new { entity.TenantId, entity.WarehouseId })
            .HasPrincipalKey(entity => new { entity.TenantId, entity.Id })
            .OnDelete(DeleteBehavior.Restrict);

        var movement = builder.Entity<InventoryMovement>();
        movement.ToTable("inventory_movements");
        movement.HasKey(entity => entity.Id);
        movement.Property(entity => entity.Code).HasMaxLength(80).IsRequired();
        movement.Property(entity => entity.MovementType).HasMaxLength(40).IsRequired();
        movement.Property(entity => entity.Reason).HasMaxLength(500).IsRequired();
        movement.Property(entity => entity.PerformedBy).HasMaxLength(160);
        movement.Property(entity => entity.TemperatureReading).HasPrecision(6, 2);
        movement.HasIndex(entity => new { entity.TenantId, entity.Code }).IsUnique();
        movement.HasIndex(entity => new { entity.TenantId, entity.OccurredAt });
        movement.HasOne<Tenant>().WithMany().HasForeignKey(entity => entity.TenantId).OnDelete(DeleteBehavior.Cascade);
        movement.HasOne<InventoryItem>()
            .WithMany()
            .HasForeignKey(entity => new { entity.TenantId, entity.InventoryItemId })
            .HasPrincipalKey(entity => new { entity.TenantId, entity.Id })
            .OnDelete(DeleteBehavior.Restrict);
        movement.HasOne<InventoryLot>()
            .WithMany()
            .HasForeignKey(entity => new { entity.TenantId, entity.InventoryLotId })
            .HasPrincipalKey(entity => new { entity.TenantId, entity.Id })
            .OnDelete(DeleteBehavior.SetNull);
        movement.HasOne<WarehouseAggregate>()
            .WithMany()
            .HasForeignKey(entity => new { entity.TenantId, entity.WarehouseId })
            .HasPrincipalKey(entity => new { entity.TenantId, entity.Id })
            .OnDelete(DeleteBehavior.SetNull);
        movement.HasOne<Order>()
            .WithMany()
            .HasForeignKey(entity => new { entity.TenantId, entity.OrderId })
            .HasPrincipalKey(entity => new { entity.TenantId, entity.Id })
            .OnDelete(DeleteBehavior.SetNull);

        var reservation = builder.Entity<InventoryReservationRecord>();
        reservation.ToTable("inventory_reservations");
        reservation.HasKey(entity => entity.Id);
        reservation.Property(entity => entity.Code).HasMaxLength(80).IsRequired();
        reservation.Property(entity => entity.Status).HasMaxLength(40).IsRequired();
        reservation.HasIndex(entity => new { entity.TenantId, entity.Code }).IsUnique();
        reservation.HasIndex(entity => new { entity.TenantId, entity.Status });
        reservation.HasOne<Tenant>().WithMany().HasForeignKey(entity => entity.TenantId).OnDelete(DeleteBehavior.Cascade);
        reservation.HasOne<InventoryItem>()
            .WithMany()
            .HasForeignKey(entity => new { entity.TenantId, entity.InventoryItemId })
            .HasPrincipalKey(entity => new { entity.TenantId, entity.Id })
            .OnDelete(DeleteBehavior.Restrict);
        reservation.HasOne<InventoryLot>()
            .WithMany()
            .HasForeignKey(entity => new { entity.TenantId, entity.InventoryLotId })
            .HasPrincipalKey(entity => new { entity.TenantId, entity.Id })
            .OnDelete(DeleteBehavior.SetNull);
        reservation.HasOne<Order>()
            .WithMany()
            .HasForeignKey(entity => new { entity.TenantId, entity.OrderId })
            .HasPrincipalKey(entity => new { entity.TenantId, entity.Id })
            .OnDelete(DeleteBehavior.SetNull);
        reservation.HasOne<PurchaseRequest>()
            .WithMany()
            .HasForeignKey(entity => new { entity.TenantId, entity.PurchaseRequestId })
            .HasPrincipalKey(entity => new { entity.TenantId, entity.Id })
            .OnDelete(DeleteBehavior.SetNull);
    }
}
