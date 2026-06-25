using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using King.Nexa.Platform.TenantManagement.Domain.Model.Entities;
using King.Nexa.Platform.TenantManagement.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace King.Nexa.Platform.TenantManagement.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class OrganizationRegistrationRequestRepository(AppDbContext context)
    : BaseRepository<OrganizationRegistrationRequest>(context), IOrganizationRegistrationRequestRepository
{
    public async Task<IEnumerable<OrganizationRegistrationRequest>> ListNewestAsync(CancellationToken cancellationToken = default) =>
        await Context.Set<OrganizationRegistrationRequest>()
            .AsNoTracking()
            .OrderByDescending(row => row.SubmittedAt)
            .ToListAsync(cancellationToken);

    public Task<OrganizationRegistrationRequest?> FindByExternalIdAsync(
        string externalId,
        CancellationToken cancellationToken = default) =>
        Context.Set<OrganizationRegistrationRequest>()
            .AsNoTracking()
            .FirstOrDefaultAsync(row => row.ExternalId == externalId, cancellationToken);

    public Task<bool> ExistsByExternalIdAsync(string externalId, CancellationToken cancellationToken = default) =>
        Context.Set<OrganizationRegistrationRequest>()
            .AnyAsync(row => row.ExternalId == externalId, cancellationToken);
}

