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
[Route("api/v1/temperature-logs")]
public class TemperatureLogsController(
    ILogisticsOperationalRecordQueryService queryService,
    ILogisticsOperationalRecordCommandService commandService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TemperatureLogResource>>> GetAll(CancellationToken cancellationToken)
    {
        var logs = await queryService.ListTemperatureLogsAsync(cancellationToken);
        return Ok(logs.Select(DispatchOrderResourceAssembler.ToResourceFromEntity));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TemperatureLogResource>> GetById(int id, CancellationToken cancellationToken)
    {
        var log = await queryService.GetTemperatureLogByIdAsync(id, cancellationToken);
        return log is null ? NotFound() : Ok(DispatchOrderResourceAssembler.ToResourceFromEntity(log));
    }

    [HttpGet("/api/v1/dispatch-orders/{dispatchOrderId:int}/temperature-logs")]
    public async Task<ActionResult<IEnumerable<TemperatureLogResource>>> GetByDispatch(int dispatchOrderId, CancellationToken cancellationToken)
    {
        var logs = await queryService.ListTemperatureLogsByDispatchAsync(dispatchOrderId, cancellationToken);
        return Ok(logs.Select(DispatchOrderResourceAssembler.ToResourceFromEntity));
    }

    [HttpPost]
    [Authorize(Policy = NexaAuthorizationPolicies.CanStartDispatch)]
    public async Task<ActionResult<TemperatureLogResource>> Create([FromBody] UpsertTemperatureLogResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var log = await commandService.CreateTemperatureLogAsync(DispatchOrderResourceAssembler.ToEntityFromResource(resource), cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = log.Id }, DispatchOrderResourceAssembler.ToResourceFromEntity(log));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("/api/v1/dispatch-orders/{dispatchOrderId:int}/temperature-logs")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanStartDispatch)]
    public async Task<ActionResult<TemperatureLogResource>> CreateForDispatch(int dispatchOrderId, [FromBody] UpsertTemperatureLogResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var log = await commandService.CreateTemperatureLogForDispatchAsync(dispatchOrderId, DispatchOrderResourceAssembler.ToEntityFromResource(resource), cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = log.Id }, DispatchOrderResourceAssembler.ToResourceFromEntity(log));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:int}/resolve-alert")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanStartDispatch)]
    public async Task<ActionResult<TemperatureLogResource>> ResolveAlert(int id, CancellationToken cancellationToken)
    {
        var log = await commandService.ResolveTemperatureAlertAsync(id, cancellationToken);
        return log is null ? NotFound() : Ok(DispatchOrderResourceAssembler.ToResourceFromEntity(log));
    }

    [HttpPatch("{id:int}")]
    [HttpPut("{id:int}")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanStartDispatch)]
    public async Task<ActionResult<TemperatureLogResource>> Update(int id, [FromBody] UpsertTemperatureLogResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var log = await commandService.UpdateTemperatureLogAsync(id, DispatchOrderResourceAssembler.ToEntityFromResource(resource), cancellationToken);
            return log is null ? NotFound() : Ok(DispatchOrderResourceAssembler.ToResourceFromEntity(log));
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
        var deleted = await commandService.DeleteTemperatureLogAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
