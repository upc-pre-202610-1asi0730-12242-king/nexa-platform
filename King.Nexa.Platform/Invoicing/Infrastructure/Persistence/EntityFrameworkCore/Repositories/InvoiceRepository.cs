using King.Nexa.Platform.Invoicing.Domain.Model.Aggregates;
using King.Nexa.Platform.Invoicing.Domain.Model.ValueObjects;
using King.Nexa.Platform.Invoicing.Domain.Repositories;
using King.Nexa.Platform.Shared.Application.Security;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace King.Nexa.Platform.Invoicing.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class InvoiceRepository(AppDbContext context, ICurrentWorkspaceContext workspaceContext) : BaseRepository<Invoice>(context), IInvoiceRepository
{
    public override async Task<Invoice?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await Scoped().FirstOrDefaultAsync(invoice => invoice.Id == id, cancellationToken);

    public override async Task<IEnumerable<Invoice>> ListAsync(CancellationToken cancellationToken = default) =>
        await Scoped().ToListAsync(cancellationToken);

    public async Task<Invoice?> FindByInvoiceNumberAsync(InvoiceNumber invoiceNumber, CancellationToken cancellationToken = default) =>
        await Scoped().FirstOrDefaultAsync(invoice => invoice.InvoiceNumber == invoiceNumber, cancellationToken);

    public async Task<IEnumerable<Invoice>> ListByOrderIdAsync(int orderId, CancellationToken cancellationToken = default) =>
        await Scoped().Where(invoice => invoice.OrderId == orderId).ToListAsync(cancellationToken);

    public async Task<IEnumerable<Invoice>> ListByPaymentStatusAsync(PaymentStatus paymentStatus, CancellationToken cancellationToken = default) =>
        await Scoped().Where(invoice => invoice.PaymentStatus == paymentStatus).ToListAsync(cancellationToken);

    private IQueryable<Invoice> Scoped()
    {
        if (workspaceContext.TenantId is not { } tenantId)
            return Context.Invoices.Where(_ => false);
        var query = Context.Invoices.Where(invoice => invoice.TenantId == tenantId);
        if (workspaceContext.ClientAccountId is not { } clientAccountId) return query;

        return query.Where(invoice => Context.Orders.Any(order =>
            order.Id == invoice.OrderId && order.ClientAccountId == clientAccountId));
    }
}
