using King.Nexa.Platform.Logistics.Application.CommandServices;
using King.Nexa.Platform.Logistics.Application.QueryServices;
using King.Nexa.Platform.Logistics.Interfaces.Rest.Resources;
using King.Nexa.Platform.Logistics.Interfaces.Rest.Transform;
using King.Nexa.Platform.Shared.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace King.Nexa.Platform.Logistics.Interfaces.Rest;

[ApiController]
[Authorize(Policy = NexaAuthorizationPolicies.WorkspaceMember)]
[Route("api/v1/dispatch-events")]
public class DispatchEventsController(
    ILogisticsOperationalRecordQueryService queryService,
    ILogisticsOperationalRecordCommandService commandService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DispatchEventResource>>> GetAll(CancellationToken cancellationToken)
    {
        var events = await queryService.ListDispatchEventsAsync(cancellationToken);
        return Ok(events.Select(DispatchOrderResourceAssembler.ToResourceFromEntity));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DispatchEventResource>> GetById(int id, CancellationToken cancellationToken)
    {
        var dispatchEvent = await queryService.GetDispatchEventByIdAsync(id, cancellationToken);
        return dispatchEvent is null ? NotFound() : Ok(DispatchOrderResourceAssembler.ToResourceFromEntity(dispatchEvent));
    }

    [HttpPost]
    [Authorize(Policy = NexaAuthorizationPolicies.CanStartDispatch)]
    public async Task<ActionResult<DispatchEventResource>> Create([FromBody] UpsertDispatchEventResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var dispatchEvent = await commandService.CreateDispatchEventAsync(DispatchOrderResourceAssembler.ToEntityFromResource(resource), cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = dispatchEvent.Id }, DispatchOrderResourceAssembler.ToResourceFromEntity(dispatchEvent));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPatch("{id:int}")]
    [HttpPut("{id:int}")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanStartDispatch)]
    public async Task<ActionResult<DispatchEventResource>> Update(int id, [FromBody] UpsertDispatchEventResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var dispatchEvent = await commandService.UpdateDispatchEventAsync(id, DispatchOrderResourceAssembler.ToEntityFromResource(resource), cancellationToken);
            return dispatchEvent is null ? NotFound() : Ok(DispatchOrderResourceAssembler.ToResourceFromEntity(dispatchEvent));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanStartDispatch)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await commandService.DeleteDispatchEventAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
