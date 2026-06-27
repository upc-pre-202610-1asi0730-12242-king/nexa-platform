using King.Nexa.Platform.Sales.Domain.Model.Entities;
using King.Nexa.Platform.Sales.Domain.Repositories;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Microsoft.EntityFrameworkCore;

namespace King.Nexa.Platform.Sales.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class CreditRequestRepository(AppDbContext context) : ICreditRequestRepository
{
    public async Task<IReadOnlyCollection<CreditRequest>> ListAsync(
        int tenantId,
        int? clientAccountId = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.CreditRequests.AsNoTracking().Where(row => row.TenantId == tenantId);
        if (clientAccountId.HasValue) query = query.Where(row => row.ClientAccountId == clientAccountId.Value);
        return await query.OrderByDescending(row => row.CreatedAt).ToListAsync(cancellationToken);
    }

    public Task<CreditRequest?> FindAsync(
        int tenantId,
        int id,
        int? clientAccountId = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.CreditRequests.Where(row => row.TenantId == tenantId && row.Id == id);
        if (clientAccountId.HasValue) query = query.Where(row => row.ClientAccountId == clientAccountId.Value);
        return query.FirstOrDefaultAsync(cancellationToken);
    }

    public Task<bool> ClientBelongsToTenantAsync(int tenantId, int clientAccountId, CancellationToken cancellationToken = default) =>
        context.ClientAccounts.AsNoTracking().AnyAsync(row => row.TenantId == tenantId && row.Id == clientAccountId, cancellationToken);

    public Task AddAsync(CreditRequest entity, CancellationToken cancellationToken = default) =>
        context.CreditRequests.AddAsync(entity, cancellationToken).AsTask();

    public void Remove(CreditRequest entity) => context.CreditRequests.Remove(entity);
}

