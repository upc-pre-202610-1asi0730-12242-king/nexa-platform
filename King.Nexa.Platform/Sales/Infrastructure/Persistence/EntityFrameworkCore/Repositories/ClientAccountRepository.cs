using King.Nexa.Platform.Sales.Domain.Model.Aggregates;
using King.Nexa.Platform.Sales.Domain.Repositories;
using King.Nexa.Platform.Shared.Application.Security;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace King.Nexa.Platform.Sales.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class ClientAccountRepository(AppDbContext context, ICurrentWorkspaceContext workspaceContext) : BaseRepository<ClientAccount>(context), IClientAccountRepository
{
    public override Task<ClientAccount?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Scoped().FirstOrDefaultAsync(client => client.Id == id, cancellationToken);

    public override async Task<IEnumerable<ClientAccount>> ListAsync(CancellationToken cancellationToken = default) =>
        await Scoped().OrderBy(client => client.BusinessName).ToListAsync(cancellationToken);

    public Task<ClientAccount?> FindByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        Scoped().FirstOrDefaultAsync(client => client.Code == code, cancellationToken);

    private IQueryable<ClientAccount> Scoped()
    {
        if (workspaceContext.TenantId is not { } tenantId)
            return Context.ClientAccounts.Where(_ => false);

        var query = Context.ClientAccounts.Where(client => client.TenantId == tenantId);
        return workspaceContext.ClientAccountId is { } clientAccountId
            ? query.Where(client => client.Id == clientAccountId)
            : query;
    }
}

