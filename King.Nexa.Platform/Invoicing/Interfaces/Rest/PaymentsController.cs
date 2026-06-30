using King.Nexa.Platform.Invoicing.Application.CommandServices;
using King.Nexa.Platform.Invoicing.Application.QueryServices;
using King.Nexa.Platform.Invoicing.Domain.Model.Aggregates;
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
[Route("api/v1/payments")]
public class PaymentsController(IPaymentCommandService paymentCommandService, IPaymentQueryService paymentQueryService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PaymentResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllPayments(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] string? status,
        [FromQuery] int? clientAccountId,
        [FromQuery] int? orderId,
        [FromQuery] int? invoiceId,
        [FromQuery] DateOnly? createdFrom,
        [FromQuery] DateOnly? createdTo,
        CancellationToken cancellationToken)
    {
        if (PaymentControllerQuery.HasCollectionQuery(page, pageSize, status, clientAccountId, orderId, invoiceId, createdFrom, createdTo))
        {
            PaymentStatus? paymentStatus = null;
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (!Enum.TryParse<PaymentStatus>(status, true, out var parsedStatus))
                    return BadRequest(new { message = "Invalid payment status.", allowed = new[] { "pending", "confirmed", "failed", "rejected", "cancelled" } });
                paymentStatus = parsedStatus;
            }

            var paged = await paymentQueryService.SearchAsync(
                new PaymentCollectionQuery(
                    new PaginationRequest(page, pageSize),
                    paymentStatus,
                    clientAccountId,
                    orderId,
                    invoiceId,
                    createdFrom,
                    createdTo),
                cancellationToken);
            return Ok(paged.Map(PaymentResourceFromEntityAssembler.ToResourceFromEntity));
        }

        var payments = await paymentQueryService.Handle(new GetAllPaymentsQuery(), cancellationToken);
        return Ok(payments.Select(PaymentResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PaymentResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaymentById(int id, CancellationToken cancellationToken)
    {
        var payment = await paymentQueryService.Handle(new GetPaymentByIdQuery(id), cancellationToken);
        return payment is null ? NotFound() : Ok(PaymentResourceFromEntityAssembler.ToResourceFromEntity(payment));
    }

    [HttpGet("/api/v1/invoices/{invoiceId:int}/payments")]
    [ProducesResponseType(typeof(IEnumerable<PaymentResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPaymentsByInvoiceSubresource(int invoiceId, CancellationToken cancellationToken)
    {
        var payments = await paymentQueryService.Handle(new GetPaymentsByInvoiceIdQuery(invoiceId), cancellationToken);
        return Ok(payments.Select(PaymentResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpGet("by-invoice/{invoiceId:int}")]
    [Obsolete("Use GET /api/v1/payments?invoiceId={invoiceId}.")]
    [ProducesResponseType(typeof(IEnumerable<PaymentResource>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetPaymentsByInvoiceId(int invoiceId, CancellationToken cancellationToken)
    {
        return GetPaymentsByInvoiceSubresource(invoiceId, cancellationToken);
    }

    [HttpGet("by-status/{status}")]
    [Obsolete("Use GET /api/v1/payments?status={status}.")]
    [ProducesResponseType(typeof(IEnumerable<PaymentResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPaymentsByStatus(string status, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<PaymentStatus>(status, true, out var paymentStatus))
            return BadRequest(new { message = "Invalid payment status.", allowed = new[] { "pending", "confirmed", "failed", "rejected", "cancelled" } });

        var payments = await paymentQueryService.Handle(new GetPaymentsByStatusQuery(paymentStatus), cancellationToken);
        return Ok(payments.Select(PaymentResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpPost]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageDocuments)]
    [ProducesResponseType(typeof(PaymentResource), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterPayment(RegisterPaymentResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var command = RegisterPaymentCommandFromResourceAssembler.ToCommandFromResource(resource);
            var payment = await paymentCommandService.CreateAsync(command, cancellationToken);
            return CreatedAtAction(nameof(GetPaymentById), new { id = payment.Id }, PaymentResourceFromEntityAssembler.ToResourceFromEntity(payment));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:int}")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageDocuments)]
    [ProducesResponseType(typeof(PaymentResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePayment(int id, UpdatePaymentResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var command = UpdatePaymentCommandFromResourceAssembler.ToCommandFromResource(id, resource);
            var payment = await paymentCommandService.UpdateAsync(command, cancellationToken);
            return payment is null ? NotFound() : Ok(PaymentResourceFromEntityAssembler.ToResourceFromEntity(payment));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageDocuments)]
    [ProducesResponseType(typeof(PaymentResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> ReplacePayment(int id, UpdatePaymentResource resource, CancellationToken cancellationToken)
    {
        return UpdatePayment(id, resource, cancellationToken);
    }

    [HttpPost("{id:int}/confirmations")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageDocuments)]
    [ProducesResponseType(typeof(PaymentResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateConfirmation(int id, CancellationToken cancellationToken) =>
        await ChangeStatus(() => paymentCommandService.ConfirmAsync(new ConfirmPaymentCommand(id), cancellationToken));

    [HttpPost("{id:int}/rejections")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageDocuments)]
    [ProducesResponseType(typeof(PaymentResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateRejection(int id, RejectPaymentResource? resource, CancellationToken cancellationToken) =>
        await ChangeStatus(() => paymentCommandService.RejectAsync(new RejectPaymentCommand(id, resource?.Reason), cancellationToken));

    [HttpPost("{id:int}/cancellations")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageDocuments)]
    [ProducesResponseType(typeof(PaymentResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateCancellation(int id, CancellationToken cancellationToken) =>
        await ChangeStatus(() => paymentCommandService.CancelAsync(new CancelPaymentCommand(id), cancellationToken));

    [HttpDelete("{id:int}")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageDocuments)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePayment(int id, CancellationToken cancellationToken)
    {
        var payment = await paymentCommandService.CancelAsync(new CancelPaymentCommand(id), cancellationToken);
        return payment is null ? NotFound() : NoContent();
    }

    private async Task<IActionResult> ChangeStatus(Func<Task<Payment?>> action)
    {
        try
        {
            var payment = await action();
            return payment is null ? NotFound() : Ok(PaymentResourceFromEntityAssembler.ToResourceFromEntity(payment));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

}

public record RejectPaymentResource(string? Reason);

file static class PaymentControllerQuery
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

