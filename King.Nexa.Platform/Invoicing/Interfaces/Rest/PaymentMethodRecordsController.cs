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
