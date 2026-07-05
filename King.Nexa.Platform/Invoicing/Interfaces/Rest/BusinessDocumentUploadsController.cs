using King.Nexa.Platform.Invoicing.Application.CommandServices;
using King.Nexa.Platform.Invoicing.Interfaces.Rest.Resources;
using King.Nexa.Platform.Invoicing.Interfaces.Rest.Transform;
using King.Nexa.Platform.Shared.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace King.Nexa.Platform.Invoicing.Interfaces.Rest;

[ApiController]
[Authorize(Policy = NexaAuthorizationPolicies.CanManageDocuments)]
[Route("api/v1/business-document-uploads")]
public class BusinessDocumentUploadsController(IBusinessDocumentCommandService commandService) : ControllerBase
{
    [HttpPost]
    [RequestSizeLimit(20_000_000)]
    [ProducesResponseType(typeof(BusinessDocumentResource), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<BusinessDocumentResource>> Create([FromForm] UploadBusinessDocumentResource resource, CancellationToken cancellationToken)
    {
        if (resource.File is null || resource.File.Length == 0) return BadRequest("Document file is required.");

        try
        {
            var document = await commandService.UploadAsync(new UploadBusinessDocumentCommand(
                resource.TenantId,
                resource.OrderId,
                resource.ClientAccountId,
                resource.Type,
                resource.Label,
                resource.VisibleToBuyer,
                resource.Required,
                resource.File), cancellationToken);
            return Created($"/api/v1/business-documents/{document.Id}", BusinessDocumentResourceFromEntityAssembler.ToResourceFromEntity(document));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
