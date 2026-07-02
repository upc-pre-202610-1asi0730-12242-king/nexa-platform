using King.Nexa.Platform.Invoicing.Application.CommandServices;
using King.Nexa.Platform.Invoicing.Domain.Model.Entities;
using King.Nexa.Platform.Invoicing.Domain.Repositories;
using King.Nexa.Platform.Shared.Application.Auditing;
using King.Nexa.Platform.Shared.Application.Security;
using King.Nexa.Platform.Shared.Domain.Repositories;

namespace King.Nexa.Platform.Invoicing.Application.Internal.CommandServices;

public class PaymentProcessRecordCommandService(
    IPaymentProcessRecordRepository paymentProcessRepository,
    IInvoicingTenantReferenceRepository referenceRepository,
    IUnitOfWork unitOfWork,
    ICurrentWorkspaceContext workspaceContext,
    IAuditLogger auditLogger,
    ILogger<PaymentProcessRecordCommandService> logger) : IPaymentProcessRecordCommandService
{
    public async Task<PaymentProcessRecord> CreateAsync(PaymentProcessRecord record, CancellationToken cancellationToken = default)
    {
        var tenantId = workspaceContext.TenantId
            ?? throw new InvalidOperationException("Current tenant is required.");
        await ValidateRelationsAsync(record, tenantId, cancellationToken);
        record.TenantId = tenantId;
        record.Status = string.IsNullOrWhiteSpace(record.Status) ? "pending" : record.Status.Trim().ToLowerInvariant();
        if (record.Total < 0 || record.Subtotal < 0 || record.Discount < 0 || record.Shipping < 0 || record.Igv < 0)
            throw new InvalidOperationException("Payment process amounts cannot be negative.");

        await paymentProcessRepository.AddAsync(record, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        logger.LogInformation("Payment process {PaymentProcessId} created for tenant {TenantId}.", record.Id, tenantId);
        await auditLogger.RecordAsync(new AuditLogEntry("payment_process.created", "payment_process_record", record.Id.ToString(), TenantId: tenantId), cancellationToken);
        return record;
    }

    public async Task<PaymentProcessRecord?> ChangeStatusAsync(ChangePaymentProcessStatusCommand command, CancellationToken cancellationToken = default)
    {
        var payment = await paymentProcessRepository.FindByIdAsync(command.Id, cancellationToken);
        if (payment is null) return null;

        payment.ChangeStatus(command.Status);
        await unitOfWork.CompleteAsync(cancellationToken);
        logger.LogInformation("Payment process {PaymentProcessId} status changed to {Status} for tenant {TenantId}.", payment.Id, payment.Status, payment.TenantId);
        var action = payment.Status switch
        {
            "confirmed" => "payment_process.confirmed",
            "failed" => "payment_process.rejected",
            _ => "payment_process.status_changed"
        };
        await auditLogger.RecordAsync(
            new AuditLogEntry(action, "payment_process_record", payment.Id.ToString(), $"{{\"status\":\"{payment.Status}\"}}", payment.TenantId),
            cancellationToken);
        return payment;
    }

    private async Task ValidateRelationsAsync(PaymentProcessRecord record, int tenantId, CancellationToken cancellationToken)
    {
        if (record.OrderId.HasValue && !await referenceRepository.OrderBelongsToTenantAsync(tenantId, record.OrderId.Value, cancellationToken))
            throw new InvalidOperationException("Order does not belong to the current tenant.");

        if (record.ClientAccountId.HasValue && !await referenceRepository.ClientAccountBelongsToTenantAsync(tenantId, record.ClientAccountId.Value, cancellationToken))
            throw new InvalidOperationException("Client account does not belong to the current tenant.");

        if (record.PaymentId.HasValue && !await referenceRepository.PaymentBelongsToTenantAsync(tenantId, record.PaymentId.Value, cancellationToken))
            throw new InvalidOperationException("Payment does not belong to the current tenant.");

        if (record.PaymentMethodRecordId.HasValue && !await referenceRepository.PaymentMethodBelongsToTenantAsync(tenantId, record.PaymentMethodRecordId.Value, cancellationToken))
            throw new InvalidOperationException("Payment method does not belong to the current tenant.");
    }
}
