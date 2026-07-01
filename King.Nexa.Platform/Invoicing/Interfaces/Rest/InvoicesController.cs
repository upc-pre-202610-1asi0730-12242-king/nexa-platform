using King.Nexa.Platform.Invoicing.Application.CommandServices;
using King.Nexa.Platform.Invoicing.Application.QueryServices;
using King.Nexa.Platform.Invoicing.Domain.Model.Commands;
using King.Nexa.Platform.Invoicing.Domain.Model.Queries;
using King.Nexa.Platform.Invoicing.Domain.Model.ValueObjects;
using King.Nexa.Platform.Invoicing.Interfaces.Rest.Resources;
using King.Nexa.Platform.Invoicing.Interfaces.Rest.Transform;
using King.Nexa.Platform.Shared.Application.Pagination;
using King.Nexa.Platform.Shared.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace King.Nexa.Platform.Invoicing.Interfaces.Rest;

[ApiController]
[Authorize(Policy = NexaAuthorizationPolicies.WorkspaceMember)]
[Route("api/v1/[controller]")]
public class InvoicesController(IInvoiceCommandService invoiceCommandService, IInvoiceQueryService invoiceQueryService) : ControllerBase
{
    /// <summary>
    /// Gets all generated invoices.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<InvoiceResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllInvoices(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] string? status,
        [FromQuery] string? paymentStatus,
        [FromQuery] int? clientAccountId,
        [FromQuery] int? orderId,
        [FromQuery] DateOnly? createdFrom,
        [FromQuery] DateOnly? createdTo,
        CancellationToken cancellationToken)
    {
        if (InvoiceControllerQuery.HasCollectionQuery(page, pageSize, status, paymentStatus, clientAccountId, orderId, createdFrom, createdTo))
        {
            var requestedStatus = paymentStatus ?? status;
            PaymentStatus? parsedPaymentStatus = null;
            if (!string.IsNullOrWhiteSpace(requestedStatus))
            {
                if (!Enum.TryParse<PaymentStatus>(requestedStatus, true, out var parsedStatus))
                    return BadRequest(new { message = "Invalid payment status." });
                parsedPaymentStatus = parsedStatus;
            }

            var paged = await invoiceQueryService.SearchAsync(
                new InvoiceCollectionQuery(
                    new PaginationRequest(page, pageSize),
                    parsedPaymentStatus,
                    clientAccountId,
                    orderId,
                    createdFrom,
                    createdTo),
                cancellationToken);
            return Ok(paged.Map(InvoiceResourceFromEntityAssembler.ToResourceFromEntity));
        }

        var invoices = await invoiceQueryService.Handle(new GetAllInvoicesQuery(), cancellationToken);
        return Ok(invoices.Select(InvoiceResourceFromEntityAssembler.ToResourceFromEntity));
    }

    /// <summary>
    /// Gets one invoice by its numeric identifier.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(InvoiceResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInvoiceById(int id, CancellationToken cancellationToken)
    {
        var invoice = await invoiceQueryService.Handle(new GetInvoiceByIdQuery(id), cancellationToken);
        return invoice is null ? NotFound() : Ok(InvoiceResourceFromEntityAssembler.ToResourceFromEntity(invoice));
    }

    /// <summary>
    /// Gets invoices by order identifier.
    /// </summary>
    [HttpGet("by-order/{orderId:int}")]
    [ProducesResponseType(typeof(IEnumerable<InvoiceResource>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInvoicesByOrderId(int orderId, CancellationToken cancellationToken)
    {
        var invoices = await invoiceQueryService.Handle(new GetInvoicesByOrderIdQuery(orderId), cancellationToken);
        return Ok(invoices.Select(InvoiceResourceFromEntityAssembler.ToResourceFromEntity));
    }

    /// <summary>
    /// Gets invoices by payment status.
    /// </summary>
    [HttpGet("by-status/{paymentStatus}")]
    [ProducesResponseType(typeof(IEnumerable<InvoiceResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetInvoicesByPaymentStatus(string paymentStatus, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<PaymentStatus>(paymentStatus, true, out var status))
            return BadRequest(new { message = "Invalid payment status." });

        var invoices = await invoiceQueryService.Handle(new GetInvoicesByPaymentStatusQuery(status), cancellationToken);
        return Ok(invoices.Select(InvoiceResourceFromEntityAssembler.ToResourceFromEntity));
    }

    /// <summary>
    /// Generates an invoice for a completed order.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageDocuments)]
    [ProducesResponseType(typeof(InvoiceResource), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> GenerateInvoice(GenerateInvoiceResource resource, CancellationToken cancellationToken)
    {
        var command = GenerateInvoiceCommandFromResourceAssembler.ToCommandFromResource(resource);
        var invoice = await invoiceCommandService.GenerateAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetInvoiceById), new { id = invoice.Id }, InvoiceResourceFromEntityAssembler.ToResourceFromEntity(invoice));
    }

    /// <summary>
    /// Updates an unpaid invoice.
    /// </summary>
    [HttpPatch("{id:int}")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageDocuments)]
    [ProducesResponseType(typeof(InvoiceResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateInvoice(int id, UpdateInvoiceResource resource, CancellationToken cancellationToken)
    {
        var command = UpdateInvoiceCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var invoice = await invoiceCommandService.UpdateAsync(command, cancellationToken);
        return invoice is null ? NotFound() : Ok(InvoiceResourceFromEntityAssembler.ToResourceFromEntity(invoice));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageDocuments)]
    [ProducesResponseType(typeof(InvoiceResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> ReplaceInvoice(int id, UpdateInvoiceResource resource, CancellationToken cancellationToken)
    {
        return UpdateInvoice(id, resource, cancellationToken);
    }

    /// <summary>
    /// Marks an invoice as paid.
    /// </summary>
    [HttpPost("{id:int}/paid")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageDocuments)]
    [ProducesResponseType(typeof(InvoiceResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkInvoicePaid(int id, CancellationToken cancellationToken)
    {
        var invoice = await invoiceCommandService.MarkPaidAsync(new MarkInvoicePaidCommand(id), cancellationToken);
        if (invoice is null) return NotFound();
        return Ok(InvoiceResourceFromEntityAssembler.ToResourceFromEntity(invoice));
    }

    [HttpPost("{id:int}/status-changes")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageDocuments)]
    [ProducesResponseType(typeof(InvoiceResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateStatusChange(int id, ChangeInvoiceStatusResource resource, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<PaymentStatus>(resource.PaymentStatus, true, out var status))
            return BadRequest(new { message = "Invalid payment status." });

        if (status == PaymentStatus.Paid)
        {
            var invoice = await invoiceCommandService.MarkPaidAsync(new MarkInvoicePaidCommand(id), cancellationToken);
            return invoice is null ? NotFound() : Ok(InvoiceResourceFromEntityAssembler.ToResourceFromEntity(invoice));
        }

        if (status == PaymentStatus.Cancelled)
        {
            var cancelled = await invoiceCommandService.CancelAsync(new CancelInvoiceCommand(id), cancellationToken);
            return cancelled ? NoContent() : NotFound();
        }

        return BadRequest(new { message = "Only Paid and Cancelled status changes are supported." });
    }

    [HttpPost("{id:int}/voidings")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageDocuments)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VoidInvoice(int id, CancellationToken cancellationToken)
    {
        var cancelled = await invoiceCommandService.CancelAsync(new CancelInvoiceCommand(id), cancellationToken);
        return cancelled ? NoContent() : NotFound();
    }

    /// <summary>
    /// Cancels an unpaid invoice.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageDocuments)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelInvoice(int id, CancellationToken cancellationToken)
    {
        var cancelled = await invoiceCommandService.CancelAsync(new CancelInvoiceCommand(id), cancellationToken);
        return cancelled ? NoContent() : NotFound();
    }
}

public record ChangeInvoiceStatusResource(string PaymentStatus);

file static class InvoiceControllerQuery
{
    public static bool HasCollectionQuery(int? page, int? pageSize, params object?[] filters) =>
        page.HasValue ||
        pageSize.HasValue ||
        filters.Any(filter => filter switch
        {
            null => false,
            string value => !string.IsNullOrWhiteSpace(value),
            _ => true
        });
}
