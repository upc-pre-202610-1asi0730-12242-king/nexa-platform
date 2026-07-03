using King.Nexa.Platform.Shared.Infrastructure.Security.Authorization;
using King.Nexa.Platform.Warehouse.Application.CommandServices;
using King.Nexa.Platform.Warehouse.Application.QueryServices;
using King.Nexa.Platform.Warehouse.Interfaces.Rest.Resources;
using King.Nexa.Platform.Warehouse.Interfaces.Rest.Transform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace King.Nexa.Platform.Warehouse.Interfaces.Rest;

[ApiController]
[Authorize(Policy = NexaAuthorizationPolicies.WorkspaceMember)]
[Route("api/v1/inventory-lots")]
public class InventoryLotsController(
    IInventoryOperationsQueryService queryService,
    IInventoryOperationsCommandService commandService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryLotResource>>> GetAll(CancellationToken cancellationToken)
    {
        var lots = await queryService.ListLotsAsync(cancellationToken);
        return Ok(lots.Select(InventoryOperationResourceAssembler.ToResourceFromSnapshot));
    }

    [HttpGet("{lotCode}")]
    public async Task<ActionResult<InventoryLotResource>> GetByCode(string lotCode, CancellationToken cancellationToken)
    {
        var lot = await queryService.GetLotByCodeAsync(lotCode, cancellationToken);
        return lot is null ? NotFound() : Ok(InventoryOperationResourceAssembler.ToResourceFromSnapshot(lot));
    }

    [HttpPost]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageInventory)]
    public async Task<ActionResult<InventoryLotResource>> Create([FromBody] UpsertInventoryLotResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var lot = await commandService.CreateLotAsync(InventoryOperationResourceAssembler.ToDraftFromResource(resource), cancellationToken);
            var snapshot = await queryService.GetLotByCodeAsync(lot.LotCode, cancellationToken);
            return CreatedAtAction(nameof(GetByCode), new { lotCode = lot.LotCode }, InventoryOperationResourceAssembler.ToResourceFromSnapshot(snapshot!));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:int}")]
    [HttpPut("{id:int}")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageInventory)]
    public async Task<ActionResult<InventoryLotResource>> Update(int id, [FromBody] UpsertInventoryLotResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var lot = await commandService.UpdateLotAsync(id, InventoryOperationResourceAssembler.ToDraftFromResource(resource), cancellationToken);
            if (lot is null) return NotFound();
            var snapshot = await queryService.GetLotByCodeAsync(lot.LotCode, cancellationToken);
            return Ok(InventoryOperationResourceAssembler.ToResourceFromSnapshot(snapshot!));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
