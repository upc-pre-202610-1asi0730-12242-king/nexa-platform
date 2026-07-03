using King.Nexa.Platform.Invoicing.Application.CommandServices;
using King.Nexa.Platform.Invoicing.Domain.Model.Aggregates;
using King.Nexa.Platform.Invoicing.Domain.Model.Commands;
using King.Nexa.Platform.Invoicing.Domain.Repositories;
using King.Nexa.Platform.Shared.Application.Auditing;
using King.Nexa.Platform.Shared.Application.Security;
using King.Nexa.Platform.Shared.Domain.Repositories;

namespace King.Nexa.Platform.Invoicing.Application.Internal.CommandServices;

public class PaymentCommandService(
    IPaymentRepository paymentRepository,
    IInvoiceRepository invoiceRepository,
    IInvoicingTenantReferenceRepository referenceRepository,
    ICurrentWorkspaceContext workspaceContext,
    IAuditLogger auditLogger,
    IUnitOfWork unitOfWork,
    ILogger<PaymentCommandService> logger) : IPaymentCommandService
{
    public async Task<Payment> CreateAsync(RegisterPaymentCommand command, CancellationToken cancellationToken = default)
    {
        var tenantId = RequiredTenantId();
        await ValidateReferencesAsync(command.InvoiceId, command.PaymentOptionId, command.PaymentMethodRecordId, cancellationToken);
        var payment = new Payment(command);
        payment.AssignTenant(tenantId);
        await paymentRepository.AddAsync(payment, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        logger.LogInformation("Payment {PaymentId} created for tenant {TenantId}.", payment.Id, payment.TenantId);
        await auditLogger.RecordAsync(new AuditLogEntry("payment.created", "payment", payment.Id.ToString(), TenantId: payment.TenantId), cancellationToken);
        return payment;
    }

    public async Task<Payment?> UpdateAsync(UpdatePaymentCommand command, CancellationToken cancellationToken = default)
    {
        var payment = await paymentRepository.FindByIdAsync(command.PaymentId, cancellationToken);
        if (payment is null) return null;

        await ValidateReferencesAsync(command.InvoiceId, command.PaymentOptionId, command.PaymentMethodRecordId, cancellationToken);
        payment.Update(command);
        paymentRepository.Update(payment);
        await unitOfWork.CompleteAsync(cancellationToken);
        return payment;
    }

    public async Task<Payment?> ConfirmAsync(ConfirmPaymentCommand command, CancellationToken cancellationToken = default)
    {
        var payment = await paymentRepository.FindByIdAsync(command.PaymentId, cancellationToken);
        if (payment is null) return null;

        payment.Confirm();
        paymentRepository.Update(payment);
        await unitOfWork.CompleteAsync(cancellationToken);
        logger.LogInformation("Payment {PaymentId} confirmed for tenant {TenantId}.", payment.Id, payment.TenantId);
        await auditLogger.RecordAsync(new AuditLogEntry("payment.confirmed", "payment", payment.Id.ToString(), "{\"status\":\"confirmed\"}", payment.TenantId), cancellationToken);
        return payment;
    }

    public async Task<Payment?> RejectAsync(RejectPaymentCommand command, CancellationToken cancellationToken = default)
    {
        var payment = await paymentRepository.FindByIdAsync(command.PaymentId, cancellationToken);
        if (payment is null) return null;

        payment.Reject();
        paymentRepository.Update(payment);
        await unitOfWork.CompleteAsync(cancellationToken);
        logger.LogInformation("Payment {PaymentId} rejected for tenant {TenantId}.", payment.Id, payment.TenantId);
        await auditLogger.RecordAsync(new AuditLogEntry("payment.rejected", "payment", payment.Id.ToString(), "{\"status\":\"rejected\"}", payment.TenantId), cancellationToken);
        return payment;
    }

    public async Task<Payment?> CancelAsync(CancelPaymentCommand command, CancellationToken cancellationToken = default)
    {
        var payment = await paymentRepository.FindByIdAsync(command.PaymentId, cancellationToken);
        if (payment is null) return null;

        payment.Cancel();
        paymentRepository.Update(payment);
        await unitOfWork.CompleteAsync(cancellationToken);
        logger.LogInformation("Payment {PaymentId} cancelled for tenant {TenantId}.", payment.Id, payment.TenantId);
        await auditLogger.RecordAsync(new AuditLogEntry("payment.cancelled", "payment", payment.Id.ToString(), "{\"status\":\"cancelled\"}", payment.TenantId), cancellationToken);
        return payment;
    }

    public async Task<bool> DeleteAsync(DeletePaymentCommand command, CancellationToken cancellationToken = default)
    {
        var payment = await CancelAsync(new CancelPaymentCommand(command.PaymentId), cancellationToken);
        return payment is not null;
    }

    private int RequiredTenantId() =>
        workspaceContext.TenantId ?? throw new InvalidOperationException("Current tenant is required to create payments.");

    private async Task ValidateReferencesAsync(
        int? invoiceId,
        int? paymentOptionId,
        int? paymentMethodRecordId,
        CancellationToken cancellationToken)
    {
        var tenantId = RequiredTenantId();

        if (invoiceId.HasValue && await invoiceRepository.FindByIdAsync(invoiceId.Value, cancellationToken) is null)
            throw new InvalidOperationException("Invoice does not exist in current tenant.");

        if (paymentOptionId.HasValue)
        {
            var optionExists = await referenceRepository.PaymentOptionIsActiveAsync(paymentOptionId.Value, cancellationToken);
            if (!optionExists) throw new InvalidOperationException("Payment option is not active.");
        }

        if (paymentMethodRecordId.HasValue)
        {
            var methodExists = await referenceRepository.PaymentMethodBelongsToTenantAsync(tenantId, paymentMethodRecordId.Value, cancellationToken);
            if (!methodExists) throw new InvalidOperationException("Payment method does not exist in current tenant.");
        }
    }
}
