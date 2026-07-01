using King.Nexa.Platform.Sales.Application.CommandServices;
using King.Nexa.Platform.Sales.Application.QueryServices;
using King.Nexa.Platform.Sales.Domain.Model.Aggregates;
using King.Nexa.Platform.Sales.Domain.Model.Commands;
using King.Nexa.Platform.Sales.Domain.Model.Queries;
using King.Nexa.Platform.Sales.Domain.Model.Entities;
using King.Nexa.Platform.Sales.Domain.Model.ValueObjects;
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
    IPurchaseRequestCommandService commandService) : ControllerBase
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
        [FromQuery] DateOnly? createdFrom,
        [FromQuery] DateOnly? createdTo,
        CancellationToken cancellationToken)
    {
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

    [HttpGet("/api/v1/commercial/purchase-requests")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanAcceptPurchaseRequest)]
    [ProducesResponseType(typeof(IEnumerable<PurchaseRequestResource>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PurchaseRequestResource>>> GetCommercialInbox(CancellationToken cancellationToken)
    {
        var requests = await queryService.ListCommercialInboxAsync(cancellationToken);
        return Ok(requests.Select(PurchaseRequestResourceAssembler.ToResourceFromEntity));
    }

    [HttpPost]
    [ProducesResponseType(typeof(PurchaseRequestResource), StatusCodes.Status201Created)]
    public async Task<ActionResult<PurchaseRequestResource>> Create([FromBody] UpsertPurchaseRequestResource resource, CancellationToken cancellationToken)
    {
        var request = await commandService.CreateAsync(PurchaseRequestResourceAssembler.ToEntityFromResource(resource), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = request.Id }, PurchaseRequestResourceAssembler.ToResourceFromEntity(request));
    }

    [HttpPost("/api/v1/manual-purchase-requests")]
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

[ApiController]
[Authorize(Policy = NexaAuthorizationPolicies.WorkspaceMember)]
[Route("api/v1/purchase-request-lines")]
public class PurchaseRequestLinesController(
    IPurchaseRequestQueryService queryService,
    IPurchaseRequestCommandService commandService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PurchaseRequestLineResource>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PurchaseRequestLineResource>>> GetAll(CancellationToken cancellationToken)
    {
        var lines = await queryService.ListLinesAsync(cancellationToken);
        return Ok(lines.Select(PurchaseRequestResourceAssembler.ToResourceFromEntity));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PurchaseRequestLineResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PurchaseRequestLineResource>> GetById(int id, CancellationToken cancellationToken)
    {
        var line = await queryService.GetLineByIdAsync(id, cancellationToken);
        return line is null ? NotFound() : Ok(PurchaseRequestResourceAssembler.ToResourceFromEntity(line));
    }

    [HttpPost]
    [ProducesResponseType(typeof(PurchaseRequestLineResource), StatusCodes.Status201Created)]
    public async Task<ActionResult<PurchaseRequestLineResource>> Create([FromBody] UpsertPurchaseRequestLineResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var line = await commandService.CreateLineAsync(PurchaseRequestResourceAssembler.ToEntityFromResource(resource), cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = line.Id }, PurchaseRequestResourceAssembler.ToResourceFromEntity(line));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(PurchaseRequestLineResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PurchaseRequestLineResource>> Update(int id, [FromBody] UpsertPurchaseRequestLineResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var line = await commandService.UpdateLineAsync(id, PurchaseRequestResourceAssembler.ToEntityFromResource(resource), cancellationToken);
            return line is null ? NotFound() : Ok(PurchaseRequestResourceAssembler.ToResourceFromEntity(line));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await commandService.DeleteLineAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}

[ApiController]
[Authorize(Policy = NexaAuthorizationPolicies.WorkspaceMember)]
[Route("api/v1/promotions")]
public class PromotionsController(
    IPromotionQueryService queryService,
    IPromotionCommandService commandService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PromotionResource>>> GetAll(CancellationToken cancellationToken)
    {
        var promotions = await queryService.ListAsync(cancellationToken);
        return Ok(promotions.Select(PromotionResourceAssembler.ToResourceFromSnapshot));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PromotionResource>> GetById(int id, CancellationToken cancellationToken)
    {
        var promotion = await queryService.GetByIdAsync(id, cancellationToken);
        return promotion is null ? NotFound() : Ok(PromotionResourceAssembler.ToResourceFromSnapshot(promotion));
    }

    [HttpPost]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManagePromotions)]
    public async Task<ActionResult<PromotionResource>> Create([FromBody] UpsertPromotionResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var promotion = await commandService.CreateAsync(PromotionResourceAssembler.ToDraftFromResource(resource), cancellationToken);
            var snapshot = await queryService.GetByIdAsync(promotion.Id, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = promotion.Id }, PromotionResourceAssembler.ToResourceFromSnapshot(snapshot!));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:int}")]
    [HttpPut("{id:int}")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManagePromotions)]
    public async Task<ActionResult<PromotionResource>> Update(int id, [FromBody] UpsertPromotionResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var promotion = await commandService.UpdateAsync(id, PromotionResourceAssembler.ToDraftFromResource(resource), cancellationToken);
            if (promotion is null) return NotFound();
            var snapshot = await queryService.GetByIdAsync(id, cancellationToken);
            return Ok(PromotionResourceAssembler.ToResourceFromSnapshot(snapshot!));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}/activate")]
    [HttpPost("{id:int}/activations")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManagePromotions)]
    public Task<ActionResult<PromotionResource>> Activate(int id, CancellationToken cancellationToken) =>
        ChangeStatus(id, "active", cancellationToken);

    [HttpPut("{id:int}/pause")]
    [HttpPost("{id:int}/deactivations")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManagePromotions)]
    public Task<ActionResult<PromotionResource>> Pause(int id, CancellationToken cancellationToken) =>
        ChangeStatus(id, "paused", cancellationToken);

    [HttpPut("{id:int}/archive")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManagePromotions)]
    public Task<ActionResult<PromotionResource>> Archive(int id, CancellationToken cancellationToken) =>
        ChangeStatus(id, "archived", cancellationToken);

    [HttpDelete("{id:int}")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManagePromotions)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await commandService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    private async Task<ActionResult<PromotionResource>> ChangeStatus(int id, string status, CancellationToken cancellationToken)
    {
        try
        {
            var promotion = await commandService.ChangeStatusAsync(id, status, cancellationToken);
            if (promotion is null) return NotFound();
            var snapshot = await queryService.GetByIdAsync(id, cancellationToken);
            return Ok(PromotionResourceAssembler.ToResourceFromSnapshot(snapshot!));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

[ApiController]
[Authorize(Policy = NexaAuthorizationPolicies.WorkspaceMember)]
[Route("api/v1/conversation-messages")]
public class ConversationMessagesController(
    IPurchaseRequestQueryService queryService,
    IPurchaseRequestCommandService commandService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ConversationMessageResource>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ConversationMessageResource>>> GetAll(CancellationToken cancellationToken)
    {
        var messages = await queryService.ListMessagesAsync(cancellationToken);
        return Ok(messages.Select(PurchaseRequestResourceAssembler.ToResourceFromEntity));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ConversationMessageResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConversationMessageResource>> GetById(int id, CancellationToken cancellationToken)
    {
        var message = await queryService.GetMessageByIdAsync(id, cancellationToken);
        return message is null ? NotFound() : Ok(PurchaseRequestResourceAssembler.ToResourceFromEntity(message));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ConversationMessageResource), StatusCodes.Status201Created)]
    public async Task<ActionResult<ConversationMessageResource>> Create([FromBody] UpsertConversationMessageResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var message = await commandService.CreateMessageAsync(PurchaseRequestResourceAssembler.ToEntityFromResource(resource), cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = message.Id }, PurchaseRequestResourceAssembler.ToResourceFromEntity(message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ConversationMessageResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConversationMessageResource>> Update(int id, [FromBody] UpsertConversationMessageResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var message = await commandService.UpdateMessageAsync(id, PurchaseRequestResourceAssembler.ToEntityFromResource(resource), cancellationToken);
            return message is null ? NotFound() : Ok(PurchaseRequestResourceAssembler.ToResourceFromEntity(message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await commandService.DeleteMessageAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
