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
[Route("api/v1/credit-requests")]
public class CreditRequestsController(ICreditRequestQueryService queries, ICreditRequestCommandService commands) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CreditRequestResource>>> GetAll(CancellationToken cancellationToken) =>
        Ok((await queries.ListAsync(cancellationToken)).Select(CreditRequestResourceAssembler.ToResource));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CreditRequestResource>> GetById(int id, CancellationToken cancellationToken)
    {
        var entity = await queries.FindAsync(id, cancellationToken);
        return entity is null ? NotFound() : Ok(CreditRequestResourceAssembler.ToResource(entity));
    }

    [HttpPost]
    public async Task<ActionResult<CreditRequestResource>> Create(CreateCreditRequestResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await commands.CreateAsync(CreditRequestResourceAssembler.ToEntity(resource), cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, CreditRequestResourceAssembler.ToResource(entity));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPost("{id:int}/resolutions")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanAcceptPurchaseRequest)]
    public async Task<ActionResult<CreditRequestResource>> Resolve(int id, ResolveCreditRequestResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await commands.ResolveAsync(id, resource.Status, resource.ReviewedBy, resource.Note, cancellationToken);
            return entity is null ? NotFound() : Ok(CreditRequestResourceAssembler.ToResource(entity));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageWorkspace)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken) =>
        await commands.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
}

