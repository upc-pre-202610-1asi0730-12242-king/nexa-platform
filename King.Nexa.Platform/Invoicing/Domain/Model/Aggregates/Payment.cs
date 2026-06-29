using King.Nexa.Platform.Invoicing.Domain.Model.Commands;
using King.Nexa.Platform.Invoicing.Domain.Model.ValueObjects;
using King.Nexa.Platform.Shared.Domain.Model.Entities;

namespace King.Nexa.Platform.Invoicing.Domain.Model.Aggregates;

public class Payment : AuditableEntity
{
    protected Payment()
    {
        BillingAmount = null!;
        ReferenceCode = string.Empty;
    }

    public Payment(RegisterPaymentCommand command)
    {
        InvoiceId = command.InvoiceId;
        BillingAmount = command.BillingAmount;
        ReferenceCode = command.ReferenceCode.Trim().ToUpperInvariant();
        Status = PaymentStatus.Pending;
    }

    public int InvoiceId { get; private set; }

    public BillingAmount BillingAmount { get; private set; }

    public string ReferenceCode { get; private set; }

    public PaymentStatus Status { get; private set; }

    public void Update(UpdatePaymentCommand command)
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException("Only pending payments can be updated.");

        InvoiceId = command.InvoiceId;
        BillingAmount = command.BillingAmount;
        ReferenceCode = command.ReferenceCode.Trim().ToUpperInvariant();
    }

    public void Cancel()
    {
        if (Status == PaymentStatus.Paid) throw new InvalidOperationException("Paid payments cannot be cancelled.");

        Status = PaymentStatus.Cancelled;
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
