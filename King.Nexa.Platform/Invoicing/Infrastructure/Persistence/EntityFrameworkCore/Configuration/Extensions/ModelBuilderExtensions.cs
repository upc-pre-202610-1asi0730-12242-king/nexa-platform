using King.Nexa.Platform.Invoicing.Domain.Model.Aggregates;
using King.Nexa.Platform.Invoicing.Domain.Model.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace King.Nexa.Platform.Invoicing.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyInvoicingConfiguration(this ModelBuilder builder)
    {
        var invoice = builder.Entity<Invoice>();

        invoice.ToTable("invoices");
        invoice.HasKey(entity => entity.Id);
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
        invoice.HasIndex(entity => entity.InvoiceNumber).IsUnique();

        var payment = builder.Entity<Payment>();
        payment.ToTable("payments");
        payment.HasKey(entity => entity.Id);
        payment.Property(entity => entity.InvoiceId)
            .HasColumnName("invoice_id")
            .IsRequired();
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
        payment.HasIndex(entity => entity.ReferenceCode).IsUnique();
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
