using King.Nexa.Platform.Invoicing.Domain.Model.Commands;
using King.Nexa.Platform.Invoicing.Domain.Model.ValueObjects;
using King.Nexa.Platform.Shared.Domain.Model.Entities;
using King.Nexa.Platform.Shared.Domain.Model.Events;

namespace King.Nexa.Platform.Invoicing.Domain.Model.Aggregates;

public class Payment : AuditableEntity, ITenantScoped
{
    protected Payment()
    {
        BillingAmount = null!;
        ReferenceCode = string.Empty;
    }

    public Payment(RegisterPaymentCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.ReferenceCode))
            throw new ArgumentException("Payment reference code is required.", nameof(command));

        InvoiceId = command.InvoiceId;
        OrderId = command.OrderId;
        ClientAccountId = command.ClientAccountId;
        PaymentOptionId = command.PaymentOptionId;
        PaymentMethodRecordId = command.PaymentMethodRecordId;
        BillingAmount = command.BillingAmount;
        ReferenceCode = command.ReferenceCode.Trim().ToUpperInvariant();
        Status = PaymentStatus.Pending;
    }

    public int TenantId { get; private set; }

    public int? InvoiceId { get; private set; }

    public int? OrderId { get; private set; }

    public int? ClientAccountId { get; private set; }

    public int? PaymentOptionId { get; private set; }

    public int? PaymentMethodRecordId { get; private set; }

    public BillingAmount BillingAmount { get; private set; }

    public string ReferenceCode { get; private set; }

    public PaymentStatus Status { get; private set; }

    public DateTimeOffset? ConfirmedAt { get; private set; }

    public DateTimeOffset? RejectedAt { get; private set; }

    public void AssignTenant(int tenantId)
    {
        if (tenantId <= 0) throw new ArgumentException("Tenant id must be positive.", nameof(tenantId));
        TenantId = tenantId;
    }

    public void Update(UpdatePaymentCommand command)
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException("Only pending payments can be updated.");
        if (string.IsNullOrWhiteSpace(command.ReferenceCode))
            throw new ArgumentException("Payment reference code is required.", nameof(command));

        InvoiceId = command.InvoiceId;
        OrderId = command.OrderId;
        ClientAccountId = command.ClientAccountId;
        PaymentOptionId = command.PaymentOptionId;
        PaymentMethodRecordId = command.PaymentMethodRecordId;
        BillingAmount = command.BillingAmount;
        ReferenceCode = command.ReferenceCode.Trim().ToUpperInvariant();
    }

    public void Confirm()
    {
        if (Status == PaymentStatus.Confirmed) throw new InvalidOperationException("Payment is already confirmed.");
        if (Status is PaymentStatus.Cancelled or PaymentStatus.Rejected or PaymentStatus.Failed)
            throw new InvalidOperationException("Only pending payments can be confirmed.");

        Status = PaymentStatus.Confirmed;
        ConfirmedAt = DateTimeOffset.UtcNow;
        RejectedAt = null;
        AddDomainEvent(new PaymentCompleted(Id, TenantId));
    }

    public void Reject()
    {
        if (Status == PaymentStatus.Confirmed) throw new InvalidOperationException("Confirmed payments cannot be rejected.");
        if (Status == PaymentStatus.Cancelled) throw new InvalidOperationException("Cancelled payments cannot be rejected.");

        Status = PaymentStatus.Rejected;
        RejectedAt = DateTimeOffset.UtcNow;
    }

    public void MarkFailed()
    {
        if (Status == PaymentStatus.Confirmed) throw new InvalidOperationException("Confirmed payments cannot fail.");
        if (Status == PaymentStatus.Cancelled) throw new InvalidOperationException("Cancelled payments cannot fail.");

        Status = PaymentStatus.Failed;
        RejectedAt = DateTimeOffset.UtcNow;
    }

    public void Cancel()
    {
        if (Status is PaymentStatus.Confirmed or PaymentStatus.Paid) throw new InvalidOperationException("Confirmed payments cannot be cancelled.");

        Status = PaymentStatus.Cancelled;
    }
}

