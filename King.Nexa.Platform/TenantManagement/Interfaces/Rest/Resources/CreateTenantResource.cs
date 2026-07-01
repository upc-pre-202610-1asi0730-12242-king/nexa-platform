namespace King.Nexa.Platform.TenantManagement.Interfaces.Rest.Resources;

public record CreateTenantResource(
    string Name,
    string LegalName,
    string Slug,
    string Ruc,
    string WorkspaceUrl,
    string EmailDomain,
    string Plan,
    string Status,
    string Country);

