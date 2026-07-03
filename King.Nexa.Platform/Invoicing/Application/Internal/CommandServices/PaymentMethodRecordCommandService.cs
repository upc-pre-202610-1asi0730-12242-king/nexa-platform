using King.Nexa.Platform.Invoicing.Application.CommandServices;
using King.Nexa.Platform.Invoicing.Domain.Model.Entities;
using King.Nexa.Platform.Invoicing.Domain.Repositories;
using King.Nexa.Platform.Shared.Application.Auditing;
using King.Nexa.Platform.Shared.Application.Security;
using King.Nexa.Platform.Shared.Domain.Repositories;

namespace King.Nexa.Platform.Invoicing.Application.Internal.CommandServices;

public class PaymentMethodRecordCommandService(
    IPaymentMethodRecordRepository paymentMethodRepository,
    IInvoicingTenantReferenceRepository referenceRepository,
    IUnitOfWork unitOfWork,
    ICurrentWorkspaceContext workspaceContext,
    IAuditLogger auditLogger,
    ILogger<PaymentMethodRecordCommandService> logger) : IPaymentMethodRecordCommandService
{
    private static readonly HashSet<string> SupportedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "active",
        "inactive",
        "disabled"
    };

    public async Task<PaymentMethodRecord> CreateAsync(PaymentMethodRecord record, CancellationToken cancellationToken = default)
    {
        var tenantId = workspaceContext.TenantId
            ?? throw new InvalidOperationException("Current tenant is required.");
        if (record.ClientAccountId <= 0) throw new InvalidOperationException("Client account is required.");
        if (string.IsNullOrWhiteSpace(record.Type)) throw new InvalidOperationException("Payment method type is required.");
        if (string.IsNullOrWhiteSpace(record.Label)) throw new InvalidOperationException("Payment method label is required.");
        if (workspaceContext.ClientAccountId is { } buyerClientAccountId && buyerClientAccountId != record.ClientAccountId)
            throw new InvalidOperationException("Buyer payment methods must belong to the current client account.");
        var clientExists = await referenceRepository.ClientAccountBelongsToTenantAsync(tenantId, record.ClientAccountId, cancellationToken);
        if (!clientExists) throw new InvalidOperationException("Client account does not belong to the current tenant.");

        record.TenantId = tenantId;
        record.Type = record.Type.Trim().ToLowerInvariant();
        record.Label = record.Label.Trim();
        record.Status = "active";
        if (record.IsDefault)
            await ClearExistingDefaultAsync(record.ClientAccountId, tenantId, null, cancellationToken);
        await paymentMethodRepository.AddAsync(record, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        logger.LogInformation("Payment method record {PaymentMethodRecordId} created for tenant {TenantId}.", record.Id, tenantId);
        await auditLogger.RecordAsync(new AuditLogEntry("payment_method_record.created", "payment_method_record", record.Id.ToString(), TenantId: tenantId), cancellationToken);
        return record;
    }

    public async Task<PaymentMethodRecord?> ChangeStatusAsync(int id, string status, bool? isDefault, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(status)) throw new InvalidOperationException("Payment method status is required.");
        var nextStatus = status.Trim().ToLowerInvariant();
        if (!SupportedStatuses.Contains(nextStatus)) throw new InvalidOperationException("Payment method status is not supported.");

        var record = await paymentMethodRepository.FindByIdAsync(id, cancellationToken);
        if (record is null) return null;

        record.Status = nextStatus;
        if (isDefault.HasValue)
        {
            if (isDefault.Value)
                await ClearExistingDefaultAsync(record.ClientAccountId, record.TenantId, record.Id, cancellationToken);
            record.IsDefault = isDefault.Value;
        }
        if (nextStatus != "active") record.IsDefault = false;
        record.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.CompleteAsync(cancellationToken);
        await auditLogger.RecordAsync(new AuditLogEntry("payment_method_record.status_changed", "payment_method_record", record.Id.ToString(), TenantId: record.TenantId), cancellationToken);
        return record;
    }

    private async Task ClearExistingDefaultAsync(int clientAccountId, int tenantId, int? excludedId, CancellationToken cancellationToken)
    {
        var defaults = await paymentMethodRepository.ListDefaultsAsync(tenantId, clientAccountId, excludedId, cancellationToken);
        foreach (var existing in defaults)
        {
            existing.IsDefault = false;
            existing.UpdatedAt = DateTime.UtcNow;
        }
    }
}
