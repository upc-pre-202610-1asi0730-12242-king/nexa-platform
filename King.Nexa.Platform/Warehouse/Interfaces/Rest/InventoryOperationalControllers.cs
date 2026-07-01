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
