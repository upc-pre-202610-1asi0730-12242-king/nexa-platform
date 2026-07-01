using King.Nexa.Platform.Logistics.Application.CommandServices;
using King.Nexa.Platform.Logistics.Application.QueryServices;
using King.Nexa.Platform.Logistics.Domain.Model.Entities;
using King.Nexa.Platform.Logistics.Domain.Model.Queries;
using King.Nexa.Platform.Logistics.Interfaces.Rest.Resources;
using King.Nexa.Platform.Logistics.Interfaces.Rest.Transform;
using King.Nexa.Platform.Shared.Application.Pagination;
using King.Nexa.Platform.Shared.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace King.Nexa.Platform.Logistics.Interfaces.Rest;

[ApiController]
[Authorize(Policy = NexaAuthorizationPolicies.WorkspaceMember)]
[Route("api/v1/dispatch-orders")]
public class DispatchOrdersController(
    IDispatchOrderQueryService queryService,
    IDispatchOrderCommandService commandService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DispatchOrderResource>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] string? status,
        [FromQuery] int? clientAccountId,
        [FromQuery] int? orderId,
        [FromQuery] DateOnly? createdFrom,
        [FromQuery] DateOnly? createdTo,
        CancellationToken cancellationToken)
    {
        if (HasCollectionQuery(page, pageSize, status, clientAccountId, orderId, createdFrom, createdTo))
        {
            var paged = await queryService.SearchAsync(
                new DispatchOrderCollectionQuery(
                    new PaginationRequest(page, pageSize),
                    status,
                    clientAccountId,
                    orderId,
                    createdFrom,
                    createdTo),
                cancellationToken);
            return Ok(paged.Map(DispatchOrderResourceAssembler.ToResourceFromEntity));
        }

        var dispatches = await queryService.ListAsync(cancellationToken);
        return Ok(dispatches.Select(DispatchOrderResourceAssembler.ToResourceFromEntity));
    }

    [HttpGet("by-tenant/{tenantId:int}")]
    [ProducesResponseType(typeof(IEnumerable<DispatchOrderResource>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByTenant(int tenantId, CancellationToken cancellationToken) =>
        await GetAll(null, null, null, null, null, null, null, cancellationToken);

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(DispatchOrderResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DispatchOrderResource>> GetById(int id, CancellationToken cancellationToken)
    {
        var dispatch = await queryService.GetByIdAsync(id, cancellationToken);
        return dispatch is null ? NotFound() : Ok(DispatchOrderResourceAssembler.ToResourceFromEntity(dispatch));
    }

    [HttpPost]
    [Authorize(Policy = NexaAuthorizationPolicies.CanStartDispatch)]
    [ProducesResponseType(typeof(DispatchOrderResource), StatusCodes.Status201Created)]
    public async Task<ActionResult<DispatchOrderResource>> Create([FromBody] UpsertDispatchOrderResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var dispatch = await commandService.CreateAsync(DispatchOrderResourceAssembler.ToEntityFromResource(resource), cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = dispatch.Id }, DispatchOrderResourceAssembler.ToResourceFromEntity(dispatch));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("/api/v1/orders/{orderId:int}/dispatch-orders")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanStartDispatch)]
    public async Task<ActionResult<DispatchOrderResource>> CreateForOrder(int orderId, [FromBody] CreateDispatchOrderResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var dispatch = await commandService.CreateForOrderAsync(orderId, resource.ClientAccountId, resource.Code, resource.RouteName, cancellationToken);
            return dispatch is null
                ? NotFound()
                : CreatedAtAction(nameof(GetById), new { id = dispatch.Id }, DispatchOrderResourceAssembler.ToResourceFromEntity(dispatch));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("/api/v1/orders/{orderId:int}/tracking")]
    public async Task<ActionResult<DispatchTrackingResource>> GetOrderTracking(int orderId, CancellationToken cancellationToken)
    {
        var snapshot = await queryService.GetOrderTrackingAsync(orderId, cancellationToken);
        return snapshot is null ? NotFound() : Ok(DispatchOrderResourceAssembler.ToResourceFromSnapshot(snapshot));
    }

    [HttpPatch("{id:int}")]
    [HttpPut("{id:int}")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanStartDispatch)]
    public async Task<ActionResult<DispatchOrderResource>> Update(int id, [FromBody] UpsertDispatchOrderResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var current = await queryService.GetByIdAsync(id, cancellationToken);
            if (current is null) return NotFound();
            var merged = new UpsertDispatchOrderResource
            {
                TenantId = resource.TenantId > 0 ? resource.TenantId : current.TenantId,
                OrderId = resource.OrderId > 0 ? resource.OrderId : current.OrderId,
                ClientAccountId = resource.ClientAccountId > 0 ? resource.ClientAccountId : current.ClientAccountId,
                Code = string.IsNullOrWhiteSpace(resource.Code) ? current.Code : resource.Code,
                Status = string.IsNullOrWhiteSpace(resource.Status) ? current.Status : resource.Status,
                RouteName = string.IsNullOrWhiteSpace(resource.RouteName) ? current.RouteName : resource.RouteName,
                Responsible = string.IsNullOrWhiteSpace(resource.Responsible) ? current.Responsible : resource.Responsible,
                Eta = resource.Eta ?? current.Eta,
                DeliveryWindow = string.IsNullOrWhiteSpace(resource.DeliveryWindow) ? current.DeliveryWindow : resource.DeliveryWindow
            };
            var dispatch = await commandService.UpdateAsync(id, DispatchOrderResourceAssembler.ToEntityFromResource(merged), cancellationToken);
            return dispatch is null ? NotFound() : Ok(DispatchOrderResourceAssembler.ToResourceFromEntity(dispatch));
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
        var deleted = await commandService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("{id:int}/assignees")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanStartDispatch)]
    public async Task<ActionResult<DispatchOrderResource>> Assign(int id, [FromBody] AssignDispatchResource resource, CancellationToken cancellationToken)
    {
        var dispatch = await commandService.AssignAsync(id, resource.Responsible, cancellationToken);
        return dispatch is null ? NotFound() : Ok(DispatchOrderResourceAssembler.ToResourceFromEntity(dispatch));
    }

    [HttpPost("{id:int}/schedules")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanStartDispatch)]
    public async Task<ActionResult<DispatchOrderResource>> Schedule(int id, [FromBody] ScheduleDispatchResource resource, CancellationToken cancellationToken)
    {
        var dispatch = await commandService.ScheduleAsync(id, resource.Eta, resource.DeliveryWindow, resource.Note, cancellationToken);
        return dispatch is null ? NotFound() : Ok(DispatchOrderResourceAssembler.ToResourceFromEntity(dispatch));
    }

    [HttpPost("{id:int}/route-starts")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanStartDispatch)]
    public async Task<ActionResult<DispatchOrderResource>> CreateRouteStart(int id, CancellationToken cancellationToken)
    {
        return await StartRouteCore(id, cancellationToken);
    }

    private async Task<ActionResult<DispatchOrderResource>> StartRouteCore(int id, CancellationToken cancellationToken)
    {
        var dispatch = await commandService.StartRouteAsync(id, cancellationToken);
        return dispatch is null ? NotFound() : Ok(DispatchOrderResourceAssembler.ToResourceFromEntity(dispatch));
    }

    [HttpPost("{id:int}/deliveries")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanStartDispatch)]
    public async Task<ActionResult<DispatchOrderResource>> CreateDeliveryCompletion(int id, CancellationToken cancellationToken)
    {
        return await CompleteCore(id, cancellationToken);
    }

    private async Task<ActionResult<DispatchOrderResource>> CompleteCore(int id, CancellationToken cancellationToken)
    {
        var dispatch = await commandService.CompleteAsync(id, cancellationToken);
        return dispatch is null ? NotFound() : Ok(DispatchOrderResourceAssembler.ToResourceFromEntity(dispatch));
    }

    [HttpPost("{id:int}/incidents")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanStartDispatch)]
    public async Task<ActionResult<DispatchOrderResource>> Incident(int id, [FromBody] DispatchNoteResource resource, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(resource.Note)) return BadRequest("Incident reason is required.");
        var dispatch = await commandService.IncidentAsync(id, resource.Note, cancellationToken);
        return dispatch is null ? NotFound() : Ok(DispatchOrderResourceAssembler.ToResourceFromEntity(dispatch));
    }

    [HttpPost("{id:int}/reschedules")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanStartDispatch)]
    public async Task<ActionResult<DispatchOrderResource>> Reprogram(int id, [FromBody] ScheduleDispatchResource resource, CancellationToken cancellationToken)
    {
        if (resource.Eta == default) return BadRequest("Reschedule date is required.");
        if (string.IsNullOrWhiteSpace(resource.Note)) return BadRequest("Reschedule reason is required.");
        var dispatch = await commandService.ReprogramAsync(id, resource.Eta, resource.DeliveryWindow, resource.Note, cancellationToken);
        return dispatch is null ? NotFound() : Ok(DispatchOrderResourceAssembler.ToResourceFromEntity(dispatch));
    }

    [HttpPost("{id:int}/status-changes")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanStartDispatch)]
    public async Task<ActionResult<DispatchOrderResource>> CreateStatusChange(int id, [FromBody] DispatchStatusChangeResource resource, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(resource.Status)) return BadRequest("Dispatch status is required.");
        var dispatch = await commandService.ChangeStatusAsync(id, resource.Status, resource.Note ?? $"Dispatch status changed to {resource.Status}.", resource.VisibleToBuyer ?? true, cancellationToken);
        return dispatch is null ? NotFound() : Ok(DispatchOrderResourceAssembler.ToResourceFromEntity(dispatch));
    }

    private static bool HasCollectionQuery(int? page, int? pageSize, params object?[] filters) =>
        page.HasValue ||
        pageSize.HasValue ||
        filters.Any(filter => filter switch
        {
            null => false,
            string value => !string.IsNullOrWhiteSpace(value),
            _ => true
        });
}

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

