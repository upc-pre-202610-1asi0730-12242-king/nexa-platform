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
