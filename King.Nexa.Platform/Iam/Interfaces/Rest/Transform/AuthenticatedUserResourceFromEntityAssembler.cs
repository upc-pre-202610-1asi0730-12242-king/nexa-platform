using King.Nexa.Platform.Iam.Application.Model;
using King.Nexa.Platform.Iam.Interfaces.Rest.Resources;

namespace King.Nexa.Platform.Iam.Interfaces.Rest.Transform;

public static class AuthenticatedUserResourceFromEntityAssembler
{
    /// <summary>
    /// Maps an authenticated user result to its REST resource.
    /// </summary>
    public static AuthenticatedUserResource ToResourceFromEntity(AuthenticatedUser authenticatedUser) =>
        new(
            authenticatedUser.User.Id,
            authenticatedUser.User.Username,
            authenticatedUser.User.Email,
            authenticatedUser.User.Role,
            authenticatedUser.User.FullName,
            authenticatedUser.User.Phone,
            authenticatedUser.User.PreferredLanguage,
            authenticatedUser.User.CriticalNotificationsEnabled,
            authenticatedUser.AccessToken,
            authenticatedUser.Tenant?.Id ?? authenticatedUser.Membership?.TenantId,
            authenticatedUser.Workspace?.Id ?? authenticatedUser.Membership?.WorkspaceId,
            authenticatedUser.Workspace?.Slug,
            authenticatedUser.Workspace?.Status,
            authenticatedUser.Membership?.Status,
            authenticatedUser.Membership?.ClientAccountId);
}
