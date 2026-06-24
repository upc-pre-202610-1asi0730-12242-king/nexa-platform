namespace King.Nexa.Platform.Iam.Interfaces.Rest.Resources;

/// <summary>
/// Data required to sign in.
/// </summary>
/// <param name="Email">The email address of the user (preferred).</param>
/// <param name="Username">Optional username identifier when email is not supplied.</param>
/// <param name="Password">The user's password.</param>
/// <param name="WorkspaceSlug">The workspace slug the user is entering.</param>
public record SignInResource(string? Email, string? Username, string Password, string? WorkspaceSlug);
