using King.Nexa.Platform.CatalogManagement.Domain.Model.Aggregates;
using King.Nexa.Platform.Sales.Domain.Model.Aggregates;
using King.Nexa.Platform.Sales.Domain.Model.Entities;
using King.Nexa.Platform.Sales.Domain.Model.ValueObjects;
using King.Nexa.Platform.TenantManagement.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace King.Nexa.Platform.Sales.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplySalesConfiguration(this ModelBuilder builder)
    {
        var order = builder.Entity<Order>();
        order.ToTable("orders");
        order.HasKey(entity => entity.Id);
        order.HasAlternateKey(entity => new { entity.TenantId, entity.Id });
        order.Property(entity => entity.OrderNumber)
            .HasConversion(value => value.Value, value => new OrderNumber(value))
            .HasColumnName("order_number")
            .HasMaxLength(32)
            .IsRequired();
        order.Property(entity => entity.TenantId).HasColumnName("tenant_id").IsRequired();
        order.Property(entity => entity.CustomerId)
            .HasConversion(value => value.Value, value => new CustomerId(value))
            .HasColumnName("customer_id")
            .HasMaxLength(64)
            .IsRequired();
        order.Property(entity => entity.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(24)
            .IsRequired();
        order.Property(entity => entity.Priority)
            .HasColumnName("priority")
            .HasMaxLength(24)
            .IsRequired();
        order.Property(entity => entity.Notes)
            .HasColumnName("notes")
            .HasMaxLength(1200)
            .IsRequired();
        order.OwnsOne(entity => entity.Delivery, delivery =>
        {
            delivery.Property(value => value.AddressType).HasColumnName("delivery_address_type").HasMaxLength(24).IsRequired();
            delivery.Property(value => value.Address).HasColumnName("delivery_address").HasMaxLength(240).IsRequired();
            delivery.Property(value => value.District).HasColumnName("delivery_district").HasMaxLength(120).IsRequired();
            delivery.Property(value => value.City).HasColumnName("delivery_city").HasMaxLength(120).IsRequired();
            delivery.Property(value => value.Province).HasColumnName("delivery_province").HasMaxLength(120).IsRequired();
            delivery.Property(value => value.Reference).HasColumnName("delivery_reference").HasMaxLength(240).IsRequired();
            delivery.Property(value => value.RequestedDate).HasColumnName("requested_delivery_date");
            delivery.Property(value => value.DispatchNote).HasColumnName("dispatch_note").HasMaxLength(600).IsRequired();
            delivery.Ignore(value => value.FullAddress);
        });
        order.OwnsOne(entity => entity.Total, money =>
        {
            money.Property(value => value.Amount).HasColumnName("total_amount").HasPrecision(12, 2).IsRequired();
            money.Property(value => value.Currency).HasColumnName("total_currency").HasMaxLength(3).IsRequired();
        });
        order.Property(entity => entity.PaymentConfirmation)
            .HasConversion(value => value == null ? null : value.Value, value => value == null ? null : new PaymentConfirmation(value))
            .HasColumnName("payment_confirmation")
            .HasMaxLength(120);
        order.Property(entity => entity.InventoryReservation)
            .HasConversion(value => value == null ? null : value.Value, value => value == null ? null : new InventoryReservation(value))
            .HasColumnName("inventory_reservation")
            .HasMaxLength(120);
        order.Property(entity => entity.RejectionReason)
            .HasConversion(value => value == null ? null : value.Value, value => value == null ? null : new RejectionReason(value))
            .HasColumnName("rejection_reason")
            .HasMaxLength(240);
        order.HasIndex(entity => new { entity.TenantId, entity.OrderNumber }).IsUnique();
        order.HasIndex(entity => new { entity.TenantId, entity.ClientAccountId });
        order.HasIndex(entity => new { entity.TenantId, entity.Status, entity.CreatedAt });
        order.HasOne<Tenant>().WithMany().HasForeignKey(entity => entity.TenantId).OnDelete(DeleteBehavior.Cascade);
        order.HasOne<ClientAccount>()
            .WithMany()
            .HasForeignKey(entity => new { entity.TenantId, entity.ClientAccountId })
            .HasPrincipalKey(entity => new { entity.TenantId, entity.Id })
            .OnDelete(DeleteBehavior.SetNull);
        order.HasMany(entity => entity.Items)
            .WithOne()
            .HasForeignKey(item => new { item.TenantId, item.OrderId })
            .HasPrincipalKey(entity => new { entity.TenantId, entity.Id })
            .OnDelete(DeleteBehavior.Cascade);

        var orderItem = builder.Entity<OrderItem>();
        orderItem.ToTable("order_items");
        orderItem.HasKey(entity => entity.Id);
        orderItem.Property(entity => entity.TenantId).HasColumnName("tenant_id").IsRequired();
        orderItem.Property(entity => entity.ProductId)
            .HasConversion(value => value.Value, value => new ProductId(value))
            .HasColumnName("product_id")
            .HasMaxLength(64)
            .IsRequired();
        orderItem.Property(entity => entity.CatalogItemId)
            .HasConversion(value => value.Value, value => new CatalogItemId(value))
            .HasColumnName("catalog_item_id")
            .HasMaxLength(64)
            .IsRequired();
        orderItem.Property(entity => entity.ItemName)
            .HasConversion(value => value.Value, value => new ItemName(value))
            .HasColumnName("item_name")
            .HasMaxLength(160)
            .IsRequired();
        orderItem.Property(entity => entity.Quantity)
            .HasConversion(value => value.Value, value => new Quantity(value))
            .HasColumnName("quantity")
            .IsRequired();
        orderItem.OwnsOne(entity => entity.UnitPrice, money =>
        {
            money.Property(value => value.Amount).HasColumnName("unit_price_amount").HasPrecision(12, 2).IsRequired();
            money.Property(value => value.Currency).HasColumnName("unit_price_currency").HasMaxLength(3).IsRequired();
        });
        orderItem.OwnsOne(entity => entity.Subtotal, money =>
        {
            money.Property(value => value.Amount).HasColumnName("subtotal_amount").HasPrecision(12, 2).IsRequired();
            money.Property(value => value.Currency).HasColumnName("subtotal_currency").HasMaxLength(3).IsRequired();
        });
        orderItem.HasIndex(entity => new { entity.TenantId, entity.OrderId });
        orderItem.HasOne<Tenant>().WithMany().HasForeignKey(entity => entity.TenantId).OnDelete(DeleteBehavior.Cascade);

        var client = builder.Entity<ClientAccount>();
        client.ToTable("client_accounts");
        client.HasKey(entity => entity.Id);
        client.HasAlternateKey(entity => new { entity.TenantId, entity.Id });
        client.Property(entity => entity.TenantId).HasColumnName("tenant_id").IsRequired();
        client.Property(entity => entity.Code).HasColumnName("code").HasMaxLength(32).IsRequired();
        client.Property(entity => entity.BusinessName).HasColumnName("business_name").HasMaxLength(180).IsRequired();
        client.Property(entity => entity.CommercialName).HasColumnName("commercial_name").HasMaxLength(160).IsRequired();
        client.Property(entity => entity.Ruc).HasColumnName("ruc").HasMaxLength(16);
        client.Property(entity => entity.Segment).HasColumnName("segment").HasMaxLength(80);
        client.Property(entity => entity.Contact).HasColumnName("contact").HasMaxLength(120);
        client.Property(entity => entity.ContactEmail).HasColumnName("contact_email").HasMaxLength(160);
        client.Property(entity => entity.Phone).HasColumnName("phone").HasMaxLength(32);
        client.Property(entity => entity.Address).HasColumnName("address").HasMaxLength(240);
        client.Property(entity => entity.District).HasColumnName("district").HasMaxLength(120);
        client.Property(entity => entity.Province).HasColumnName("province").HasMaxLength(120);
        client.Property(entity => entity.DeliveryReference).HasColumnName("delivery_reference").HasMaxLength(240);
        client.Property(entity => entity.DocumentProfile).HasColumnName("document_profile").HasMaxLength(80).IsRequired();
        client.Property(entity => entity.PaymentCondition).HasColumnName("payment_condition").HasMaxLength(40).IsRequired();
        client.Property(entity => entity.MonthlyCreditLimit).HasColumnName("monthly_credit_limit").HasPrecision(12, 2).IsRequired();
        client.Property(entity => entity.MonthlyCreditUsed).HasColumnName("monthly_credit_used").HasPrecision(12, 2).IsRequired();
        client.Property(entity => entity.MonthlyCreditStatus).HasColumnName("monthly_credit_status").HasMaxLength(40).IsRequired();
        client.Property(entity => entity.DeliveryPreference).HasColumnName("delivery_preference").HasMaxLength(160);
        client.Property(entity => entity.PortalAccess).HasColumnName("portal_access").IsRequired();
        client.Property(entity => entity.SellerWorkspaceEmail).HasColumnName("seller_workspace_email").HasMaxLength(160);
        client.Property(entity => entity.Status).HasColumnName("status").HasMaxLength(32).IsRequired();
        client.Ignore(entity => entity.MonthlyCreditAvailable);
        client.HasIndex(entity => new { entity.TenantId, entity.Code }).IsUnique();
        client.HasIndex(entity => new { entity.TenantId, entity.Ruc });
        client.HasOne<Tenant>().WithMany().HasForeignKey(entity => entity.TenantId).OnDelete(DeleteBehavior.Cascade);

        ConfigurePurchaseRequest(builder);
        ConfigurePurchaseRequestLine(builder);
        ConfigurePromotion(builder);
        ConfigureConversationMessage(builder);
        ConfigureCreditRequest(builder);
    }

    private static void ConfigurePurchaseRequest(ModelBuilder builder)
    {
        var entity = builder.Entity<PurchaseRequest>();
        entity.ToTable("purchase_requests");
        entity.HasKey(row => row.Id);
        entity.HasAlternateKey(row => new { row.TenantId, row.Id });
        entity.Property(row => row.Code).HasMaxLength(40).IsRequired();
        entity.Property(row => row.Origin).HasMaxLength(40).IsRequired();
        entity.Property(row => row.Status).HasMaxLength(40).IsRequired();
        entity.Property(row => row.Priority).HasMaxLength(32).IsRequired();
        entity.Property(row => row.DeliveryAddress).HasColumnName("delivery_address").HasMaxLength(240);
        entity.Property(row => row.DeliveryDistrict).HasColumnName("delivery_district").HasMaxLength(120);
        entity.Property(row => row.DeliveryCity).HasColumnName("delivery_city").HasMaxLength(120);
        entity.Property(row => row.DeliveryProvince).HasColumnName("delivery_province").HasMaxLength(120);
        entity.Property(row => row.DeliveryReference).HasColumnName("delivery_reference").HasMaxLength(240);
        entity.Property(row => row.PaymentOption).HasColumnName("payment_option").HasMaxLength(80);
        entity.Property(row => row.ShippingEstimate).HasColumnName("shipping_estimate").HasPrecision(12, 2);
        entity.Property(row => row.Comments).HasMaxLength(600);
        entity.Property(row => row.CommercialOwner).HasMaxLength(140);
        entity.HasIndex(row => new { row.TenantId, row.Code }).IsUnique();
        entity.HasIndex(row => new { row.TenantId, row.Status });
        entity.HasIndex(row => new { row.TenantId, row.Status, row.CreatedAt });
        entity.HasOne<Tenant>().WithMany().HasForeignKey(row => row.TenantId).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne<ClientAccount>()
            .WithMany()
            .HasForeignKey(row => new { row.TenantId, row.ClientAccountId })
            .HasPrincipalKey(row => new { row.TenantId, row.Id })
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigurePurchaseRequestLine(ModelBuilder builder)
    {
        var entity = builder.Entity<PurchaseRequestLine>();
        entity.ToTable("purchase_request_lines");
        entity.HasKey(row => row.Id);
        entity.Property(row => row.Quantity).HasPrecision(12, 2).IsRequired();
        entity.Property(row => row.Unit).HasMaxLength(32).IsRequired();
        entity.Property(row => row.EstimatedWeightKg).HasPrecision(12, 2);
        entity.Property(row => row.Notes).HasMaxLength(360);
        entity.HasIndex(row => row.PurchaseRequestId);
        entity.HasOne<Tenant>().WithMany().HasForeignKey(row => row.TenantId).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne<PurchaseRequest>()
            .WithMany()
            .HasForeignKey(row => new { row.TenantId, row.PurchaseRequestId })
            .HasPrincipalKey(row => new { row.TenantId, row.Id })
            .OnDelete(DeleteBehavior.Cascade);
        entity.HasOne<CatalogItem>()
            .WithMany()
            .HasForeignKey(row => new { row.TenantId, row.CatalogItemId })
            .HasPrincipalKey(row => new { row.TenantId, row.Id })
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigurePromotion(ModelBuilder builder)
    {
        var entity = builder.Entity<Promotion>();
        entity.ToTable("promotions");
        entity.HasKey(row => row.Id);
        entity.HasAlternateKey(row => new { row.TenantId, row.Id });
        entity.Property(row => row.Code).HasMaxLength(40).IsRequired();
        entity.Property(row => row.Name).HasMaxLength(160).IsRequired();
        entity.Property(row => row.Campaign).HasMaxLength(120);
        entity.Property(row => row.Description).HasMaxLength(800);
        entity.Property(row => row.DiscountLabel).HasMaxLength(160);
        entity.Property(row => row.Visibility).HasMaxLength(40).IsRequired();
        entity.Property(row => row.CommercialRule).HasMaxLength(300);
        entity.Property(row => row.AdjustmentType).HasMaxLength(60);
        entity.Property(row => row.TargetSegment).HasMaxLength(160);
        entity.Property(row => row.Notes).HasMaxLength(800);
        entity.Property(row => row.CatalogScope).HasMaxLength(200);
        entity.Property(row => row.Status).HasMaxLength(40).IsRequired();
        entity.HasIndex(row => new { row.TenantId, row.Code }).IsUnique();
        entity.HasIndex(row => new { row.TenantId, row.Status });
        entity.HasOne<Tenant>().WithMany().HasForeignKey(row => row.TenantId).OnDelete(DeleteBehavior.Cascade);

        var catalogItem = builder.Entity<PromotionCatalogItem>();
        catalogItem.ToTable("promotion_catalog_items");
        catalogItem.HasKey(row => row.Id);
        catalogItem.HasIndex(row => new { row.TenantId, row.PromotionId, row.CatalogItemId }).IsUnique();
        catalogItem.HasOne<Tenant>().WithMany().HasForeignKey(row => row.TenantId).OnDelete(DeleteBehavior.Cascade);
        catalogItem.HasOne<Promotion>()
            .WithMany()
            .HasForeignKey(row => new { row.TenantId, row.PromotionId })
            .HasPrincipalKey(row => new { row.TenantId, row.Id })
            .OnDelete(DeleteBehavior.Cascade);
        catalogItem.HasOne<CatalogItem>()
            .WithMany()
            .HasForeignKey(row => new { row.TenantId, row.CatalogItemId })
            .HasPrincipalKey(row => new { row.TenantId, row.Id })
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureConversationMessage(ModelBuilder builder)
    {
        var entity = builder.Entity<ConversationMessage>();
        entity.ToTable("conversation_messages");
        entity.HasKey(row => row.Id);
        entity.Property(row => row.SenderRole).HasMaxLength(60).IsRequired();
        entity.Property(row => row.SenderName).HasMaxLength(140).IsRequired();
        entity.Property(row => row.Body).HasMaxLength(1200).IsRequired();
        entity.HasIndex(row => new { row.TenantId, row.ClientAccountId });
        entity.HasOne<Tenant>().WithMany().HasForeignKey(row => row.TenantId).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne<ClientAccount>()
            .WithMany()
            .HasForeignKey(row => new { row.TenantId, row.ClientAccountId })
            .HasPrincipalKey(row => new { row.TenantId, row.Id })
            .OnDelete(DeleteBehavior.SetNull);
        entity.HasOne<PurchaseRequest>()
            .WithMany()
            .HasForeignKey(row => new { row.TenantId, row.PurchaseRequestId })
            .HasPrincipalKey(row => new { row.TenantId, row.Id })
            .OnDelete(DeleteBehavior.SetNull);
        entity.HasOne<Order>()
            .WithMany()
            .HasForeignKey(row => new { row.TenantId, row.OrderId })
            .HasPrincipalKey(row => new { row.TenantId, row.Id })
            .OnDelete(DeleteBehavior.SetNull);
    }

    private static void ConfigureCreditRequest(ModelBuilder builder)
    {
        var entity = builder.Entity<CreditRequest>();
        entity.ToTable("credit_requests");
        entity.HasKey(row => row.Id);
        entity.Property(row => row.Code).HasMaxLength(40).IsRequired();
        entity.Property(row => row.RequestedAmount).HasPrecision(12, 2).IsRequired();
        entity.Property(row => row.Reason).HasMaxLength(600).IsRequired();
        entity.Property(row => row.Status).HasMaxLength(32).IsRequired();
        entity.Property(row => row.ReviewedBy).HasMaxLength(140);
        entity.Property(row => row.ResolutionNote).HasMaxLength(600);
        entity.HasIndex(row => new { row.TenantId, row.Code }).IsUnique();
        entity.HasIndex(row => new { row.TenantId, row.Status });
        entity.HasOne<Tenant>().WithMany().HasForeignKey(row => row.TenantId).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne<ClientAccount>()
            .WithMany()
            .HasForeignKey(row => new { row.TenantId, row.ClientAccountId })
            .HasPrincipalKey(row => new { row.TenantId, row.Id })
            .OnDelete(DeleteBehavior.Restrict);
    }
}
