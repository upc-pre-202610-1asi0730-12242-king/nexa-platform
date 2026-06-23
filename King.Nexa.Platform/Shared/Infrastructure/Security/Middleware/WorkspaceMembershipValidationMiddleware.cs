using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Microsoft.EntityFrameworkCore;

namespace King.Nexa.Platform.Shared.Infrastructure.Security.Middleware;

public class WorkspaceMembershipValidationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext httpContext, AppDbContext dbContext)
    {
        var user = httpContext.User;
        if (user.Identity?.IsAuthenticated != true)
        {
            await next(httpContext);
            return;
        }

        var tenantClaim = user.FindFirst("tenant_id")?.Value;
        var workspaceClaim = user.FindFirst("workspace_id")?.Value;
        var membershipClaim = user.FindFirst("membership_id")?.Value;
        var userClaim = user.FindFirst("sub")?.Value
            ?? user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(tenantClaim) || string.IsNullOrWhiteSpace(workspaceClaim) || string.IsNullOrWhiteSpace(membershipClaim))
        {
            await next(httpContext);
            return;
        }

        var headerTenant = httpContext.Request.Headers["X-Nexa-Tenant-Id"].ToString();
        var headerWorkspace = httpContext.Request.Headers["X-Nexa-Workspace"].ToString();
        if (!string.IsNullOrWhiteSpace(headerTenant) && headerTenant != tenantClaim)
        {
            httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            await httpContext.Response.WriteAsJsonAsync(new { message = "Tenant header does not match authenticated workspace." });
            return;
        }

        if (!int.TryParse(userClaim, out var userId) ||
            !int.TryParse(tenantClaim, out var tenantId) ||
            !int.TryParse(workspaceClaim, out var workspaceId) ||
            !int.TryParse(membershipClaim, out var membershipId))
        {
            httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            await httpContext.Response.WriteAsJsonAsync(new { message = "Invalid workspace claims." });
            return;
        }

        var membership = await dbContext.UserWorkspaceMemberships.AsNoTracking()
            .FirstOrDefaultAsync(row =>
                row.Id == membershipId &&
                row.UserId == userId &&
                row.TenantId == tenantId &&
                row.WorkspaceId == workspaceId &&
                row.Status == "active",
                httpContext.RequestAborted);

        if (membership is null)
        {
            httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            await httpContext.Response.WriteAsJsonAsync(new { message = "Workspace membership is not active." });
            return;
        }

        if (!string.IsNullOrWhiteSpace(headerWorkspace))
        {
            var workspaceSlug = await dbContext.Workspaces.AsNoTracking()
                .Where(row => row.Id == workspaceId)
                .Select(row => row.Slug)
                .FirstOrDefaultAsync(httpContext.RequestAborted);
            if (!string.Equals(headerWorkspace, workspaceSlug, StringComparison.OrdinalIgnoreCase))
            {
                httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                await httpContext.Response.WriteAsJsonAsync(new { message = "Workspace header does not match authenticated workspace." });
                return;
            }
        }

        await next(httpContext);
    }
}

