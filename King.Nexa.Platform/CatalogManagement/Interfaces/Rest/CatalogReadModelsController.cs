using King.Nexa.Platform.Shared.Application.Pagination;
using King.Nexa.Platform.Shared.Application.ReadModels;
using King.Nexa.Platform.Shared.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace King.Nexa.Platform.CatalogManagement.Interfaces.Rest;

[ApiController]
[Authorize(Policy = NexaAuthorizationPolicies.WorkspaceMember)]
[Route("api/v1/catalog")]
public class CatalogReadModelsController(IWorkspaceReadModelQueryService readModels) : ControllerBase
{
    [HttpGet("promotional-catalog")]
    public async Task<IActionResult> GetPromotionalCatalog([FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken cancellationToken)
    {
        var catalog = await readModels.GetPromotionalCatalogAsync(new PaginationRequest(page, pageSize), cancellationToken);
        return Ok(catalog);
    }
}
