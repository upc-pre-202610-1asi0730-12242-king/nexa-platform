using King.Nexa.Platform.Iam.Domain.Model.Aggregates;
using King.Nexa.Platform.TenantManagement.Domain.Model.Aggregates;
using King.Nexa.Platform.TenantManagement.Domain.Model.Entities;

namespace King.Nexa.Platform.Iam.Application.OutboundServices;

public interface ITokenService
{
    string GenerateToken(User user, Tenant? tenant = null, Workspace? workspace = null, UserWorkspaceMembership? membership = null);
}

