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
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var clients = await queryService.ListAsync(cancellationToken);
        return Ok(clients.Select(ClientAccountResourceAssembler.ToResource));
    }

    [HttpGet("/api/v1/clients")]
    [Obsolete("Use GET /api/v1/client-accounts.")]
    public Task<IActionResult> GetAllLegacy(CancellationToken cancellationToken) => GetAll(cancellationToken);

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var client = await queryService.FindByIdAsync(id, cancellationToken);
        return client is null ? NotFound() : Ok(ClientAccountResourceAssembler.ToResource(client));
    }

    [HttpGet("/api/v1/clients/{id:int}")]
    [Obsolete("Use GET /api/v1/client-accounts/{id}.")]
    public Task<IActionResult> GetByIdLegacy(int id, CancellationToken cancellationToken) => GetById(id, cancellationToken);

    [HttpGet("{id:int}/financial-profile")]
    public async Task<IActionResult> GetFinancialProfile(int id, CancellationToken cancellationToken)
    {
        var profile = await readModels.GetClientFinancialProfileAsync(id, cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpGet("by-code/{code}")]
    [Obsolete("Use GET /api/v1/client-accounts?code={code}.")]
    public async Task<IActionResult> GetByCode(string code, CancellationToken cancellationToken)
    {
        var client = await queryService.FindByCodeAsync(code, cancellationToken);
        return client is null ? NotFound() : Ok(ClientAccountResourceAssembler.ToResource(client));
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

    [HttpPost("/api/v1/clients")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanAcceptPurchaseRequest)]
    [Obsolete("Use POST /api/v1/client-accounts.")]
    public Task<IActionResult> CreateLegacy(CreateClientAccountResource resource, CancellationToken cancellationToken) => Create(resource, cancellationToken);

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

    [HttpPut("/api/v1/clients/{id:int}")]
    [HttpPatch("/api/v1/clients/{id:int}")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanAcceptPurchaseRequest)]
    [Obsolete("Use PUT or PATCH /api/v1/client-accounts/{id}.")]
    public Task<IActionResult> UpdateLegacy(int id, CreateClientAccountResource resource, CancellationToken cancellationToken) => Update(id, resource, cancellationToken);

    private int RequireTenantId() => workspaceContext.TenantId is > 0
        ? workspaceContext.TenantId.Value
        : throw new InvalidOperationException("An authenticated tenant context is required.");
}
