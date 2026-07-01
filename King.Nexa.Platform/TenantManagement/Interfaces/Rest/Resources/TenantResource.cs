namespace King.Nexa.Platform.TenantManagement.Interfaces.Rest.Resources;

public record TenantResource(
    int Id,
    string Name,
    string LegalName,
    string Slug,
    string Ruc,
    string WorkspaceUrl,
    string EmailDomain,
    string Plan,
    string Status,
    string Country);

public record TenantPreviewResource(
    string Name,
    string Slug,
    string WorkspaceUrl,
    string Plan,
    string Status);