[ApiController]
[Authorize(Policy = NexaAuthorizationPolicies.WorkspaceMember)]
[Route("api/v1/proof-of-delivery-records")]
public class ProofOfDeliveryRecordsController(
    ILogisticsOperationalRecordQueryService queryService,
    ILogisticsOperationalRecordCommandService commandService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProofOfDeliveryRecordResource>>> GetAll(CancellationToken cancellationToken)
    {
        var proofs = await queryService.ListProofsOfDeliveryAsync(cancellationToken);
        return Ok(proofs.Select(DispatchOrderResourceAssembler.ToResourceFromEntity));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProofOfDeliveryRecordResource>> GetById(int id, CancellationToken cancellationToken)
    {
        var proof = await queryService.GetProofOfDeliveryByIdAsync(id, cancellationToken);
        return proof is null ? NotFound() : Ok(DispatchOrderResourceAssembler.ToResourceFromEntity(proof));
    }

    [HttpGet("/api/v1/orders/{orderId:int}/delivery-evidence")]
    public async Task<ActionResult<IEnumerable<ProofOfDeliveryRecordResource>>> GetOrderEvidence(int orderId, CancellationToken cancellationToken)
    {
        var evidence = await queryService.ListProofsOfDeliveryByOrderAsync(orderId, cancellationToken);
        return Ok(evidence.Select(DispatchOrderResourceAssembler.ToResourceFromEntity));
    }

    [HttpPost]
    [Authorize(Policy = NexaAuthorizationPolicies.CanStartDispatch)]
    public async Task<ActionResult<ProofOfDeliveryRecordResource>> Create([FromBody] UpsertProofOfDeliveryRecordResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var proof = await commandService.CreateProofOfDeliveryAsync(DispatchOrderResourceAssembler.ToEntityFromResource(resource), cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = proof.Id }, DispatchOrderResourceAssembler.ToResourceFromEntity(proof));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("/api/v1/dispatch-orders/{dispatchOrderId:int}/proofs-of-delivery")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanStartDispatch)]
    public async Task<ActionResult<ProofOfDeliveryRecordResource>> CreateForDispatch(int dispatchOrderId, [FromBody] CompletePodResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var proof = await commandService.CreateProofOfDeliveryForDispatchAsync(dispatchOrderId, resource.ReceivedBy, resource.CompletedAt, resource.PhotoReference, resource.SignatureReference, resource.Notes, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = proof.Id }, DispatchOrderResourceAssembler.ToResourceFromEntity(proof));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:int}/complete")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanStartDispatch)]
    public async Task<ActionResult<ProofOfDeliveryRecordResource>> Complete(int id, [FromBody] CompletePodResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var proof = await commandService.CompleteProofOfDeliveryAsync(id, resource.ReceivedBy, resource.CompletedAt, resource.PhotoReference, resource.SignatureReference, resource.Notes, cancellationToken);
            return proof is null ? NotFound() : Ok(DispatchOrderResourceAssembler.ToResourceFromEntity(proof));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPatch("{id:int}")]
    [HttpPut("{id:int}")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanStartDispatch)]
    public async Task<ActionResult<ProofOfDeliveryRecordResource>> Update(int id, [FromBody] UpsertProofOfDeliveryRecordResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var proof = await commandService.UpdateProofOfDeliveryAsync(id, DispatchOrderResourceAssembler.ToEntityFromResource(resource), cancellationToken);
            return proof is null ? NotFound() : Ok(DispatchOrderResourceAssembler.ToResourceFromEntity(proof));
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
        var deleted = await commandService.DeleteProofOfDeliveryAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}

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

public record AssignDispatchResource(string Responsible);

public record ScheduleDispatchResource(DateTime Eta, string DeliveryWindow, string? Note);

public record DispatchNoteResource(string? Note);

public record CompletePodResource(
    string ReceivedBy,
    DateTime? CompletedAt,
    bool PhotoReference,
    bool SignatureReference,
    string? Notes);

public record CreateDispatchOrderResource(int? ClientAccountId, string? Code, string? RouteName);

public record DispatchStatusChangeResource(string Status, string? Note, bool? VisibleToBuyer);
