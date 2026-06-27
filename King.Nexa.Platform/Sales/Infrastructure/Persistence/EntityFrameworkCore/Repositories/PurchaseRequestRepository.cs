using King.Nexa.Platform.Sales.Domain.Model.Entities;
using King.Nexa.Platform.Sales.Domain.Repositories;
using King.Nexa.Platform.Shared.Application.Security;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Microsoft.EntityFrameworkCore;

namespace King.Nexa.Platform.Sales.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class PurchaseRequestRepository(AppDbContext context, ICurrentWorkspaceContext workspaceContext) : IPurchaseRequestRepository
{
    public async Task AddAsync(PurchaseRequest request, CancellationToken cancellationToken = default) =>
        await context.PurchaseRequests.AddAsync(request, cancellationToken);

    public Task<PurchaseRequest?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        ScopedRequests().FirstOrDefaultAsync(row => row.Id == id, cancellationToken);

    public async Task<IEnumerable<PurchaseRequest>> ListAsync(CancellationToken cancellationToken = default) =>
        await ScopedRequests()
            .AsNoTracking()
            .OrderByDescending(row => row.UpdatedAt ?? row.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<PurchaseRequest>> ListCommercialInboxAsync(CancellationToken cancellationToken = default) =>
        await ScopedRequests()
            .AsNoTracking()
            .Where(row => row.Status == "submitted" || row.Status == "buyer_adjustment_requested" || row.Status == "commercially_validated")
            .OrderByDescending(row => row.UpdatedAt ?? row.CreatedAt)
            .ToListAsync(cancellationToken);

    public void Remove(PurchaseRequest request) => context.PurchaseRequests.Remove(request);

    public async Task AddLineAsync(PurchaseRequestLine line, CancellationToken cancellationToken = default) =>
        await context.PurchaseRequestLines.AddAsync(line, cancellationToken);

    public Task<PurchaseRequestLine?> FindLineByIdAsync(int id, CancellationToken cancellationToken = default) =>
        ScopedLines().FirstOrDefaultAsync(row => row.Id == id, cancellationToken);

    public async Task<IEnumerable<PurchaseRequestLine>> ListLinesAsync(CancellationToken cancellationToken = default) =>
        await ScopedLines()
            .AsNoTracking()
            .OrderBy(row => row.PurchaseRequestId)
            .ThenBy(row => row.Id)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<PurchaseRequestLine>> ListLinesByRequestIdAsync(int requestId, CancellationToken cancellationToken = default) =>
        await ScopedLines()
            .Where(row => row.PurchaseRequestId == requestId)
            .OrderBy(row => row.Id)
            .ToListAsync(cancellationToken);

    public void RemoveLine(PurchaseRequestLine line) => context.PurchaseRequestLines.Remove(line);

    public async Task AddMessageAsync(ConversationMessage message, CancellationToken cancellationToken = default) =>
        await context.ConversationMessages.AddAsync(message, cancellationToken);

    public Task<ConversationMessage?> FindMessageByIdAsync(int id, CancellationToken cancellationToken = default) =>
        ScopedMessages().FirstOrDefaultAsync(row => row.Id == id, cancellationToken);

    public async Task<IEnumerable<ConversationMessage>> ListMessagesAsync(CancellationToken cancellationToken = default) =>
        await ScopedMessages()
            .AsNoTracking()
            .OrderBy(row => row.CreatedAt)
            .ThenBy(row => row.Id)
            .ToListAsync(cancellationToken);

    public void RemoveMessage(ConversationMessage message) => context.ConversationMessages.Remove(message);

    public Task<bool> RequestBelongsToTenantAsync(int tenantId, int requestId, CancellationToken cancellationToken = default) =>
        ScopedRequests().AsNoTracking().AnyAsync(row => row.Id == requestId && row.TenantId == tenantId, cancellationToken);

    private IQueryable<PurchaseRequest> ScopedRequests()
    {
        var query = context.PurchaseRequests.AsQueryable();
        if (workspaceContext.TenantId is not { } tenantId) return query.Where(_ => false);
        query = query.Where(row => row.TenantId == tenantId);
        return workspaceContext.ClientAccountId is { } clientAccountId
            ? query.Where(row => row.ClientAccountId == clientAccountId)
            : query;
    }

    private IQueryable<PurchaseRequestLine> ScopedLines()
    {
        var query = context.PurchaseRequestLines.AsQueryable();
        if (workspaceContext.TenantId is not { } tenantId) return query.Where(_ => false);
        query = query.Where(row => row.TenantId == tenantId);
        return workspaceContext.ClientAccountId is { } clientAccountId
            ? query.Where(row => context.PurchaseRequests.Any(request =>
                request.Id == row.PurchaseRequestId && request.ClientAccountId == clientAccountId))
            : query;
    }

    private IQueryable<ConversationMessage> ScopedMessages()
    {
        var query = context.ConversationMessages.AsQueryable();
        if (workspaceContext.TenantId is not { } tenantId) return query.Where(_ => false);
        query = query.Where(row => row.TenantId == tenantId);
        return workspaceContext.ClientAccountId is { } clientAccountId
            ? query.Where(row => row.ClientAccountId == clientAccountId)
            : query;
    }
}
