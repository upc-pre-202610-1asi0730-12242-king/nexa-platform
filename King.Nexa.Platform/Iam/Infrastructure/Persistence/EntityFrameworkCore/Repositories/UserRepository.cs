using King.Nexa.Platform.Iam.Domain.Model.Aggregates;
using King.Nexa.Platform.Iam.Domain.Repositories;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace King.Nexa.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class UserRepository(AppDbContext context) : BaseRepository<User>(context), IUserRepository
{
    public async Task<User?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
        await Context.Users.FirstOrDefaultAsync(user => user.Username == username.Trim(), cancellationToken);

    public async Task<IReadOnlyCollection<User>> ListByTenantAsync(int tenantId, CancellationToken cancellationToken = default) =>
        await Context.UserWorkspaceMemberships.AsNoTracking()
            .Where(membership => membership.TenantId == tenantId)
            .Join(Context.Users.AsNoTracking(), membership => membership.UserId, user => user.Id, (_, user) => user)
            .Distinct()
            .OrderBy(user => user.Id)
            .ToListAsync(cancellationToken);
}
