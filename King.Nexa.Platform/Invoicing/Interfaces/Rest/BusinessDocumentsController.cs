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
[Route("api/v1/business-documents")]
public class BusinessDocumentsController(
    IBusinessDocumentCommandService commandService,
    IBusinessDocumentQueryService queryService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BusinessDocumentResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<BusinessDocumentResource>>> GetAll(CancellationToken cancellationToken)
    {
        var documents = await queryService.ListAsync(cancellationToken);
        return Ok(documents.Select(BusinessDocumentResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BusinessDocumentResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BusinessDocumentResource>> GetById(int id, CancellationToken cancellationToken)
    {
        var document = await queryService.GetByIdAsync(id, cancellationToken);
        return document is null ? NotFound() : Ok(BusinessDocumentResourceFromEntityAssembler.ToResourceFromEntity(document));
    }

    [HttpGet("{id:int}/content")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetContent(int id, CancellationToken cancellationToken)
    {
        var content = await queryService.GetContentAsync(id, cancellationToken);
        if (content is null) return NotFound();
        if ((User.IsInRole("Buyer") || User.IsInRole("B2B Buyer")) && !content.VisibleToBuyer)
            return Forbid();
        return File(content.Content, content.ContentType, content.FileName);
    }

    [HttpPost]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageDocuments)]
    [ProducesResponseType(typeof(BusinessDocumentResource), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<BusinessDocumentResource>> Create([FromBody] CreateBusinessDocumentResource resource, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(resource.Type)) return BadRequest("Document type is required.");
        if (string.IsNullOrWhiteSpace(resource.Label)) return BadRequest("Document label is required.");
        try
        {
            var document = await commandService.CreateAsync(BusinessDocumentResourceFromEntityAssembler.ToEntityFromResource(resource), cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = document.Id }, BusinessDocumentResourceFromEntityAssembler.ToResourceFromEntity(document));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("generations")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageDocuments)]
    [ProducesResponseType(typeof(BusinessDocumentResource), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<BusinessDocumentResource>> Generate(
        [FromBody] GenerateBusinessDocumentResource resource,
        CancellationToken cancellationToken)
    {
        if (resource.OrderId <= 0) return BadRequest("Order is required.");
        if (string.IsNullOrWhiteSpace(resource.Type)) return BadRequest("Document type is required.");
        try
        {
            var document = await commandService.GenerateAsync(
                new GenerateBusinessDocumentCommand(resource.OrderId, resource.Type),
                cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = document.Id }, BusinessDocumentResourceFromEntityAssembler.ToResourceFromEntity(document));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:int}/status-changes")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageDocuments)]
    [ProducesResponseType(typeof(BusinessDocumentResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BusinessDocumentResource>> CreateStatusChange(int id, ChangeBusinessDocumentStatusResource resource, CancellationToken cancellationToken)
    {
        var document = await commandService.ChangeStatusAsync(
            new ChangeBusinessDocumentStatusCommand(id, resource.Status, resource.VisibleToBuyer),
            cancellationToken);
        return document is null ? NotFound() : Ok(BusinessDocumentResourceFromEntityAssembler.ToResourceFromEntity(document));
    }

}
