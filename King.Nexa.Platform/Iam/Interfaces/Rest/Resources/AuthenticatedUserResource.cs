namespace King.Nexa.Platform.Iam.Interfaces.Rest.Resources;

/// <summary>
/// Authenticated user response.
/// </summary>
public record AuthenticatedUserResource(
    int Id,
    string Username,
    string Email,
    string Role,
    string FullName,
    string Phone,
    string PreferredLanguage,
    bool CriticalNotificationsEnabled,
    string AccessToken,
    int? TenantId,
    int? WorkspaceId,
    string? WorkspaceSlug,
    string? WorkspaceStatus,
    string? MembershipStatus,
    int? ClientAccountId);
