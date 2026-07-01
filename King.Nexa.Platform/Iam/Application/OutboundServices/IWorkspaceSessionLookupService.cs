using King.Nexa.Platform.TenantManagement.Domain.Model.Aggregates;
using King.Nexa.Platform.TenantManagement.Domain.Model.Entities;

namespace King.Nexa.Platform.Iam.Application.OutboundServices;

public interface IWorkspaceSessionLookupService
{
    Task<WorkspaceSessionLookupResult?> FindActiveSessionAsync(int userId, string workspaceSlug, CancellationToken cancellationToken = default);
}

public record WorkspaceSessionLookupResult(Tenant Tenant, Workspace Workspace, UserWorkspaceMembership Membership);
