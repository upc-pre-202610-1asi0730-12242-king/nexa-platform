using System.Security.Claims;
using King.Nexa.Platform.Shared.Application.Security;

namespace King.Nexa.Platform.Shared.Infrastructure.Security.Context;

public class HttpCurrentUserContext(IHttpContextAccessor accessor) : ICurrentUserContext
{
    public ClaimsPrincipal Principal => accessor.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());
    public bool IsAuthenticated => Principal.Identity?.IsAuthenticated == true;
    public int? UserId => TryInt(Principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? Principal.FindFirstValue("sub"));
    public string? Email => Principal.FindFirstValue("email");
    public string? Role => Principal.FindFirstValue(ClaimTypes.Role) ?? Principal.FindFirstValue("role");

    private static int? TryInt(string? value) => int.TryParse(value, out var parsed) ? parsed : null;
}

