using King.Nexa.Platform.Invoicing.Domain.Model.Entities;
using King.Nexa.Platform.Invoicing.Domain.Repositories;
using King.Nexa.Platform.Shared.Application.Security;
using King.Nexa.Platform.Shared.Domain.Model.Entities;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Microsoft.EntityFrameworkCore;

namespace King.Nexa.Platform.Invoicing.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class BusinessDocumentRepository(AppDbContext context, ICurrentWorkspaceContext workspaceContext) : IBusinessDocumentRepository
{
    public async Task AddAsync(BusinessDocument document, CancellationToken cancellationToken = default) =>
        await context.BusinessDocuments.AddAsync(document, cancellationToken);

    public Task<BusinessDocument?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Scoped().FirstOrDefaultAsync(row => row.Id == id, cancellationToken);

    public Task<BusinessDocument?> FindByOrderAndTypeAsync(int orderId, string type, CancellationToken cancellationToken = default) =>
        Scoped().FirstOrDefaultAsync(row => row.OrderId == orderId && row.Type == type, cancellationToken);

    public async Task<IEnumerable<BusinessDocument>> ListAsync(CancellationToken cancellationToken = default) =>
        await Scoped().AsNoTracking().OrderByDescending(row => row.CreatedAt).ToListAsync(cancellationToken);

    private IQueryable<BusinessDocument> Scoped()
    {
        if (workspaceContext.TenantId is not { } tenantId) return context.BusinessDocuments.Where(_ => false);
        var query = context.BusinessDocuments.Where(row => row.TenantId == tenantId);
        return workspaceContext.ClientAccountId is { } clientAccountId
            ? query.Where(row => row.ClientAccountId == clientAccountId && row.VisibleToBuyer)
            : query;
    }
}

public class PaymentMethodRecordRepository(AppDbContext context, ICurrentWorkspaceContext workspaceContext) : IPaymentMethodRecordRepository
{
    public async Task AddAsync(PaymentMethodRecord record, CancellationToken cancellationToken = default) =>
        await context.PaymentMethodRecords.AddAsync(record, cancellationToken);

    public Task<PaymentMethodRecord?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Scoped().FirstOrDefaultAsync(row => row.Id == id, cancellationToken);

    public async Task<IEnumerable<PaymentMethodRecord>> ListAsync(CancellationToken cancellationToken = default) =>
        await Scoped().AsNoTracking()
            .OrderBy(row => row.ClientAccountId)
            .ThenByDescending(row => row.IsDefault)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<PaymentMethodRecord>> ListDefaultsAsync(int tenantId, int clientAccountId, int? excludedId, CancellationToken cancellationToken = default) =>
        await context.PaymentMethodRecords
            .Where(row => row.TenantId == tenantId && row.ClientAccountId == clientAccountId && row.IsDefault)
            .Where(row => !excludedId.HasValue || row.Id != excludedId.Value)
            .ToListAsync(cancellationToken);

    private IQueryable<PaymentMethodRecord> Scoped()
    {
        if (workspaceContext.TenantId is not { } tenantId) return context.PaymentMethodRecords.Where(_ => false);
        var query = context.PaymentMethodRecords.Where(row => row.TenantId == tenantId);
        return workspaceContext.ClientAccountId is { } clientAccountId
            ? query.Where(row => row.ClientAccountId == clientAccountId)
            : query;
    }
}

public class PaymentProcessRecordRepository(AppDbContext context, ICurrentWorkspaceContext workspaceContext) : IPaymentProcessRecordRepository
{
    public async Task AddAsync(PaymentProcessRecord record, CancellationToken cancellationToken = default) =>
        await context.PaymentProcessRecords.AddAsync(record, cancellationToken);

    public Task<PaymentProcessRecord?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Scoped().FirstOrDefaultAsync(row => row.Id == id, cancellationToken);

    public async Task<IEnumerable<PaymentProcessRecord>> ListAsync(CancellationToken cancellationToken = default) =>
        await Scoped().AsNoTracking().OrderByDescending(row => row.CreatedAt).ToListAsync(cancellationToken);

    private IQueryable<PaymentProcessRecord> Scoped()
    {
        if (workspaceContext.TenantId is not { } tenantId) return context.PaymentProcessRecords.Where(_ => false);
        var query = context.PaymentProcessRecords.Where(row => row.TenantId == tenantId);
        return workspaceContext.ClientAccountId is { } clientAccountId
            ? query.Where(row => row.ClientAccountId == clientAccountId)
            : query;
    }
}

public class NotificationRecordRepository(AppDbContext context, ICurrentWorkspaceContext workspaceContext) : INotificationRecordRepository
{
    public async Task AddAsync(NotificationRecord notification, CancellationToken cancellationToken = default) =>
        await context.NotificationRecords.AddAsync(notification, cancellationToken);

    public Task<NotificationRecord?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Scoped().FirstOrDefaultAsync(row => row.Id == id, cancellationToken);

    public async Task<IEnumerable<NotificationRecord>> ListAsync(CancellationToken cancellationToken = default) =>
        await Scoped().AsNoTracking()
            .OrderBy(row => row.Read)
            .ThenByDescending(row => row.CreatedAt)
            .ToListAsync(cancellationToken);

    public void Remove(NotificationRecord notification) => context.NotificationRecords.Remove(notification);

    private IQueryable<NotificationRecord> Scoped()
    {
        if (workspaceContext.TenantId is not { } tenantId) return context.NotificationRecords.Where(_ => false);
        var query = context.NotificationRecords.Where(row => row.TenantId == tenantId);
        return workspaceContext.ClientAccountId is { } clientAccountId
            ? query.Where(row => row.ClientAccountId == clientAccountId)
            : query;
    }
}

public class InvoicingTenantReferenceRepository(AppDbContext context) : IInvoicingTenantReferenceRepository
{
    public Task<DocumentType?> FindActiveDocumentTypeAsync(int? id, string key, CancellationToken cancellationToken = default) =>
        context.DocumentTypes.AsNoTracking()
            .FirstOrDefaultAsync(row =>
                row.IsActive &&
                (id.HasValue ? row.Id == id.Value : row.Key == key),
                cancellationToken);

    public Task<bool> OrderBelongsToTenantAsync(int tenantId, int orderId, CancellationToken cancellationToken = default) =>
        context.Orders.AsNoTracking().AnyAsync(row => row.TenantId == tenantId && row.Id == orderId, cancellationToken);

    public Task<bool> ClientAccountBelongsToTenantAsync(int tenantId, int clientAccountId, CancellationToken cancellationToken = default) =>
        context.ClientAccounts.AsNoTracking().AnyAsync(row => row.TenantId == tenantId && row.Id == clientAccountId, cancellationToken);

    public Task<bool> PaymentBelongsToTenantAsync(int tenantId, int paymentId, CancellationToken cancellationToken = default) =>
        context.Payments.AsNoTracking().AnyAsync(row => row.TenantId == tenantId && row.Id == paymentId, cancellationToken);

    public Task<bool> PaymentOptionIsActiveAsync(int paymentOptionId, CancellationToken cancellationToken = default) =>
        context.PaymentOptions.AsNoTracking().AnyAsync(row => row.Id == paymentOptionId && row.IsActive, cancellationToken);

    public Task<bool> PaymentMethodBelongsToTenantAsync(int tenantId, int paymentMethodRecordId, CancellationToken cancellationToken = default) =>
        context.PaymentMethodRecords.AsNoTracking().AnyAsync(row => row.TenantId == tenantId && row.Id == paymentMethodRecordId, cancellationToken);
}
