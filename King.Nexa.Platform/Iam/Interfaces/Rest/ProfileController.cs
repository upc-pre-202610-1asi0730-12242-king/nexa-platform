using King.Nexa.Platform.Iam.Application.CommandServices;
using King.Nexa.Platform.Shared.Application.Security;
using King.Nexa.Platform.Shared.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace King.Nexa.Platform.Iam.Interfaces.Rest;

[ApiController]
[Authorize(Policy = NexaAuthorizationPolicies.WorkspaceMember)]
[Route("api/v1/profile")]
public sealed class ProfileController(IUserCommandService commands, ICurrentUserContext currentUser) : ControllerBase
{
    /// <summary>Changes the authenticated user's password without returning credentials or a replacement token.</summary>
    [HttpPost("password-changes")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword(ChangePasswordResource resource, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(resource.CurrentPassword) || string.IsNullOrWhiteSpace(resource.NewPassword))
            return BadRequest(new { message = "Current and new passwords are required." });
        if (!string.Equals(resource.NewPassword, resource.ConfirmPassword, StringComparison.Ordinal))
            return BadRequest(new { message = "Password confirmation does not match." });

        try
        {
            var changed = await commands.ChangePasswordAsync(CurrentUserId(), resource.CurrentPassword, resource.NewPassword, cancellationToken);
            return changed ? NoContent() : NotFound();
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    private int CurrentUserId() => currentUser.UserId ?? throw new InvalidOperationException("Current user is required.");
}

public sealed record ChangePasswordResource(string CurrentPassword, string NewPassword, string ConfirmPassword);
