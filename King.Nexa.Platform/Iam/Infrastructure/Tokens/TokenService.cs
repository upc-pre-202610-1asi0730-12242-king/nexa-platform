using System.Security.Claims;
using King.Nexa.Platform.Iam.Application.OutboundServices;
using King.Nexa.Platform.Iam.Domain.Model.Aggregates;
using King.Nexa.Platform.Shared.Infrastructure.Security.Authentication;
using King.Nexa.Platform.TenantManagement.Domain.Model.Aggregates;
using King.Nexa.Platform.TenantManagement.Domain.Model.Entities;

namespace King.Nexa.Platform.Iam.Infrastructure.Tokens;

public class TokenService(JwtTokenCodec tokenCodec) : ITokenService
{
    public string GenerateToken(User user, Tenant? tenant = null, Workspace? workspace = null, UserWorkspaceMembership? membership = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new("sub", user.Id.ToString()),
            new("email", user.Email),
            new(ClaimTypes.Role, membership?.Role ?? user.Role),
            new("role", membership?.Role ?? user.Role)
        };

        if (tenant is not null) claims.Add(new Claim("tenant_id", tenant.Id.ToString()));
        if (workspace is not null)
        {
            claims.Add(new Claim("workspace_id", workspace.Id.ToString()));
            claims.Add(new Claim("workspace_slug", workspace.Slug));
        }
        if (membership is not null)
        {
            claims.Add(new Claim("membership_id", membership.Id.ToString()));
            if (membership.ClientAccountId.HasValue)
                claims.Add(new Claim("client_account_id", membership.ClientAccountId.Value.ToString()));
        }

        return tokenCodec.CreateToken(claims);
    }
}

