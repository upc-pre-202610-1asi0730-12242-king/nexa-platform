using King.Nexa.Platform.Invoicing.Domain.Model.Aggregates;
using King.Nexa.Platform.Invoicing.Domain.Model.ValueObjects;
using King.Nexa.Platform.Invoicing.Domain.Repositories;
using King.Nexa.Platform.Shared.Application.Security;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace King.Nexa.Platform.Invoicing.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class PaymentRepository(AppDbContext context, ICurrentWorkspaceContext workspaceContext) : BaseRepository<Payment>(context), IPaymentRepository
{
    public override async Task<Payment?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await Scoped().FirstOrDefaultAsync(payment => payment.Id == id, cancellationToken);

    public override async Task<IEnumerable<Payment>> ListAsync(CancellationToken cancellationToken = default) =>
        await Scoped().OrderByDescending(payment => payment.CreatedAt).ToListAsync(cancellationToken);

    public async Task<IEnumerable<Payment>> ListByInvoiceIdAsync(int invoiceId, CancellationToken cancellationToken = default) =>
        await Scoped().Where(payment => payment.InvoiceId == invoiceId).ToListAsync(cancellationToken);

    public async Task<IEnumerable<Payment>> ListByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default) =>
        await Scoped().Where(payment => payment.Status == status).ToListAsync(cancellationToken);

    private IQueryable<Payment> Scoped()
    {
        if (workspaceContext.TenantId is not { } tenantId)
            return Context.Payments.Where(_ => false);
        var query = Context.Payments.Where(payment => payment.TenantId == tenantId);
        return workspaceContext.ClientAccountId is { } clientAccountId
            ? query.Where(payment => payment.ClientAccountId == clientAccountId)
            : query;
    }
}
