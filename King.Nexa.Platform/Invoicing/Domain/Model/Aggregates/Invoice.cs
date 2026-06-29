using King.Nexa.Platform.Invoicing.Domain.Model.Commands;
using King.Nexa.Platform.Invoicing.Domain.Model.ValueObjects;
using King.Nexa.Platform.Shared.Domain.Model.Entities;

namespace King.Nexa.Platform.Invoicing.Domain.Model.Aggregates;

public class Invoice : AuditableEntity
{
    protected Invoice()
    {
        InvoiceNumber = null!;
        BillingAmount = null!;
    }

    public Invoice(GenerateInvoiceCommand command)
    {
        InvoiceNumber = command.InvoiceNumber;
        OrderId = command.OrderId;
        BillingAmount = command.BillingAmount;
        PaymentStatus = PaymentStatus.Pending;
    }

    public InvoiceNumber InvoiceNumber { get; private set; }

    public int OrderId { get; private set; }

    public BillingAmount BillingAmount { get; private set; }

    public PaymentStatus PaymentStatus { get; private set; }

    public DateTimeOffset? PaidAt { get; private set; }

    public void Update(UpdateInvoiceCommand command)
    {
        if (PaymentStatus == PaymentStatus.Paid) throw new InvalidOperationException("Paid invoices cannot be updated.");
        if (PaymentStatus == PaymentStatus.Cancelled) throw new InvalidOperationException("Cancelled invoices cannot be updated.");

        InvoiceNumber = command.InvoiceNumber;
        OrderId = command.OrderId;
        BillingAmount = command.BillingAmount;
    }

    public void Cancel()
    {
        if (PaymentStatus == PaymentStatus.Paid) throw new InvalidOperationException("Paid invoices cannot be cancelled.");

        PaymentStatus = PaymentStatus.Cancelled;
    }

    public void MarkPaid()
    {
        if (PaymentStatus == PaymentStatus.Cancelled) throw new InvalidOperationException("Cancelled invoices cannot be paid.");

        PaymentStatus = PaymentStatus.Paid;
        PaidAt = DateTimeOffset.UtcNow;
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
