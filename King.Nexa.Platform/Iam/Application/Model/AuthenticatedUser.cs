using King.Nexa.Platform.Iam.Domain.Model.Aggregates;
using King.Nexa.Platform.TenantManagement.Domain.Model.Aggregates;
using King.Nexa.Platform.TenantManagement.Domain.Model.Entities;

namespace King.Nexa.Platform.Iam.Application.Model;

public record AuthenticatedUser(
    User User,
    string AccessToken,
    Tenant? Tenant = null,
    Workspace? Workspace = null,
    UserWorkspaceMembership? Membership = null);

