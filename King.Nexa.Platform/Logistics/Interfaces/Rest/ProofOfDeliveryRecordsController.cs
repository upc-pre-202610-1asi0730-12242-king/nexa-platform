using King.Nexa.Platform.Logistics.Application.CommandServices;
using King.Nexa.Platform.Logistics.Application.QueryServices;
using King.Nexa.Platform.Logistics.Interfaces.Rest.Resources;
using King.Nexa.Platform.Logistics.Interfaces.Rest.Transform;
using King.Nexa.Platform.Shared.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace King.Nexa.Platform.Logistics.Interfaces.Rest;

[ApiController]
[Authorize(Policy = NexaAuthorizationPolicies.WorkspaceMember)]
[Route("api/v1/proof-of-delivery-records")]
public class ProofOfDeliveryRecordsController(
    ILogisticsOperationalRecordQueryService queryService,
    ILogisticsOperationalRecordCommandService commandService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProofOfDeliveryRecordResource>>> GetAll(CancellationToken cancellationToken)
    {
        var proofs = await queryService.ListProofsOfDeliveryAsync(cancellationToken);
        return Ok(proofs.Select(DispatchOrderResourceAssembler.ToResourceFromEntity));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProofOfDeliveryRecordResource>> GetById(int id, CancellationToken cancellationToken)
    {
        var proof = await queryService.GetProofOfDeliveryByIdAsync(id, cancellationToken);
        return proof is null ? NotFound() : Ok(DispatchOrderResourceAssembler.ToResourceFromEntity(proof));
    }

    [HttpGet("/api/v1/orders/{orderId:int}/delivery-evidence")]
    public async Task<ActionResult<IEnumerable<ProofOfDeliveryRecordResource>>> GetOrderEvidence(int orderId, CancellationToken cancellationToken)
    {
        var evidence = await queryService.ListProofsOfDeliveryByOrderAsync(orderId, cancellationToken);
        return Ok(evidence.Select(DispatchOrderResourceAssembler.ToResourceFromEntity));
    }

    [HttpPost]
    [Authorize(Policy = NexaAuthorizationPolicies.CanStartDispatch)]
    public async Task<ActionResult<ProofOfDeliveryRecordResource>> Create([FromBody] UpsertProofOfDeliveryRecordResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var proof = await commandService.CreateProofOfDeliveryAsync(DispatchOrderResourceAssembler.ToEntityFromResource(resource), cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = proof.Id }, DispatchOrderResourceAssembler.ToResourceFromEntity(proof));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("/api/v1/dispatch-orders/{dispatchOrderId:int}/proofs-of-delivery")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanStartDispatch)]
    public async Task<ActionResult<ProofOfDeliveryRecordResource>> CreateForDispatch(int dispatchOrderId, [FromBody] CompletePodResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var proof = await commandService.CreateProofOfDeliveryForDispatchAsync(dispatchOrderId, resource.ReceivedBy, resource.CompletedAt, resource.PhotoReference, resource.SignatureReference, resource.Notes, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = proof.Id }, DispatchOrderResourceAssembler.ToResourceFromEntity(proof));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:int}/completions")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanStartDispatch)]
    public async Task<ActionResult<ProofOfDeliveryRecordResource>> CreateCompletion(int id, [FromBody] CompletePodResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var proof = await commandService.CompleteProofOfDeliveryAsync(id, resource.ReceivedBy, resource.CompletedAt, resource.PhotoReference, resource.SignatureReference, resource.Notes, cancellationToken);
            return proof is null ? NotFound() : Ok(DispatchOrderResourceAssembler.ToResourceFromEntity(proof));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPatch("{id:int}")]
    [HttpPut("{id:int}")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanStartDispatch)]
    public async Task<ActionResult<ProofOfDeliveryRecordResource>> Update(int id, [FromBody] UpsertProofOfDeliveryRecordResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var proof = await commandService.UpdateProofOfDeliveryAsync(id, DispatchOrderResourceAssembler.ToEntityFromResource(resource), cancellationToken);
            return proof is null ? NotFound() : Ok(DispatchOrderResourceAssembler.ToResourceFromEntity(proof));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanStartDispatch)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await commandService.DeleteProofOfDeliveryAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
