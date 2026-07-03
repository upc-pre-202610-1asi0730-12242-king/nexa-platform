using King.Nexa.Platform.Invoicing.Application.CommandServices;
using King.Nexa.Platform.Invoicing.Application.QueryServices;
using King.Nexa.Platform.Invoicing.Domain.Model.Aggregates;
using King.Nexa.Platform.Invoicing.Domain.Model.Commands;
using King.Nexa.Platform.Invoicing.Domain.Repositories;
using King.Nexa.Platform.Shared.Application.Auditing;
using King.Nexa.Platform.Shared.Application.Security;
using King.Nexa.Platform.Shared.Domain.Repositories;

namespace King.Nexa.Platform.Invoicing.Application.Internal.CommandServices;

public class InvoiceCommandService(
    IInvoiceRepository invoiceRepository,
    IUnitOfWork unitOfWork,
    ICurrentWorkspaceContext workspaceContext,
    IAuditLogger auditLogger,
    ILogger<InvoiceCommandService> logger) : IInvoiceCommandService
{
    public async Task<Invoice> GenerateAsync(GenerateInvoiceCommand command, CancellationToken cancellationToken = default)
    {
        var invoice = new Invoice(command);
        invoice.AssignTenant(workspaceContext.TenantId
            ?? throw new InvalidOperationException("Current tenant is required to create invoices."));
        await invoiceRepository.AddAsync(invoice, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        logger.LogInformation("Invoice {InvoiceId} created for tenant {TenantId}.", invoice.Id, invoice.TenantId);
        await auditLogger.RecordAsync(new AuditLogEntry("invoice.created", "invoice", invoice.Id.ToString(), TenantId: invoice.TenantId), cancellationToken);
        return invoice;
    }

    public async Task<Invoice?> UpdateAsync(UpdateInvoiceCommand command, CancellationToken cancellationToken = default)
    {
        var invoice = await invoiceRepository.FindByIdAsync(command.InvoiceId, cancellationToken);
        if (invoice is null) return null;

        invoice.Update(command);
        invoiceRepository.Update(invoice);
        await unitOfWork.CompleteAsync(cancellationToken);
        logger.LogInformation("Invoice {InvoiceId} updated for tenant {TenantId}.", invoice.Id, invoice.TenantId);
        return invoice;
    }

    public async Task<Invoice?> MarkPaidAsync(MarkInvoicePaidCommand command, CancellationToken cancellationToken = default)
    {
        var invoice = await invoiceRepository.FindByIdAsync(command.InvoiceId, cancellationToken);
        if (invoice is null) return null;

        invoice.MarkPaid();
        invoiceRepository.Update(invoice);
        await unitOfWork.CompleteAsync(cancellationToken);
        logger.LogInformation("Invoice {InvoiceId} marked paid for tenant {TenantId}.", invoice.Id, invoice.TenantId);
        await auditLogger.RecordAsync(new AuditLogEntry("invoice.status_changed", "invoice", invoice.Id.ToString(), "{\"status\":\"paid\"}", invoice.TenantId), cancellationToken);
        return invoice;
    }

    public async Task<bool> CancelAsync(CancelInvoiceCommand command, CancellationToken cancellationToken = default)
    {
        var invoice = await invoiceRepository.FindByIdAsync(command.InvoiceId, cancellationToken);
        if (invoice is null) return false;

        invoice.Cancel();
        invoiceRepository.Update(invoice);
        await unitOfWork.CompleteAsync(cancellationToken);
        logger.LogInformation("Invoice {InvoiceId} cancelled for tenant {TenantId}.", invoice.Id, invoice.TenantId);
        await auditLogger.RecordAsync(new AuditLogEntry("invoice.status_changed", "invoice", invoice.Id.ToString(), "{\"status\":\"cancelled\"}", invoice.TenantId), cancellationToken);
        return true;
    }
}
