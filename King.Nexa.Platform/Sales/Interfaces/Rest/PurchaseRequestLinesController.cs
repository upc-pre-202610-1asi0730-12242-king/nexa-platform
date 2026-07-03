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
