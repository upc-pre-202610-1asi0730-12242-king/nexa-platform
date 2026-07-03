using System.Text.Json;
using System.Text.Json.Serialization;

namespace King.Nexa.Platform.TenantManagement.Interfaces.Rest.Resources;

public record CreateOrganizationRegistrationResource(
    string Id,
    string? Status,
    OrganizationRegistrationCompanyResource Company,
    OrganizationRegistrationWorkspaceResource Workspace,
    OrganizationRegistrationAdministratorResource Administrator)
{
    [JsonExtensionData]
    public Dictionary<string, JsonElement> ExtensionData { get; init; } = [];
}

public record OrganizationRegistrationCompanyResource(string LegalName);

public record OrganizationRegistrationWorkspaceResource(string WorkspaceName, string WorkspaceSlug);

public record OrganizationRegistrationAdministratorResource(string Email);

public record OrganizationRegistrationResource(
    int Id,
    string ExternalId,
    string Status,
    string CompanyName,
    string WorkspaceName,
    string WorkspaceSlug,
    string AdminEmail,
    DateTime SubmittedAt);
