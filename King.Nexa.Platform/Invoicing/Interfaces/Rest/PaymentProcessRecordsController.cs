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
