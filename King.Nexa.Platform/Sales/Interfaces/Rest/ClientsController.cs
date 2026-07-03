using King.Nexa.Platform.Sales.Application.CommandServices;
using King.Nexa.Platform.Sales.Application.QueryServices;
using King.Nexa.Platform.Sales.Interfaces.Rest.Resources;
using King.Nexa.Platform.Sales.Interfaces.Rest.Transform;
using King.Nexa.Platform.Shared.Application.ReadModels;
using King.Nexa.Platform.Shared.Application.Security;
using King.Nexa.Platform.Shared.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace King.Nexa.Platform.Sales.Interfaces.Rest;

[ApiController]
[Authorize(Policy = NexaAuthorizationPolicies.WorkspaceMember)]
[Route("api/v1/client-accounts")]
public class ClientsController(
    IClientAccountQueryService queryService,
    IClientAccountCommandService commandService,
    ICurrentWorkspaceContext workspaceContext,
    IWorkspaceReadModelQueryService readModels) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? code, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(code))
        {
            var client = await queryService.FindByCodeAsync(code, cancellationToken);
            return client is null ? NotFound() : Ok(ClientAccountResourceAssembler.ToResource(client));
        }

        var clients = await queryService.ListAsync(cancellationToken);
        return Ok(clients.Select(ClientAccountResourceAssembler.ToResource));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var client = await queryService.FindByIdAsync(id, cancellationToken);
        return client is null ? NotFound() : Ok(ClientAccountResourceAssembler.ToResource(client));
    }

    [HttpGet("{id:int}/financial-profile")]
    public async Task<IActionResult> GetFinancialProfile(int id, CancellationToken cancellationToken)
    {
        var profile = await readModels.GetClientFinancialProfileAsync(id, cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpPost]
    [Authorize(Policy = NexaAuthorizationPolicies.CanAcceptPurchaseRequest)]
    public async Task<IActionResult> Create(CreateClientAccountResource resource, CancellationToken cancellationToken)
    {
        var client = await commandService.CreateAsync(
            ClientAccountResourceAssembler.ToEntity(resource, RequireTenantId()),
            cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = client.Id }, ClientAccountResourceAssembler.ToResource(client));
    }

    [HttpPut("{id:int}")]
    [HttpPatch("{id:int}")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanAcceptPurchaseRequest)]
    public async Task<IActionResult> Update(int id, CreateClientAccountResource resource, CancellationToken cancellationToken)
    {
        var client = await commandService.UpdateAsync(
            id,
            ClientAccountResourceAssembler.ToEntity(resource, RequireTenantId()),
            cancellationToken);
        return client is null ? NotFound() : Ok(ClientAccountResourceAssembler.ToResource(client));
    }

    [HttpPut("/api/v1/profile/client-account")]
    public async Task<IActionResult> UpdateCurrentBuyerProfile(CreateClientAccountResource resource, CancellationToken cancellationToken)
    {
        if (workspaceContext.ClientAccountId is not { } clientAccountId) return Forbid();
        var client = await commandService.UpdateAsync(
            clientAccountId,
            ClientAccountResourceAssembler.ToEntity(resource, RequireTenantId()),
            cancellationToken);
        return client is null ? NotFound() : Ok(ClientAccountResourceAssembler.ToResource(client));
    }

    private int RequireTenantId() => workspaceContext.TenantId is > 0
        ? workspaceContext.TenantId.Value
        : throw new InvalidOperationException("An authenticated tenant context is required.");
}
