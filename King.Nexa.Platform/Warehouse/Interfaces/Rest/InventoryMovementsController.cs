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
[Route("api/v1/inventory-movements")]
[Route("api/v1/stock-movements")]
public class InventoryMovementsController(
    IInventoryOperationsQueryService queryService,
    IInventoryOperationsCommandService commandService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryMovementResource>>> GetAll(CancellationToken cancellationToken)
    {
        var movements = await queryService.ListMovementsAsync(cancellationToken);
        return Ok(movements.Select(InventoryOperationResourceAssembler.ToResourceFromSnapshot));
    }

    [HttpGet("{code}")]
    public async Task<ActionResult<InventoryMovementResource>> GetByCode(string code, CancellationToken cancellationToken)
    {
        var movement = await queryService.GetMovementByCodeAsync(code, cancellationToken);
        return movement is null ? NotFound() : Ok(InventoryOperationResourceAssembler.ToResourceFromSnapshot(movement));
    }

    [HttpPost]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageInventory)]
    public async Task<ActionResult<InventoryMovementResource>> Create([FromBody] CreateInventoryMovementResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var movement = await commandService.CreateMovementAsync(InventoryOperationResourceAssembler.ToDraftFromResource(resource), cancellationToken);
            var snapshot = await queryService.GetMovementByCodeAsync(movement.Code, cancellationToken);
            return CreatedAtAction(nameof(GetByCode), new { code = movement.Code }, InventoryOperationResourceAssembler.ToResourceFromSnapshot(snapshot!));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
