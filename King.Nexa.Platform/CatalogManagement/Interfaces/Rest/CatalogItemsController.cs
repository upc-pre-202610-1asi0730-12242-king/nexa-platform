using King.Nexa.Platform.CatalogManagement.Application.CommandServices;
using King.Nexa.Platform.CatalogManagement.Application.QueryServices;
using King.Nexa.Platform.CatalogManagement.Domain.Model.Commands;
using King.Nexa.Platform.CatalogManagement.Domain.Model.Queries;
using King.Nexa.Platform.CatalogManagement.Domain.Model.ValueObjects;
using King.Nexa.Platform.CatalogManagement.Interfaces.Rest.Resources;
using King.Nexa.Platform.CatalogManagement.Interfaces.Rest.Transform;
using King.Nexa.Platform.Sales.Application.QueryServices;
using King.Nexa.Platform.Shared.Application.Pagination;
using King.Nexa.Platform.Shared.Application.ReadModels;
using King.Nexa.Platform.Shared.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace King.Nexa.Platform.CatalogManagement.Interfaces.Rest;

[ApiController]
[Authorize(Policy = NexaAuthorizationPolicies.WorkspaceMember)]
[Route("api/v1/[controller]")]
public class CatalogItemsController(
    ICatalogItemCommandService catalogItemCommandService,
    ICatalogItemQueryService catalogItemQueryService,
    IPromotionQueryService promotionQueryService,
    IWorkspaceReadModelQueryService readModels) : ControllerBase
{
    /// <summary>
    /// Gets all catalog items.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllCatalogItems(
        [FromQuery] bool includePromotions,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] string? search,
        [FromQuery] string? brand,
        [FromQuery] string? category,
        [FromQuery] string? coldChain,
        [FromQuery] bool? active,
        [FromQuery] DateOnly? createdFrom,
        [FromQuery] DateOnly? createdTo,
        CancellationToken cancellationToken)
    {
        if (!includePromotions && HasCollectionQuery(page, pageSize, search, brand, category, coldChain, active, createdFrom, createdTo))
        {
            ColdChainRequirement? coldChainRequirement = null;
            if (!string.IsNullOrWhiteSpace(coldChain))
            {
                if (!Enum.TryParse<ColdChainRequirement>(coldChain, true, out var parsedRequirement))
                    return BadRequest(new { message = "Invalid cold-chain requirement." });
                coldChainRequirement = parsedRequirement;
            }

            var paged = await catalogItemQueryService.SearchAsync(
                new CatalogItemCollectionQuery(
                    new PaginationRequest(page, pageSize),
                    search,
                    brand,
                    category,
                    coldChainRequirement,
                    active,
                    createdFrom,
                    createdTo),
                cancellationToken);
            return Ok(paged.Map(CatalogItemResourceFromEntityAssembler.ToResourceFromEntity));
        }

        var catalogItems = await catalogItemQueryService.Handle(new GetAllCatalogItemsQuery(), cancellationToken);
        var resources = catalogItems.Select(CatalogItemResourceFromEntityAssembler.ToResourceFromEntity).ToList();
        if (!includePromotions) return Ok(resources);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var promotions = (await promotionQueryService.ListAsync(cancellationToken))
            .Where(snapshot => snapshot.Promotion.Status == "active")
            .Where(snapshot => !snapshot.Promotion.StartsOn.HasValue || snapshot.Promotion.StartsOn.Value <= today)
            .Where(snapshot => !snapshot.Promotion.EndsOn.HasValue || snapshot.Promotion.EndsOn.Value >= today)
            .OrderBy(snapshot => snapshot.Promotion.EndsOn)
            .ToArray();

        return Ok(resources.Select(item => new
        {
            Item = item,
            Promotions = promotions
                .Where(snapshot => snapshot.ProductIds.Contains(item.ProductId, StringComparer.OrdinalIgnoreCase))
                .Select(snapshot => new
                {
                    snapshot.Promotion.Id,
                    snapshot.Promotion.Code,
                    snapshot.Promotion.Name,
                    snapshot.Promotion.Campaign,
                    snapshot.Promotion.DiscountLabel,
                    snapshot.Promotion.StartsOn,
                    snapshot.Promotion.EndsOn,
                    snapshot.Promotion.Status
                })
        }));
    }

    /// <summary>
    /// Gets a catalog item by its numeric identifier.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCatalogItemById(int id, CancellationToken cancellationToken)
    {
        var catalogItem = await catalogItemQueryService.Handle(new GetCatalogItemByIdQuery(id), cancellationToken);
        return catalogItem is null ? NotFound() : Ok(CatalogItemResourceFromEntityAssembler.ToResourceFromEntity(catalogItem));
    }

    [HttpGet("{id:int}/availability")]
    public async Task<IActionResult> GetCatalogItemAvailability(int id, CancellationToken cancellationToken)
    {
        var availability = await readModels.GetCatalogItemAvailabilityAsync(id, cancellationToken);
        return availability is null ? NotFound() : Ok(availability);
    }

    /// <summary>
    /// Gets a catalog item by its stable catalog item identifier.
    /// </summary>
    [HttpGet("by-catalog-item/{catalogItemId}")]
    [Obsolete("Use GET /api/v1/catalog-items?catalogItemId={catalogItemId}.")]
    public async Task<IActionResult> GetCatalogItemByCatalogItemId(string catalogItemId, CancellationToken cancellationToken)
    {
        var catalogItem = await catalogItemQueryService.Handle(new GetCatalogItemByCatalogItemIdQuery(catalogItemId), cancellationToken);
        return catalogItem is null ? NotFound() : Ok(CatalogItemResourceFromEntityAssembler.ToResourceFromEntity(catalogItem));
    }

    /// <summary>
    /// Gets catalog items by category name.
    /// </summary>
    [HttpGet("by-category/{categoryName}")]
    [Obsolete("Use GET /api/v1/catalog-items?category={categoryName}.")]
    public async Task<IActionResult> GetCatalogItemsByCategoryName(string categoryName, CancellationToken cancellationToken)
    {
        var catalogItems = await catalogItemQueryService.Handle(new GetCatalogItemsByCategoryNameQuery(categoryName), cancellationToken);
        return Ok(catalogItems.Select(CatalogItemResourceFromEntityAssembler.ToResourceFromEntity));
    }

    /// <summary>
    /// Gets catalog items by brand name.
    /// </summary>
    [HttpGet("by-brand/{brandName}")]
    [Obsolete("Use GET /api/v1/catalog-items?brand={brandName}.")]
    public async Task<IActionResult> GetCatalogItemsByBrandName(string brandName, CancellationToken cancellationToken)
    {
        var catalogItems = await catalogItemQueryService.Handle(new GetCatalogItemsByBrandNameQuery(brandName), cancellationToken);
        return Ok(catalogItems.Select(CatalogItemResourceFromEntityAssembler.ToResourceFromEntity));
    }

    /// <summary>
    /// Gets catalog items by cold-chain requirement.
    /// </summary>
    [HttpGet("cold-chain/{requirement}")]
    public async Task<IActionResult> GetCatalogItemsByColdChainRequirement(string requirement, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ColdChainRequirement>(requirement, true, out var coldChainRequirement))
            return BadRequest(new { message = "Invalid cold-chain requirement." });

        var catalogItems = await catalogItemQueryService.Handle(new GetCatalogItemsByColdChainRequirementQuery(coldChainRequirement), cancellationToken);
        return Ok(catalogItems.Select(CatalogItemResourceFromEntityAssembler.ToResourceFromEntity));
    }

    /// <summary>
    /// Creates a catalog item for the Nexa product catalog.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageCatalog)]
    public async Task<IActionResult> CreateCatalogItem(CreateCatalogItemResource resource, CancellationToken cancellationToken)
    {
        var command = CreateCatalogItemCommandFromResourceAssembler.ToCommandFromResource(resource);
        var catalogItem = await catalogItemCommandService.CreateAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetCatalogItemById), new { id = catalogItem.Id }, CatalogItemResourceFromEntityAssembler.ToResourceFromEntity(catalogItem));
    }

    /// <summary>
    /// Updates an existing catalog item.
    /// </summary>
    [HttpPut("{id:int}")]
    [HttpPatch("{id:int}")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageCatalog)]
    public async Task<IActionResult> UpdateCatalogItem(int id, UpdateCatalogItemResource resource, CancellationToken cancellationToken)
    {
        var command = UpdateCatalogItemCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var catalogItem = await catalogItemCommandService.UpdateAsync(command, cancellationToken);
        return catalogItem is null ? NotFound() : Ok(CatalogItemResourceFromEntityAssembler.ToResourceFromEntity(catalogItem));
    }

    /// <summary>
    /// Deactivates an existing catalog item.
    /// </summary>
    [HttpDelete("{id:int}")]
    [HttpPost("{id:int}/deactivations")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageCatalog)]
    public async Task<IActionResult> DeleteCatalogItem(int id, CancellationToken cancellationToken)
    {
        var deleted = await catalogItemCommandService.DeleteAsync(new DeleteCatalogItemCommand(id), cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    private static bool HasCollectionQuery(int? page, int? pageSize, params object?[] filters) =>
        page.HasValue ||
        pageSize.HasValue ||
        filters.Any(filter => filter switch
        {
            null => false,
            string value => !string.IsNullOrWhiteSpace(value),
            _ => true
        });
}
