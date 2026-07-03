using System.Text.Json;
using King.Nexa.Platform.TenantManagement.Domain.Model.Commands;
using King.Nexa.Platform.TenantManagement.Domain.Model.Entities;
using King.Nexa.Platform.TenantManagement.Interfaces.Rest.Resources;

namespace King.Nexa.Platform.TenantManagement.Interfaces.Rest.Transform;

public static class OrganizationRegistrationResourceAssembler
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static OrganizationRegistrationResource ToResource(OrganizationRegistrationRequest entity) =>
        new(
            entity.Id,
            entity.ExternalId,
            entity.Status,
            entity.CompanyName,
            entity.WorkspaceName,
            entity.WorkspaceSlug,
            entity.AdminEmail,
            entity.SubmittedAt);

    public static CreateOrganizationRegistrationCommand ToCommand(CreateOrganizationRegistrationResource resource) =>
        new(
            Required(resource.Id, "Organization registration id is required."),
            resource.Status?.Trim() is { Length: > 0 } status ? status : "pending_review",
            Required(resource.Company?.LegalName, "Company legal name is required."),
            Required(resource.Workspace?.WorkspaceName, "Workspace name is required."),
            NormalizeSlug(Required(resource.Workspace?.WorkspaceSlug, "Workspace slug is required.")),
            Required(resource.Administrator?.Email, "Administrator email is required.").ToLowerInvariant(),
            JsonSerializer.Serialize(resource, JsonOptions));

    private static string Required(string? value, string message)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? throw new ArgumentException(message) : trimmed;
    }

    private static string NormalizeSlug(string value) =>
        string.Join('-', value.Trim().ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
