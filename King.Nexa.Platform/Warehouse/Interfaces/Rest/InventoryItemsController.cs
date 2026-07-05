using King.Nexa.Platform.Warehouse.Application.CommandServices;
using King.Nexa.Platform.Warehouse.Application.QueryServices;
using King.Nexa.Platform.Warehouse.Domain.Model.Commands;
using King.Nexa.Platform.Warehouse.Domain.Model.Queries;
using King.Nexa.Platform.Warehouse.Domain.Model.ValueObjects;
using King.Nexa.Platform.Warehouse.Interfaces.Rest.Resources;
using King.Nexa.Platform.Warehouse.Interfaces.Rest.Transform;
using King.Nexa.Platform.Shared.Application.Pagination;
using King.Nexa.Platform.Shared.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace King.Nexa.Platform.Warehouse.Interfaces.Rest;

[ApiController]
[Authorize(Policy = NexaAuthorizationPolicies.WorkspaceMember)]
[Route("api/v1/[controller]")]
public class InventoryItemsController(
    IInventoryItemCommandService inventoryItemCommandService,
    IInventoryItemQueryService inventoryItemQueryService) : ControllerBase
{
    /// <summary>
    /// Gets all inventory items.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllInventoryItems(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] string? search,
        [FromQuery] string? productId,
        [FromQuery] string? catalogItemId,
        [FromQuery] int? warehouseId,
        [FromQuery] string? warehouseLocation,
        [FromQuery] string? stockStatus,
        [FromQuery] int? threshold,
        CancellationToken cancellationToken)
    {
        var itemIdentifier = catalogItemId ?? productId;
        if (!string.IsNullOrWhiteSpace(itemIdentifier))
        {
            var item = await inventoryItemQueryService.Handle(new GetInventoryItemByCatalogItemIdQuery(itemIdentifier), cancellationToken);
            return item is null ? NotFound() : Ok(InventoryItemResourceFromEntityAssembler.ToResourceFromEntity(item));
        }

        if (!string.IsNullOrWhiteSpace(warehouseLocation))
        {
            var byLocation = await inventoryItemQueryService.Handle(new GetInventoryItemsByWarehouseLocationQuery(warehouseLocation), cancellationToken);
            return Ok(byLocation.Select(InventoryItemResourceFromEntityAssembler.ToResourceFromEntity));
        }

        if (string.Equals(stockStatus, "low", StringComparison.OrdinalIgnoreCase))
        {
            var lowStockItems = await inventoryItemQueryService.Handle(new GetLowStockInventoryItemsQuery(threshold ?? 10), cancellationToken);
            return Ok(lowStockItems.Select(InventoryItemResourceFromEntityAssembler.ToResourceFromEntity));
        }

        if (HasCollectionQuery(page, pageSize, search, productId, warehouseId, stockStatus, threshold))
        {
            var paged = await inventoryItemQueryService.SearchAsync(
                new InventoryItemCollectionQuery(
                    new PaginationRequest(page, pageSize),
                    search,
                    productId,
                    warehouseId),
                cancellationToken);
            return Ok(paged.Map(InventoryItemResourceFromEntityAssembler.ToResourceFromEntity));
        }

        var items = await inventoryItemQueryService.Handle(new GetAllInventoryItemsQuery(), cancellationToken);
        return Ok(items.Select(InventoryItemResourceFromEntityAssembler.ToResourceFromEntity));
    }

    /// <summary>
    /// Gets one inventory item by its numeric identifier.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetInventoryItemById(int id, CancellationToken cancellationToken)
    {
        var item = await inventoryItemQueryService.Handle(new GetInventoryItemByIdQuery(id), cancellationToken);
        return item is null ? NotFound() : Ok(InventoryItemResourceFromEntityAssembler.ToResourceFromEntity(item));
    }

    /// <summary>
    /// Creates an inventory item.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageInventory)]
    public async Task<IActionResult> CreateInventoryItem(CreateInventoryItemResource resource, CancellationToken cancellationToken)
    {
        var command = CreateInventoryItemCommandFromResourceAssembler.ToCommandFromResource(resource);
        var item = await inventoryItemCommandService.CreateAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetInventoryItemById), new { id = item.Id }, InventoryItemResourceFromEntityAssembler.ToResourceFromEntity(item));
    }

    /// <summary>
    /// Updates an inventory item.
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageInventory)]
    public async Task<IActionResult> UpdateInventoryItem(int id, UpdateInventoryItemResource resource, CancellationToken cancellationToken)
    {
        var command = UpdateInventoryItemCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var item = await inventoryItemCommandService.UpdateAsync(command, cancellationToken);
        return item is null ? NotFound() : Ok(InventoryItemResourceFromEntityAssembler.ToResourceFromEntity(item));
    }

    /// <summary>
    /// Deletes an inventory item.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanManageInventory)]
    public async Task<IActionResult> DeleteInventoryItem(int id, CancellationToken cancellationToken)
    {
        var deleted = await inventoryItemCommandService.DeleteAsync(new DeleteInventoryItemCommand(id), cancellationToken);
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
