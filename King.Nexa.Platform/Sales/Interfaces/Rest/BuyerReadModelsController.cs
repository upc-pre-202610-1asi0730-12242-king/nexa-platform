using King.Nexa.Platform.Shared.Application.ReadModels;
using King.Nexa.Platform.Shared.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace King.Nexa.Platform.Sales.Interfaces.Rest;

[ApiController]
[Authorize(Policy = NexaAuthorizationPolicies.WorkspaceMember)]
[Route("api/v1/buyer")]
public class BuyerReadModelsController(IWorkspaceReadModelQueryService readModels) : ControllerBase
{
    [HttpGet("dashboard-summary")]
    public async Task<IActionResult> GetDashboardSummary(CancellationToken cancellationToken)
    {
        var summary = await readModels.GetBuyerDashboardSummaryAsync(cancellationToken);
        return Ok(summary);
    }

    [HttpGet("orders/{id:int}/lifecycle")]
    public async Task<IActionResult> GetOrderLifecycle(int id, CancellationToken cancellationToken)
    {
        var lifecycle = await readModels.GetBuyerOrderLifecycleAsync(id, cancellationToken);
        return lifecycle is null ? NotFound() : Ok(lifecycle);
    }

    [HttpGet("financial-profile")]
    public async Task<IActionResult> GetFinancialProfile(CancellationToken cancellationToken)
    {
        var profile = await readModels.GetBuyerFinancialProfileAsync(cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }
}
