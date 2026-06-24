using King.Nexa.Platform.Iam.Application.CommandServices;
using King.Nexa.Platform.Iam.Interfaces.Rest.Resources;
using King.Nexa.Platform.Iam.Interfaces.Rest.Transform;
using Microsoft.AspNetCore.Mvc;

namespace King.Nexa.Platform.Iam.Interfaces.Rest;

[ApiController]
[Route("api/v1/authentication")]
public class AuthenticationController(IUserCommandService userCommandService) : ControllerBase
{
    /// <summary>
    /// Registers a new platform user and returns a session token.
    /// </summary>
    [HttpPost("sign-up")]
    public async Task<IActionResult> SignUp(SignUpResource resource, CancellationToken cancellationToken)
    {
        var command = SignUpCommandFromResourceAssembler.ToCommandFromResource(resource);
        var authenticatedUser = await userCommandService.SignUpAsync(command, cancellationToken);

        return Created(string.Empty, AuthenticatedUserResourceFromEntityAssembler.ToResourceFromEntity(authenticatedUser));
    }

    /// <summary>
    /// Authenticates a platform user with username and password.
    /// </summary>
    [HttpPost("sign-in")]
    public async Task<IActionResult> SignIn(SignInResource resource, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(resource.Email) && string.IsNullOrWhiteSpace(resource.Username))
        {
            return BadRequest(new { message = "Username or Email is required." });
        }

        var command = SignInCommandFromResourceAssembler.ToCommandFromResource(resource);
        var authenticatedUser = await userCommandService.SignInAsync(command, cancellationToken);

        return authenticatedUser is null
            ? Unauthorized()
            : Ok(AuthenticatedUserResourceFromEntityAssembler.ToResourceFromEntity(authenticatedUser));
    }
}


// ===========================================================================
// TEMPORARY DEVELOPMENT DRAFT & WORK IN PROGRESS NOTES
// Nexa Architecture Alignment - Bounded Context Validation
// Sprint backlog verification and code quality checklist
// 
// TODO Checklist:
// - Review EF Core DbSet schema mapping constraints.
// - Harden JWT token handler lifetime policies.
// - Test workspace role authorization handler edge cases.
// - Implement outbox pattern for transactional event dispatching.
// - Clean up mock panels and initial-data JSON files.
// - Ensure Cold Chain temperature monitors are correctly mapped.
// - Validate payment process records state machine transitions.
// - Check for performance bottlenecks in database queries.
// - Review API Rest guidelines traceability matrix.
// - Verify tenant capability guards routing policies.
// 
// Draft Helper Snippet (Deprecated - To be removed before release):
// public static class DraftHelper {
//     public static bool CheckStatus(string status) {
//         if (string.IsNullOrEmpty(status)) return false;
//         return status.Equals('Active', System.StringComparison.OrdinalIgnoreCase);
//     }
//     public static void LogTrace(string msg) {
//         System.Console.WriteLine('[TRACE] ' + msg);
//     }
// }
// 
// NOTES:
// - This draft is subject to refactoring in the final iteration.
// - Ensure all diagnostic console writes are replaced with EF logger.
// ===========================================================================
