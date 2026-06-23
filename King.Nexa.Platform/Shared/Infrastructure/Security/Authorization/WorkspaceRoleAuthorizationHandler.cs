using System.Security.Claims;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace King.Nexa.Platform.Shared.Infrastructure.Security.Authorization;

public sealed class WorkspaceRoleRequirement(IEnumerable<string> allowedRoles) : IAuthorizationRequirement
{
    public HashSet<string> AllowedRoles { get; } = new(allowedRoles, StringComparer.OrdinalIgnoreCase);
}

public sealed class WorkspaceRoleAuthorizationHandler(AppDbContext dbContext) : AuthorizationHandler<WorkspaceRoleRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        WorkspaceRoleRequirement requirement)
    {
        var userId = TryInt(context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? context.User.FindFirstValue("sub"));
        var tenantId = TryInt(context.User.FindFirstValue("tenant_id"));
        var workspaceId = TryInt(context.User.FindFirstValue("workspace_id"));
        var membershipId = TryInt(context.User.FindFirstValue("membership_id"));

        if (userId is null || tenantId is null || workspaceId is null || membershipId is null)
            return;

        var role = await dbContext.UserWorkspaceMemberships
            .AsNoTracking()
            .Where(row =>
                row.Id == membershipId.Value &&
                row.UserId == userId.Value &&
                row.TenantId == tenantId.Value &&
                row.WorkspaceId == workspaceId.Value &&
                row.Status == "active")
            .Select(row => row.Role)
            .FirstOrDefaultAsync();

        if (!string.IsNullOrWhiteSpace(role) && requirement.AllowedRoles.Contains(role))
            context.Succeed(requirement);
    }

    private static int? TryInt(string? value) =>
        int.TryParse(value, out var parsed) && parsed > 0 ? parsed : null;
}

