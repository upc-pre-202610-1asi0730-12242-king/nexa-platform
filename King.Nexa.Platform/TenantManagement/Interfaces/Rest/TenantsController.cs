using King.Nexa.Platform.TenantManagement.Application.CommandServices;
using King.Nexa.Platform.TenantManagement.Application.QueryServices;
using King.Nexa.Platform.TenantManagement.Interfaces.Rest.Resources;
using King.Nexa.Platform.TenantManagement.Interfaces.Rest.Transform;
using King.Nexa.Platform.Shared.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace King.Nexa.Platform.TenantManagement.Interfaces.Rest;

[ApiController]
[Authorize(Policy = NexaAuthorizationPolicies.WorkspaceMember)]
[Route("api/v1/tenants")]
public class TenantsController(ITenantQueryService tenantQueryService, ITenantCommandService tenantCommandService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] string? slug, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(slug))
        {
            var tenant = await tenantQueryService.FindBySlugAsync(slug, cancellationToken);
            return tenant is null ? NotFound() : Ok(TenantResourceAssembler.ToPreviewResource(tenant));
        }
        if (CurrentTenantId() is not { } tenantId) return Forbid();
        var tenants = await tenantQueryService.ListAsync(cancellationToken);
        return Ok(tenants
            .Where(tenant => tenant.Id == tenantId)
            .Select(TenantResourceAssembler.ToResource));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var tenant = await tenantQueryService.FindByIdAsync(id, cancellationToken);
        return tenant is null ? NotFound() : Ok(TenantResourceAssembler.ToResource(tenant));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageWorkspace)]
    public async Task<IActionResult> Update(int id, CreateTenantResource resource, CancellationToken cancellationToken)
    {
        if (CurrentTenantId() is not { } tenantId || tenantId != id) return NotFound();
        var tenant = await tenantCommandService.UpdateAsync(id, TenantResourceAssembler.ToEntity(resource), cancellationToken);
        return tenant is null ? NotFound() : Ok(TenantResourceAssembler.ToResource(tenant));
    }

    private int? CurrentTenantId() =>
        int.TryParse(User.FindFirstValue("tenant_id"), out var tenantId) && tenantId > 0 ? tenantId : null;
}
