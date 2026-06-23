using System.Security.Claims;
using King.Nexa.Platform.Shared.Application.Security;

namespace King.Nexa.Platform.Shared.Infrastructure.Security.Context;

public class HttpCurrentWorkspaceContext(IHttpContextAccessor accessor) : ICurrentWorkspaceContext
{
    private ClaimsPrincipal Principal => accessor.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());

    public int? TenantId => TryInt(Principal.FindFirstValue("tenant_id"));
    public int? WorkspaceId => TryInt(Principal.FindFirstValue("workspace_id"));
    public int? MembershipId => TryInt(Principal.FindFirstValue("membership_id"));
    public int? ClientAccountId => TryInt(Principal.FindFirstValue("client_account_id"));
    public string? WorkspaceSlug => Principal.FindFirstValue("workspace_slug");
    public bool HasWorkspaceScope => TenantId.HasValue && WorkspaceId.HasValue && MembershipId.HasValue;

    private static int? TryInt(string? value) => int.TryParse(value, out var parsed) ? parsed : null;
}

