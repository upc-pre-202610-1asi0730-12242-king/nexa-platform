using King.Nexa.Platform.Sales.Application.CommandServices;
using King.Nexa.Platform.Sales.Application.QueryServices;
using King.Nexa.Platform.Sales.Interfaces.Rest.Resources;
using King.Nexa.Platform.Sales.Interfaces.Rest.Transform;
using King.Nexa.Platform.Shared.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace King.Nexa.Platform.Sales.Interfaces.Rest;

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
