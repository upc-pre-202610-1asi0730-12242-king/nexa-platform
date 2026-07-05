using King.Nexa.Platform.Sales.Application.CommandServices;
using King.Nexa.Platform.Sales.Application.QueryServices;
using King.Nexa.Platform.Sales.Domain.Model.Queries;
using King.Nexa.Platform.Sales.Interfaces.Rest.Resources;
using King.Nexa.Platform.Sales.Interfaces.Rest.Transform;
using King.Nexa.Platform.Shared.Application.Pagination;
using King.Nexa.Platform.Shared.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace King.Nexa.Platform.Sales.Interfaces.Rest;

[ApiController]
[Authorize(Policy = NexaAuthorizationPolicies.WorkspaceMember)]
[Route("api/v1/purchase-requests")]
public class PurchaseRequestsController(
    IPurchaseRequestQueryService queryService,
    IPurchaseRequestCommandService commandService,
    IAuthorizationService authorizationService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PurchaseRequestResource>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] string? status,
        [FromQuery] int? clientAccountId,
        [FromQuery] string? priority,
        [FromQuery] string? search,
        [FromQuery] string? scope,
        [FromQuery] DateOnly? createdFrom,
        [FromQuery] DateOnly? createdTo,
        CancellationToken cancellationToken)
    {
        if (string.Equals(scope, "commercial", StringComparison.OrdinalIgnoreCase))
        {
            var authorization = await authorizationService.AuthorizeAsync(User, null, NexaAuthorizationPolicies.CanAcceptPurchaseRequest);
            if (!authorization.Succeeded)
                return Forbid();

            var commercialRequests = await queryService.ListCommercialInboxAsync(cancellationToken);
            return Ok(commercialRequests.Select(PurchaseRequestResourceAssembler.ToResourceFromEntity));
        }

        if (HasCollectionQuery(page, pageSize, status, clientAccountId, priority, search, createdFrom, createdTo))
        {
            var paged = await queryService.SearchAsync(
                new PurchaseRequestCollectionQuery(
                    new PaginationRequest(page, pageSize),
                    status,
                    clientAccountId,
                    priority,
                    search,
                    createdFrom,
                    createdTo),
                cancellationToken);
            return Ok(paged.Map(PurchaseRequestResourceAssembler.ToResourceFromEntity));
        }

        var requests = await queryService.ListAsync(cancellationToken);
        return Ok(requests.Select(PurchaseRequestResourceAssembler.ToResourceFromEntity));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PurchaseRequestResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PurchaseRequestResource>> GetById(int id, CancellationToken cancellationToken)
    {
        var request = await queryService.GetByIdAsync(id, cancellationToken);
        return request is null ? NotFound() : Ok(PurchaseRequestResourceAssembler.ToResourceFromEntity(request));
    }

    [HttpPost]
    [ProducesResponseType(typeof(PurchaseRequestResource), StatusCodes.Status201Created)]
    public async Task<ActionResult<PurchaseRequestResource>> Create([FromBody] UpsertPurchaseRequestResource resource, CancellationToken cancellationToken)
    {
        var request = await commandService.CreateAsync(PurchaseRequestResourceAssembler.ToEntityFromResource(resource), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = request.Id }, PurchaseRequestResourceAssembler.ToResourceFromEntity(request));
    }

    [HttpPost("manual-creations")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanAcceptPurchaseRequest)]
    [ProducesResponseType(typeof(PurchaseRequestResource), StatusCodes.Status201Created)]
    public async Task<ActionResult<PurchaseRequestResource>> CreateManualRequest([FromBody] UpsertPurchaseRequestResource resource, CancellationToken cancellationToken)
    {
        var request = PurchaseRequestResourceAssembler.ToEntityFromResource(resource);
        request.Origin = "manual";
        request.Status = string.IsNullOrWhiteSpace(request.Status) ? "submitted" : request.Status;
        var created = await commandService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, PurchaseRequestResourceAssembler.ToResourceFromEntity(created));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(PurchaseRequestResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PurchaseRequestResource>> Update(int id, [FromBody] UpsertPurchaseRequestResource resource, CancellationToken cancellationToken)
    {
        var request = await commandService.UpdateAsync(id, PurchaseRequestResourceAssembler.ToEntityFromResource(resource), cancellationToken);
        return request is null ? NotFound() : Ok(PurchaseRequestResourceAssembler.ToResourceFromEntity(request));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await commandService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("{id:int}/submissions")]
    [ProducesResponseType(typeof(PurchaseRequestResource), StatusCodes.Status200OK)]
    public async Task<ActionResult<PurchaseRequestResource>> Submit(int id, [FromBody] RequestNoteResource resource, CancellationToken cancellationToken)
    {
        var request = await commandService.SubmitAsync(id, resource.Note, cancellationToken);
        return request is null ? NotFound() : Ok(PurchaseRequestResourceAssembler.ToResourceFromEntity(request));
    }

    [HttpPost("{id:int}/adjustment-requests")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanAcceptPurchaseRequest)]
    [ProducesResponseType(typeof(PurchaseRequestResource), StatusCodes.Status200OK)]
    public async Task<ActionResult<PurchaseRequestResource>> RequestAdjustment(int id, [FromBody] RequestNoteResource resource, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(resource.Note)) return BadRequest("Adjustment reason is required.");
        var request = await commandService.RequestAdjustmentAsync(id, resource.Note, cancellationToken);
        return request is null ? NotFound() : Ok(PurchaseRequestResourceAssembler.ToResourceFromEntity(request));
    }

    [HttpPost("{id:int}/rejections")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanAcceptPurchaseRequest)]
    [ProducesResponseType(typeof(PurchaseRequestResource), StatusCodes.Status200OK)]
    public async Task<ActionResult<PurchaseRequestResource>> Reject(int id, [FromBody] RequestNoteResource resource, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(resource.Note)) return BadRequest("Rejection reason is required.");
        var request = await commandService.RejectAsync(id, resource.Note, cancellationToken);
        return request is null ? NotFound() : Ok(PurchaseRequestResourceAssembler.ToResourceFromEntity(request));
    }

    [HttpPost("{id:int}/commercial-validations")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanAcceptPurchaseRequest)]
    [ProducesResponseType(typeof(PurchaseRequestResource), StatusCodes.Status200OK)]
    public async Task<ActionResult<PurchaseRequestResource>> ValidateCommercially(int id, [FromBody] RequestOwnerResource resource, CancellationToken cancellationToken)
    {
        var request = await commandService.ValidateCommerciallyAsync(id, resource.CommercialOwner, resource.Comments, cancellationToken);
        return request is null ? NotFound() : Ok(PurchaseRequestResourceAssembler.ToResourceFromEntity(request));
    }

    [HttpPost("{id:int}/acceptances")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanAcceptPurchaseRequest)]
    public async Task<ActionResult<OrderAcceptanceResource>> CreateAcceptance(int id, [FromBody] RequestNoteResource resource, CancellationToken cancellationToken)
    {
        return await AcceptIntoOrderCore(id, resource, cancellationToken);
    }

    private async Task<ActionResult<OrderAcceptanceResource>> AcceptIntoOrderCore(int id, RequestNoteResource resource, CancellationToken cancellationToken)
    {
        var result = await commandService.AcceptIntoOrderAsync(id, resource.Note, cancellationToken);
        return result is null
            ? NotFound()
            : Ok(new OrderAcceptanceResource(result.PurchaseRequestId, result.OrderId, result.DispatchOrderId, result.Status));
    }

    [HttpPost("{id:int}/cancellations")]
    public async Task<ActionResult<PurchaseRequestResource>> Cancel(int id, [FromBody] RequestNoteResource resource, CancellationToken cancellationToken)
    {
        var request = await commandService.CancelAsync(id, resource.Note, cancellationToken);
        return request is null ? NotFound() : Ok(PurchaseRequestResourceAssembler.ToResourceFromEntity(request));
    }

    [HttpPost("{id:int}/buyer-responses")]
    [HttpPost("{id:int}/messages")]
    [ProducesResponseType(typeof(ConversationMessageResource), StatusCodes.Status201Created)]
    public async Task<ActionResult<ConversationMessageResource>> CreateMessage(int id, [FromBody] PurchaseRequestMessageResource resource, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(resource.Body)) return BadRequest("Message body is required.");

        var message = await commandService.CreateMessageAsync(
            id,
            new PurchaseRequestMessageDraft(resource.Body, resource.SenderRole, resource.SenderName, resource.VisibleToBuyer),
            cancellationToken);
        return message is null
            ? NotFound()
            : CreatedAtAction(nameof(GetById), new { id }, PurchaseRequestResourceAssembler.ToResourceFromEntity(message));
    }

    [HttpPost("{id:int}/reservations")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanAcceptPurchaseRequest)]
    [ProducesResponseType(typeof(ReservationResource), StatusCodes.Status201Created)]
    public async Task<ActionResult<ReservationResource>> CreateReservation(int id, [FromBody] ReservationRequestResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var result = await commandService.CreateReservationAsync(
                id,
                new PurchaseRequestReservationDraft(resource.Id, resource.InventoryItemId, resource.ProductId, resource.LotCode, resource.Units),
                cancellationToken);
            return result is null
                ? NotFound()
                : Created($"/api/v1/reservations/{result.Id}", new ReservationResource(result.Id, result.ExternalId, result.Status));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
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
