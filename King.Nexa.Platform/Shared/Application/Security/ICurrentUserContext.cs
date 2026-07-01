using System.Security.Claims;

namespace King.Nexa.Platform.Shared.Application.Security;

public interface ICurrentUserContext
{
    bool IsAuthenticated { get; }
    int? UserId { get; }
    string? Email { get; }
    string? Role { get; }
    ClaimsPrincipal Principal { get; }
}
