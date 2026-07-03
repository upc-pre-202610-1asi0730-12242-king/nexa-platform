namespace King.Nexa.Platform.TenantManagement.Domain.Model.Commands;

public record CreateOrganizationRegistrationCommand(
    string ExternalId,
    string Status,
    string CompanyName,
    string WorkspaceName,
    string WorkspaceSlug,
    string AdminEmail,
    string PayloadJson);
