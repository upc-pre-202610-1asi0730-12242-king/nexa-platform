using King.Nexa.Platform.Invoicing.Domain.Model.Aggregates;
using King.Nexa.Platform.Invoicing.Domain.Model.Entities;
using King.Nexa.Platform.Invoicing.Domain.Model.ValueObjects;
using King.Nexa.Platform.Sales.Domain.Model.Aggregates;
using King.Nexa.Platform.Shared.Domain.Model.Entities;
using King.Nexa.Platform.TenantManagement.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace King.Nexa.Platform.Invoicing.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyInvoicingConfiguration(this ModelBuilder builder)
    {
        var invoice = builder.Entity<Invoice>();

        invoice.ToTable("invoices");
        invoice.HasKey(entity => entity.Id);
        invoice.HasAlternateKey(entity => new { entity.TenantId, entity.Id });
        invoice.Property(entity => entity.TenantId).HasColumnName("tenant_id").IsRequired();
        invoice.Property(entity => entity.InvoiceNumber)
            .HasConversion(value => value.Value, value => new InvoiceNumber(value))
            .HasColumnName("invoice_number")
            .HasMaxLength(32)
            .IsRequired();
        invoice.OwnsOne(entity => entity.BillingAmount, money =>
        {
            money.Property(value => value.Amount).HasColumnName("amount").HasPrecision(12, 2).IsRequired();
            money.Property(value => value.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        });
        invoice.Property(entity => entity.PaymentStatus)
            .HasColumnName("payment_status")
            .HasConversion<string>()
            .HasMaxLength(24)
            .IsRequired();
        invoice.HasIndex(entity => new { entity.TenantId, entity.InvoiceNumber }).IsUnique();
        invoice.HasIndex(entity => new { entity.TenantId, entity.PaymentStatus });
        invoice.HasIndex(entity => new { entity.TenantId, entity.PaymentStatus, entity.CreatedAt });
        invoice.HasOne<Tenant>().WithMany().HasForeignKey(entity => entity.TenantId).OnDelete(DeleteBehavior.Cascade);

        var payment = builder.Entity<Payment>();
        payment.ToTable("payments");
        payment.HasKey(entity => entity.Id);
        payment.HasAlternateKey(entity => new { entity.TenantId, entity.Id });
        payment.Property(entity => entity.TenantId).HasColumnName("tenant_id").IsRequired();
        payment.Property(entity => entity.InvoiceId)
            .HasColumnName("invoice_id");
        payment.Property(entity => entity.OrderId).HasColumnName("order_id");
        payment.Property(entity => entity.ClientAccountId).HasColumnName("client_account_id");
        payment.Property(entity => entity.PaymentOptionId).HasColumnName("payment_option_id");
        payment.Property(entity => entity.PaymentMethodRecordId).HasColumnName("payment_method_record_id");
        payment.OwnsOne(entity => entity.BillingAmount, money =>
        {
            money.Property(value => value.Amount).HasColumnName("amount").HasPrecision(12, 2).IsRequired();
            money.Property(value => value.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        });
        payment.Property(entity => entity.ReferenceCode)
            .HasColumnName("reference_code")
            .HasMaxLength(80)
            .IsRequired();
        payment.Property(entity => entity.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(24)
            .IsRequired();
        payment.Property(entity => entity.ConfirmedAt).HasColumnName("confirmed_at");
        payment.Property(entity => entity.RejectedAt).HasColumnName("rejected_at");
        payment.HasIndex(entity => new { entity.TenantId, entity.ReferenceCode }).IsUnique();
        payment.HasIndex(entity => new { entity.TenantId, entity.Status });
        payment.HasIndex(entity => new { entity.TenantId, entity.CreatedAt });
        payment.HasIndex(entity => new { entity.TenantId, entity.Status, entity.CreatedAt });
        payment.HasOne<Tenant>().WithMany().HasForeignKey(entity => entity.TenantId).OnDelete(DeleteBehavior.Cascade);
        payment.HasOne<Invoice>()
            .WithMany()
            .HasForeignKey(entity => new { entity.TenantId, entity.InvoiceId })
            .HasPrincipalKey(entity => new { entity.TenantId, entity.Id })
            .OnDelete(DeleteBehavior.SetNull);
        payment.HasOne<Order>()
            .WithMany()
            .HasForeignKey(entity => new { entity.TenantId, entity.OrderId })
            .HasPrincipalKey(entity => new { entity.TenantId, entity.Id })
            .OnDelete(DeleteBehavior.SetNull);
        payment.HasOne<ClientAccount>()
            .WithMany()
            .HasForeignKey(entity => new { entity.TenantId, entity.ClientAccountId })
            .HasPrincipalKey(entity => new { entity.TenantId, entity.Id })
            .OnDelete(DeleteBehavior.SetNull);
        payment.HasOne<PaymentOption>().WithMany().HasForeignKey(entity => entity.PaymentOptionId).OnDelete(DeleteBehavior.Restrict);
        payment.HasOne<PaymentMethodRecord>()
            .WithMany()
            .HasForeignKey(entity => new { entity.TenantId, entity.PaymentMethodRecordId })
            .HasPrincipalKey(entity => new { entity.TenantId, entity.Id })
            .OnDelete(DeleteBehavior.SetNull);

        ConfigureBusinessDocument(builder);
        ConfigurePaymentMethodRecord(builder);
        ConfigurePaymentProcessRecord(builder);
        ConfigureNotificationRecord(builder);
    }

    private static void ConfigureBusinessDocument(ModelBuilder builder)
    {
        var entity = builder.Entity<BusinessDocument>();
        entity.ToTable("business_documents");
        entity.HasKey(row => row.Id);
        entity.Property(row => row.Type).HasMaxLength(60).IsRequired();
        entity.Property(row => row.Label).HasMaxLength(160).IsRequired();
        entity.Property(row => row.Status).HasMaxLength(40).IsRequired();
        entity.Property(row => row.FileName).HasMaxLength(220);
        entity.HasIndex(row => new { row.TenantId, row.ClientAccountId, row.Status });
        entity.HasOne<Tenant>().WithMany().HasForeignKey(row => row.TenantId).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne<Order>()
            .WithMany()
            .HasForeignKey(row => new { row.TenantId, row.OrderId })
            .HasPrincipalKey(row => new { row.TenantId, row.Id })
            .OnDelete(DeleteBehavior.SetNull);
        entity.HasOne<ClientAccount>()
            .WithMany()
            .HasForeignKey(row => new { row.TenantId, row.ClientAccountId })
            .HasPrincipalKey(row => new { row.TenantId, row.Id })
            .OnDelete(DeleteBehavior.SetNull);
        entity.HasOne<DocumentType>().WithMany().HasForeignKey(row => row.DocumentTypeId).OnDelete(DeleteBehavior.SetNull);
    }

    private static void ConfigurePaymentMethodRecord(ModelBuilder builder)
    {
        var entity = builder.Entity<PaymentMethodRecord>();
        entity.ToTable("payment_method_records");
        entity.HasKey(row => row.Id);
        entity.HasAlternateKey(row => new { row.TenantId, row.Id });
        entity.Property(row => row.Type).HasMaxLength(60).IsRequired();
        entity.Property(row => row.Label).HasMaxLength(160);
        entity.Property(row => row.Status).HasMaxLength(40).IsRequired();
        entity.HasIndex(row => new { row.TenantId, row.ClientAccountId });
        entity.HasOne<Tenant>().WithMany().HasForeignKey(row => row.TenantId).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne<ClientAccount>()
            .WithMany()
            .HasForeignKey(row => new { row.TenantId, row.ClientAccountId })
            .HasPrincipalKey(row => new { row.TenantId, row.Id })
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigurePaymentProcessRecord(ModelBuilder builder)
    {
        var entity = builder.Entity<PaymentProcessRecord>();
        entity.ToTable("payment_process_records");
        entity.HasKey(row => row.Id);
        entity.Property(row => row.Subtotal).HasPrecision(12, 2);
        entity.Property(row => row.Discount).HasPrecision(12, 2);
        entity.Property(row => row.Shipping).HasPrecision(12, 2);
        entity.Property(row => row.Igv).HasPrecision(12, 2);
        entity.Property(row => row.Total).HasPrecision(12, 2);
        entity.Property(row => row.Status).HasMaxLength(40).IsRequired();
        entity.HasIndex(row => new { row.TenantId, row.Status });
        entity.HasOne<Tenant>().WithMany().HasForeignKey(row => row.TenantId).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne<Order>()
            .WithMany()
            .HasForeignKey(row => new { row.TenantId, row.OrderId })
            .HasPrincipalKey(row => new { row.TenantId, row.Id })
            .OnDelete(DeleteBehavior.SetNull);
        entity.HasOne<ClientAccount>()
            .WithMany()
            .HasForeignKey(row => new { row.TenantId, row.ClientAccountId })
            .HasPrincipalKey(row => new { row.TenantId, row.Id })
            .OnDelete(DeleteBehavior.SetNull);
        entity.HasOne<Payment>()
            .WithMany()
            .HasForeignKey(row => new { row.TenantId, row.PaymentId })
            .HasPrincipalKey(row => new { row.TenantId, row.Id })
            .OnDelete(DeleteBehavior.SetNull);
        entity.HasOne<PaymentMethodRecord>()
            .WithMany()
            .HasForeignKey(row => new { row.TenantId, row.PaymentMethodRecordId })
            .HasPrincipalKey(row => new { row.TenantId, row.Id })
            .OnDelete(DeleteBehavior.SetNull);
    }

    private static void ConfigureNotificationRecord(ModelBuilder builder)
    {
        var entity = builder.Entity<NotificationRecord>();
        entity.ToTable("notification_records");
        entity.HasKey(row => row.Id);
        entity.Property(row => row.RecipientRole).HasMaxLength(60).IsRequired();
        entity.Property(row => row.Type).HasMaxLength(60).IsRequired();
        entity.Property(row => row.Title).HasMaxLength(180).IsRequired();
        entity.Property(row => row.Body).HasMaxLength(600);
        entity.HasIndex(row => new { row.TenantId, row.ClientAccountId, row.Read });
        entity.HasOne<Tenant>().WithMany().HasForeignKey(row => row.TenantId).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne<ClientAccount>()
            .WithMany()
            .HasForeignKey(row => new { row.TenantId, row.ClientAccountId })
            .HasPrincipalKey(row => new { row.TenantId, row.Id })
            .OnDelete(DeleteBehavior.SetNull);
    }
}
