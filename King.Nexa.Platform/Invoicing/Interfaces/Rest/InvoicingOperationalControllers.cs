using King.Nexa.Platform.Invoicing.Application.CommandServices;
using King.Nexa.Platform.Invoicing.Application.QueryServices;
using King.Nexa.Platform.Invoicing.Domain.Model.Entities;
using King.Nexa.Platform.Invoicing.Interfaces.Rest.Resources;
using King.Nexa.Platform.Invoicing.Interfaces.Rest.Transform;
using King.Nexa.Platform.Shared.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace King.Nexa.Platform.Invoicing.Interfaces.Rest;

[ApiController]
[Authorize(Policy = NexaAuthorizationPolicies.WorkspaceMember)]
[Route("api/v1/business-documents")]
public class BusinessDocumentsController(
    IBusinessDocumentCommandService commandService,
    IBusinessDocumentQueryService queryService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BusinessDocumentResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<BusinessDocumentResource>>> GetAll(CancellationToken cancellationToken)
    {
        var documents = await queryService.ListAsync(cancellationToken);
        return Ok(documents.Select(BusinessDocumentResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BusinessDocumentResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BusinessDocumentResource>> GetById(int id, CancellationToken cancellationToken)
    {
        var document = await queryService.GetByIdAsync(id, cancellationToken);
        return document is null ? NotFound() : Ok(BusinessDocumentResourceFromEntityAssembler.ToResourceFromEntity(document));
    }

    [HttpGet("{id:int}/content")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetContent(int id, CancellationToken cancellationToken)
    {
        var content = await queryService.GetContentAsync(id, cancellationToken);
        if (content is null) return NotFound();
        if ((User.IsInRole("Buyer") || User.IsInRole("B2B Buyer")) && !content.VisibleToBuyer)
            return Forbid();
        return File(content.Content, content.ContentType, content.FileName);
    }

    [HttpPost]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageDocuments)]
    [ProducesResponseType(typeof(BusinessDocumentResource), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<BusinessDocumentResource>> Create([FromBody] CreateBusinessDocumentResource resource, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(resource.Type)) return BadRequest("Document type is required.");
        if (string.IsNullOrWhiteSpace(resource.Label)) return BadRequest("Document label is required.");
        try
        {
            var document = await commandService.CreateAsync(BusinessDocumentResourceFromEntityAssembler.ToEntityFromResource(resource), cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = document.Id }, BusinessDocumentResourceFromEntityAssembler.ToResourceFromEntity(document));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("generations")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageDocuments)]
    [ProducesResponseType(typeof(BusinessDocumentResource), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<BusinessDocumentResource>> Generate(
        [FromBody] GenerateBusinessDocumentResource resource,
        CancellationToken cancellationToken)
    {
        if (resource.OrderId <= 0) return BadRequest("Order is required.");
        if (string.IsNullOrWhiteSpace(resource.Type)) return BadRequest("Document type is required.");
        try
        {
            var document = await commandService.GenerateAsync(
                new GenerateBusinessDocumentCommand(resource.OrderId, resource.Type),
                cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = document.Id }, BusinessDocumentResourceFromEntityAssembler.ToResourceFromEntity(document));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("upload")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageDocuments)]
    [RequestSizeLimit(20_000_000)]
    [ProducesResponseType(typeof(BusinessDocumentResource), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<BusinessDocumentResource>> Upload([FromForm] UploadBusinessDocumentResource resource, CancellationToken cancellationToken)
    {
        if (resource.File is null || resource.File.Length == 0) return BadRequest("Document file is required.");

        try
        {
            var document = await commandService.UploadAsync(new UploadBusinessDocumentCommand(
                resource.TenantId,
                resource.OrderId,
                resource.ClientAccountId,
                resource.Type,
                resource.Label,
                resource.VisibleToBuyer,
                resource.Required,
                resource.File), cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = document.Id }, BusinessDocumentResourceFromEntityAssembler.ToResourceFromEntity(document));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:int}/status-changes")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageDocuments)]
    [ProducesResponseType(typeof(BusinessDocumentResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BusinessDocumentResource>> CreateStatusChange(int id, ChangeBusinessDocumentStatusResource resource, CancellationToken cancellationToken)
    {
        var document = await commandService.ChangeStatusAsync(
            new ChangeBusinessDocumentStatusCommand(id, resource.Status, resource.VisibleToBuyer),
            cancellationToken);
        return document is null ? NotFound() : Ok(BusinessDocumentResourceFromEntityAssembler.ToResourceFromEntity(document));
    }

    [HttpPut("{id:int}/mark-ready")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageDocuments)]
    public async Task<ActionResult<BusinessDocumentResource>> MarkReady(int id, CancellationToken cancellationToken)
    {
        var document = await commandService.MarkReadyAsync(id, cancellationToken);
        return document is null ? NotFound() : Ok(BusinessDocumentResourceFromEntityAssembler.ToResourceFromEntity(document));
    }

    [HttpPut("{id:int}/authorize-buyer")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageDocuments)]
    public async Task<ActionResult<BusinessDocumentResource>> AuthorizeBuyer(int id, CancellationToken cancellationToken)
    {
        var document = await commandService.AuthorizeBuyerAsync(id, cancellationToken);
        return document is null ? NotFound() : Ok(BusinessDocumentResourceFromEntityAssembler.ToResourceFromEntity(document));
    }

    [HttpPut("{id:int}/mark-missing")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageDocuments)]
    public async Task<ActionResult<BusinessDocumentResource>> MarkMissing(int id, CancellationToken cancellationToken)
    {
        var document = await commandService.MarkMissingAsync(id, cancellationToken);
        return document is null ? NotFound() : Ok(BusinessDocumentResourceFromEntityAssembler.ToResourceFromEntity(document));
    }

}

[ApiController]
[Authorize(Policy = NexaAuthorizationPolicies.WorkspaceMember)]
[Route("api/v1/payment-method-records")]
public class PaymentMethodRecordsController(
    IPaymentMethodRecordQueryService queryService,
    IPaymentMethodRecordCommandService commandService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PaymentMethodRecordResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<PaymentMethodRecordResource>>> GetAll(CancellationToken cancellationToken)
    {
        var records = await queryService.ListAsync(cancellationToken);
        return Ok(records.Select(PaymentMethodRecordResourceAssembler.ToResourceFromEntity));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PaymentMethodRecordResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentMethodRecordResource>> GetById(int id, CancellationToken cancellationToken)
    {
        var record = await queryService.GetByIdAsync(id, cancellationToken);
        return record is null ? NotFound() : Ok(PaymentMethodRecordResourceAssembler.ToResourceFromEntity(record));
    }

    [HttpPost]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManagePaymentMethods)]
    [ProducesResponseType(typeof(PaymentMethodRecordResource), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PaymentMethodRecordResource>> Create([FromBody] CreatePaymentMethodRecordResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var record = await commandService.CreateAsync(PaymentMethodRecordResourceAssembler.ToEntityFromResource(resource), cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = record.Id }, PaymentMethodRecordResourceAssembler.ToResourceFromEntity(record));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:int}/status-changes")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManagePaymentMethods)]
    [ProducesResponseType(typeof(PaymentMethodRecordResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentMethodRecordResource>> CreateStatusChange(int id, [FromBody] ChangePaymentMethodRecordStatusResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var record = await commandService.ChangeStatusAsync(id, resource.Status, resource.IsDefault, cancellationToken);
            return record is null ? NotFound() : Ok(PaymentMethodRecordResourceAssembler.ToResourceFromEntity(record));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

[ApiController]
[Authorize(Policy = NexaAuthorizationPolicies.WorkspaceMember)]
[Route("api/v1/payment-process-records")]
public class PaymentProcessRecordsController(
    IPaymentProcessRecordCommandService commandService,
    IPaymentProcessRecordQueryService queryService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PaymentProcessRecordResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<PaymentProcessRecordResource>>> GetAll(CancellationToken cancellationToken)
    {
        var records = await queryService.ListAsync(cancellationToken);
        return Ok(records.Select(PaymentProcessRecordResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PaymentProcessRecordResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentProcessRecordResource>> GetById(int id, CancellationToken cancellationToken)
    {
        var record = await queryService.GetByIdAsync(id, cancellationToken);
        return record is null ? NotFound() : Ok(PaymentProcessRecordResourceFromEntityAssembler.ToResourceFromEntity(record));
    }

    [HttpPost]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageDocuments)]
    [ProducesResponseType(typeof(PaymentProcessRecordResource), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PaymentProcessRecordResource>> Create([FromBody] CreatePaymentProcessRecordResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var record = await commandService.CreateAsync(PaymentProcessRecordResourceFromEntityAssembler.ToEntityFromResource(resource), cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = record.Id }, PaymentProcessRecordResourceFromEntityAssembler.ToResourceFromEntity(record));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:int}/status-changes")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageDocuments)]
    [ProducesResponseType(typeof(PaymentProcessRecordResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<ActionResult<PaymentProcessRecordResource>> CreateStatusChange(int id, ChangePaymentProcessStatusResource resource, CancellationToken cancellationToken) =>
        ChangeStatus(id, resource.Status, cancellationToken);

    [HttpPost("{id:int}/confirmations")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageDocuments)]
    public Task<ActionResult<PaymentProcessRecordResource>> CreateConfirmation(int id, CancellationToken cancellationToken) =>
        ChangeStatus(id, "confirmed", cancellationToken);

    [HttpPost("{id:int}/rejections")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageDocuments)]
    public Task<ActionResult<PaymentProcessRecordResource>> CreateRejection(int id, CancellationToken cancellationToken) =>
        ChangeStatus(id, "failed", cancellationToken);

    private async Task<ActionResult<PaymentProcessRecordResource>> ChangeStatus(int id, string status, CancellationToken cancellationToken)
    {
        var payment = await commandService.ChangeStatusAsync(new ChangePaymentProcessStatusCommand(id, status), cancellationToken);
        return payment is null ? NotFound() : Ok(PaymentProcessRecordResourceFromEntityAssembler.ToResourceFromEntity(payment));
    }

}

[ApiController]
[Authorize(Policy = NexaAuthorizationPolicies.WorkspaceMember)]
[Route("api/v1/notification-records")]
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

    [HttpPut("{id:int}/read")]
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

public record UploadBusinessDocumentResource(
    int TenantId,
    int? OrderId,
    int? ClientAccountId,
    string Type,
    string Label,
    bool VisibleToBuyer,
    bool Required,
    IFormFile File);

public record ChangePaymentProcessStatusResource(string Status);

public record ChangeBusinessDocumentStatusResource(string Status, bool? VisibleToBuyer = null);
