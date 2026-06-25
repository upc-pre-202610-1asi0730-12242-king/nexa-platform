using King.Nexa.Platform.Shared.Infrastructure.Security.Authorization;
using King.Nexa.Platform.TenantManagement.Application.CommandServices;
using King.Nexa.Platform.TenantManagement.Application.QueryServices;
using King.Nexa.Platform.TenantManagement.Interfaces.Rest.Resources;
using King.Nexa.Platform.TenantManagement.Interfaces.Rest.Transform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace King.Nexa.Platform.TenantManagement.Interfaces.Rest;

[ApiController]
[Route("api/v1/organization-registrations")]
public class OrganizationRegistrationsController(
    IOrganizationRegistrationQueryService queryService,
    IOrganizationRegistrationCommandService commandService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageWorkspace)]
    public async Task<ActionResult<IEnumerable<OrganizationRegistrationResource>>> GetAll(CancellationToken cancellationToken)
    {
        var rows = await queryService.ListNewestAsync(cancellationToken);
        return Ok(rows.Select(OrganizationRegistrationResourceAssembler.ToResource));
    }

    [HttpGet("{externalId}")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageWorkspace)]
    public async Task<ActionResult<OrganizationRegistrationResource>> GetByExternalId(
        string externalId,
        CancellationToken cancellationToken)
    {
        var row = await queryService.FindByExternalIdAsync(externalId, cancellationToken);
        return row is null ? NotFound() : Ok(OrganizationRegistrationResourceAssembler.ToResource(row));
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<OrganizationRegistrationResource>> Create(
        CreateOrganizationRegistrationResource resource,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = OrganizationRegistrationResourceAssembler.ToCommand(resource);
            var created = await commandService.CreateAsync(command, cancellationToken);
            var response = OrganizationRegistrationResourceAssembler.ToResource(created);
            return CreatedAtAction(nameof(GetByExternalId), new { externalId = response.ExternalId }, response);
        }
        catch (ArgumentException error)
        {
            return BadRequest(new { message = error.Message });
        }
        catch (InvalidOperationException error)
        {
            return Conflict(new { message = error.Message });
        }
    }
}
