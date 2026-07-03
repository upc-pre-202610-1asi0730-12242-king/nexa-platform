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
[Route("api/v1/reservations")]
public class ReservationsController(
    IInventoryOperationsQueryService queryService,
    IInventoryOperationsCommandService commandService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryReservationResource>>> GetAll(CancellationToken cancellationToken)
    {
        var reservations = await queryService.ListReservationsAsync(cancellationToken);
        return Ok(reservations.Select(InventoryOperationResourceAssembler.ToResourceFromSnapshot));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<InventoryReservationResource>> GetById(int id, CancellationToken cancellationToken)
    {
        var reservation = await queryService.GetReservationAsync(id, cancellationToken);
        return reservation is null ? NotFound() : Ok(InventoryOperationResourceAssembler.ToResourceFromSnapshot(reservation));
    }

    [HttpPost]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageInventory)]
    public async Task<ActionResult<InventoryReservationResource>> Create([FromBody] CreateInventoryReservationResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var reservation = await commandService.CreateReservationAsync(InventoryOperationResourceAssembler.ToDraftFromResource(resource), cancellationToken);
            var snapshot = await queryService.GetReservationAsync(reservation.Id, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = reservation.Id }, InventoryOperationResourceAssembler.ToResourceFromSnapshot(snapshot!));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:int}/releases")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageInventory)]
    public async Task<ActionResult<InventoryReservationResource>> Release(int id, CancellationToken cancellationToken)
    {
        try
        {
            var reservation = await commandService.ReleaseReservationAsync(id, cancellationToken);
            if (reservation is null) return NotFound();
            var snapshot = await queryService.GetReservationAsync(id, cancellationToken);
            return Ok(InventoryOperationResourceAssembler.ToResourceFromSnapshot(snapshot!));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
