using King.Nexa.Platform.Logistics.Domain.Model.Aggregates;
using King.Nexa.Platform.Logistics.Domain.Model.Entities;
using King.Nexa.Platform.Sales.Domain.Model.Aggregates;
using King.Nexa.Platform.Logistics.Domain.Model.ValueObjects;
using King.Nexa.Platform.TenantManagement.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace King.Nexa.Platform.Logistics.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyLogisticsConfiguration(this ModelBuilder builder)
    {
        var shipment = builder.Entity<Shipment>();

        shipment.ToTable("shipments");
        shipment.HasKey(entity => entity.Id);
        shipment.Property(entity => entity.TenantId).HasColumnName("tenant_id").IsRequired();
        shipment.Property(entity => entity.ShipmentCode)
            .HasConversion(value => value.Value, value => new ShipmentCode(value))
            .HasColumnName("shipment_code")
            .HasMaxLength(32)
            .IsRequired();
        shipment.Property(entity => entity.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(24)
            .IsRequired();
        shipment.OwnsOne(entity => entity.LastTemperatureRecord, temperature =>
        {
            temperature.Property(value => value.Celsius).HasColumnName("last_temperature_celsius").HasPrecision(5, 2);
            temperature.Property(value => value.RecordedAt).HasColumnName("last_temperature_recorded_at");
        });
        shipment.HasIndex(entity => new { entity.TenantId, entity.ShipmentCode }).IsUnique();
        shipment.HasOne<Tenant>().WithMany().HasForeignKey(entity => entity.TenantId).OnDelete(DeleteBehavior.Cascade);

        ConfigureDispatchOrder(builder);
        ConfigureDispatchEvent(builder);
        ConfigureProofOfDelivery(builder);
        ConfigureTemperatureLog(builder);
        ConfigureCustomerPortalTask(builder);
    }

    private static void ConfigureDispatchOrder(ModelBuilder builder)
    {
        var entity = builder.Entity<DispatchOrder>();
        entity.ToTable("dispatch_orders");
        entity.HasKey(row => row.Id);
        entity.HasAlternateKey(row => new { row.TenantId, row.Id });
        entity.Property(row => row.Code).HasMaxLength(40).IsRequired();
        entity.Property(row => row.Status).HasMaxLength(40).IsRequired();
        entity.Property(row => row.RouteName).HasMaxLength(140);
        entity.Property(row => row.Responsible).HasMaxLength(140);
        entity.Property(row => row.DeliveryWindow).HasMaxLength(160);
        entity.HasIndex(row => new { row.TenantId, row.Code }).IsUnique();
        entity.HasIndex(row => new { row.TenantId, row.Status });
        entity.HasOne<Tenant>().WithMany().HasForeignKey(row => row.TenantId).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne<Order>()
            .WithMany()
            .HasForeignKey(row => new { row.TenantId, row.OrderId })
            .HasPrincipalKey(row => new { row.TenantId, row.Id })
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne<ClientAccount>()
            .WithMany()
            .HasForeignKey(row => new { row.TenantId, row.ClientAccountId })
            .HasPrincipalKey(row => new { row.TenantId, row.Id })
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureDispatchEvent(ModelBuilder builder)
    {
        var entity = builder.Entity<DispatchEvent>();
        entity.ToTable("dispatch_events");
        entity.HasKey(row => row.Id);
        entity.Property(row => row.Status).HasMaxLength(40).IsRequired();
        entity.Property(row => row.Description).HasMaxLength(500);
        entity.HasIndex(row => row.DispatchOrderId);
        entity.HasOne<Tenant>().WithMany().HasForeignKey(row => row.TenantId).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne<DispatchOrder>()
            .WithMany()
            .HasForeignKey(row => new { row.TenantId, row.DispatchOrderId })
            .HasPrincipalKey(row => new { row.TenantId, row.Id })
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureProofOfDelivery(ModelBuilder builder)
    {
        var entity = builder.Entity<ProofOfDeliveryRecord>();
        entity.ToTable("proof_of_delivery_records");
        entity.HasKey(row => row.Id);
        entity.Property(row => row.ReceivedBy).HasMaxLength(140);
        entity.Property(row => row.Notes).HasMaxLength(500);
        entity.Property(row => row.Status).HasMaxLength(40).IsRequired();
        entity.HasIndex(row => row.DispatchOrderId).IsUnique();
        entity.HasOne<Tenant>().WithMany().HasForeignKey(row => row.TenantId).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne<DispatchOrder>()
            .WithMany()
            .HasForeignKey(row => new { row.TenantId, row.DispatchOrderId })
            .HasPrincipalKey(row => new { row.TenantId, row.Id })
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureTemperatureLog(ModelBuilder builder)
    {
        var entity = builder.Entity<TemperatureLog>();
        entity.ToTable("temperature_logs");
        entity.HasKey(row => row.Id);
        entity.Property(row => row.Celsius).HasPrecision(5, 2).IsRequired();
        entity.Property(row => row.Zone).HasMaxLength(80);
        entity.Property(row => row.Status).HasMaxLength(40).IsRequired();
        entity.HasIndex(row => new { row.TenantId, row.Status });
        entity.HasOne<Tenant>().WithMany().HasForeignKey(row => row.TenantId).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne<DispatchOrder>()
            .WithMany()
            .HasForeignKey(row => new { row.TenantId, row.DispatchOrderId })
            .HasPrincipalKey(row => new { row.TenantId, row.Id })
            .OnDelete(DeleteBehavior.SetNull);
        entity.HasOne<Order>()
            .WithMany()
            .HasForeignKey(row => new { row.TenantId, row.OrderId })
            .HasPrincipalKey(row => new { row.TenantId, row.Id })
            .OnDelete(DeleteBehavior.SetNull);
    }

    private static void ConfigureCustomerPortalTask(ModelBuilder builder)
    {
        var entity = builder.Entity<CustomerPortalTask>();
        entity.ToTable("customer_portal_tasks");
        entity.HasKey(row => row.Id);
        entity.Property(row => row.PortalName).HasMaxLength(140).IsRequired();
        entity.Property(row => row.ContactPerson).HasMaxLength(140);
        entity.Property(row => row.UploadChannel).HasMaxLength(60).IsRequired();
        entity.Property(row => row.RequiredDocuments).HasMaxLength(360);
        entity.Property(row => row.Status).HasMaxLength(40).IsRequired();
        entity.Property(row => row.Owner).HasMaxLength(140);
        entity.HasIndex(row => new { row.TenantId, row.ClientAccountId, row.Status });
        entity.HasOne<Tenant>().WithMany().HasForeignKey(row => row.TenantId).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne<ClientAccount>()
            .WithMany()
            .HasForeignKey(row => new { row.TenantId, row.ClientAccountId })
            .HasPrincipalKey(row => new { row.TenantId, row.Id })
            .OnDelete(DeleteBehavior.Restrict);
    }

}
