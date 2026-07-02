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

    [HttpGet("/api/v1/notification-records")]
    [Obsolete("Use GET /api/v1/notifications.")]
    public Task<ActionResult<IEnumerable<NotificationRecordResource>>> GetAllLegacy(CancellationToken cancellationToken) => GetAll(cancellationToken);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<NotificationRecordResource>> GetById(int id, CancellationToken cancellationToken)
    {
        var notification = await queryService.GetByIdAsync(id, cancellationToken);
        return notification is null ? NotFound() : Ok(NotificationRecordResourceFromEntityAssembler.ToResourceFromEntity(notification));
    }

    [HttpGet("/api/v1/notification-records/{id:int}")]
    [Obsolete("Use GET /api/v1/notifications/{id}.")]
    public Task<ActionResult<NotificationRecordResource>> GetByIdLegacy(int id, CancellationToken cancellationToken) => GetById(id, cancellationToken);

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

    [HttpPost("/api/v1/notification-records")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageDocuments)]
    [Obsolete("Use POST /api/v1/notifications.")]
    public Task<ActionResult<NotificationRecordResource>> CreateLegacy([FromBody] UpsertNotificationRecordResource resource, CancellationToken cancellationToken) => Create(resource, cancellationToken);

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

    [HttpPatch("/api/v1/notification-records/{id:int}")]
    [HttpPut("/api/v1/notification-records/{id:int}")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageDocuments)]
    [Obsolete("Use PATCH or PUT /api/v1/notifications/{id}.")]
    public Task<ActionResult<NotificationRecordResource>> UpdateLegacy(int id, [FromBody] UpsertNotificationRecordResource resource, CancellationToken cancellationToken) => Update(id, resource, cancellationToken);

    [HttpPost("{id:int}/reads")]
    public async Task<ActionResult<NotificationRecordResource>> MarkRead(int id, CancellationToken cancellationToken)
    {
        var notification = await commandService.MarkReadAsync(id, cancellationToken);
        return notification is null ? NotFound() : Ok(NotificationRecordResourceFromEntityAssembler.ToResourceFromEntity(notification));
    }

    [HttpPut("{id:int}/read")]
    [Obsolete("Use POST /api/v1/notifications/{id}/reads.")]
    public Task<ActionResult<NotificationRecordResource>> MarkReadLegacy(int id, CancellationToken cancellationToken) => MarkRead(id, cancellationToken);

    [HttpPut("/api/v1/notification-records/{id:int}/read")]
    [Obsolete("Use POST /api/v1/notifications/{id}/reads.")]
    public Task<ActionResult<NotificationRecordResource>> MarkReadRecordLegacy(int id, CancellationToken cancellationToken) => MarkRead(id, cancellationToken);

    [HttpDelete("{id:int}")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageDocuments)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await commandService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpDelete("/api/v1/notification-records/{id:int}")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageDocuments)]
    [Obsolete("Use DELETE /api/v1/notifications/{id}.")]
    public Task<IActionResult> DeleteLegacy(int id, CancellationToken cancellationToken) => Delete(id, cancellationToken);
}
