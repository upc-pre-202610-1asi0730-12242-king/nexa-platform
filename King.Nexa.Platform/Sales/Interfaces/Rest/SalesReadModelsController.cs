using King.Nexa.Platform.Shared.Application.Pagination;
using King.Nexa.Platform.Shared.Application.ReadModels;
using King.Nexa.Platform.Shared.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace King.Nexa.Platform.Sales.Interfaces.Rest;

[ApiController]
[Authorize(Policy = NexaAuthorizationPolicies.CanAcceptPurchaseRequest)]
[Route("api/v1/sales")]
public class SalesReadModelsController(IWorkspaceReadModelQueryService readModels) : ControllerBase
{
    [HttpGet("order-summaries")]
    public async Task<IActionResult> GetOrderSummaries([FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken cancellationToken)
    {
        var summaries = await readModels.GetSalesOrderSummariesAsync(new PaginationRequest(page, pageSize), cancellationToken);
        return Ok(summaries);
    }

    [HttpGet("purchase-request-inbox")]
    public async Task<IActionResult> GetPurchaseRequestInbox([FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken cancellationToken)
    {
        var inbox = await readModels.GetSalesPurchaseRequestInboxAsync(new PaginationRequest(page, pageSize), cancellationToken);
        return Ok(inbox);
    }
}
