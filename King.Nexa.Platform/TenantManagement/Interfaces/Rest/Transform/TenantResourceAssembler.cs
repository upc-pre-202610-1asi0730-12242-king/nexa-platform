using King.Nexa.Platform.TenantManagement.Domain.Model.Aggregates;
using King.Nexa.Platform.TenantManagement.Interfaces.Rest.Resources;

namespace King.Nexa.Platform.TenantManagement.Interfaces.Rest.Transform;

public static class TenantResourceAssembler
{
    public static TenantResource ToResource(Tenant tenant) =>
        new(tenant.Id, tenant.Name, tenant.LegalName, tenant.Slug, tenant.Ruc, tenant.WorkspaceUrl, tenant.EmailDomain, tenant.Plan, tenant.Status, tenant.Country);

    public static TenantPreviewResource ToPreviewResource(Tenant tenant) =>
        new(tenant.Name, tenant.Slug, tenant.WorkspaceUrl, tenant.Plan, tenant.Status);

    public static Tenant ToEntity(CreateTenantResource resource) =>
        new(resource.Name, resource.LegalName, resource.Slug, resource.Ruc, resource.WorkspaceUrl, resource.EmailDomain, resource.Plan, resource.Status, resource.Country);
}
