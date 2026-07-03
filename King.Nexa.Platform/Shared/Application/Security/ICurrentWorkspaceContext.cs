namespace King.Nexa.Platform.Shared.Application.Security;

public interface ICurrentWorkspaceContext
{
    int? TenantId { get; }
    int? WorkspaceId { get; }
    int? MembershipId { get; }
    int? ClientAccountId { get; }
    string? WorkspaceSlug { get; }
    bool HasWorkspaceScope { get; }
}
