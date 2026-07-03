using King.Nexa.Platform.Invoicing.Application.CommandServices;
using King.Nexa.Platform.Invoicing.Application.QueryServices;
using King.Nexa.Platform.Invoicing.Interfaces.Rest.Resources;
using King.Nexa.Platform.Invoicing.Interfaces.Rest.Transform;
using King.Nexa.Platform.Shared.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace King.Nexa.Platform.Invoicing.Interfaces.Rest;

[ApiController]
[Authorize(Policy = NexaAuthorizationPolicies.WorkspaceMember)]
[Route("api/v1/notifications")]
public class NotificationRecordsController(
    INotificationRecordQueryService queryService,
    INotificationRecordCommandService commandService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotificationRecordResource>>> GetAll(CancellationToken cancellationToken)
    {
        var notifications = await queryService.ListAsync(cancellationToken);
        return Ok(notifications.Select(NotificationRecordResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<NotificationRecordResource>> GetById(int id, CancellationToken cancellationToken)
    {
        var notification = await queryService.GetByIdAsync(id, cancellationToken);
        return notification is null ? NotFound() : Ok(NotificationRecordResourceFromEntityAssembler.ToResourceFromEntity(notification));
    }

    [HttpPost]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageDocuments)]
    public async Task<ActionResult<NotificationRecordResource>> Create([FromBody] UpsertNotificationRecordResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var notification = await commandService.CreateAsync(NotificationRecordResourceFromEntityAssembler.ToEntityFromResource(resource), cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = notification.Id }, NotificationRecordResourceFromEntityAssembler.ToResourceFromEntity(notification));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPatch("{id:int}")]
    [HttpPut("{id:int}")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageDocuments)]
    public async Task<ActionResult<NotificationRecordResource>> Update(int id, [FromBody] UpsertNotificationRecordResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var notification = await commandService.UpdateAsync(id, NotificationRecordResourceFromEntityAssembler.ToEntityFromResource(resource), cancellationToken);
            return notification is null ? NotFound() : Ok(NotificationRecordResourceFromEntityAssembler.ToResourceFromEntity(notification));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:int}/reads")]
    public async Task<ActionResult<NotificationRecordResource>> MarkRead(int id, CancellationToken cancellationToken)
    {
        var notification = await commandService.MarkReadAsync(id, cancellationToken);
        return notification is null ? NotFound() : Ok(NotificationRecordResourceFromEntityAssembler.ToResourceFromEntity(notification));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageDocuments)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await commandService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

}
