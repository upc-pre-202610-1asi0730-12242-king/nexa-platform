using King.Nexa.Platform.Sales.Application.CommandServices;
using King.Nexa.Platform.Sales.Application.QueryServices;
using King.Nexa.Platform.Sales.Domain.Model.Commands;
using King.Nexa.Platform.Sales.Domain.Model.Queries;
using King.Nexa.Platform.Sales.Domain.Model.ValueObjects;
using King.Nexa.Platform.Sales.Interfaces.Rest.Resources;
using King.Nexa.Platform.Sales.Interfaces.Rest.Transform;
using King.Nexa.Platform.Shared.Application.Pagination;
using King.Nexa.Platform.Shared.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace King.Nexa.Platform.Sales.Interfaces.Rest;

[ApiController]
[Authorize(Policy = NexaAuthorizationPolicies.WorkspaceMember)]
[Route("api/v1/[controller]")]
public class OrdersController(IOrderCommandService orderCommandService, IOrderQueryService orderQueryService) : ControllerBase
{
    /// <summary>
    /// Gets all orders.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllOrders(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] string? status,
        [FromQuery] int? clientAccountId,
        [FromQuery] string? search,
        [FromQuery] DateOnly? createdFrom,
        [FromQuery] DateOnly? createdTo,
        [FromQuery] string? sort,
        CancellationToken cancellationToken)
    {
        if (HasCollectionQuery(page, pageSize, status, clientAccountId, search, createdFrom, createdTo, sort))
        {
            OrderStatus? orderStatus = null;
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (!Enum.TryParse<OrderStatus>(status, true, out var parsedStatus))
                    return BadRequest(new { message = "Invalid order status." });
                orderStatus = parsedStatus;
            }

            var paged = await orderQueryService.SearchAsync(
                new OrderCollectionQuery(
                    new PaginationRequest(page, pageSize),
                    orderStatus,
                    clientAccountId,
                    search,
                    createdFrom,
                    createdTo,
                    sort),
                cancellationToken);
            return Ok(paged.Map(OrderResourceFromEntityAssembler.ToResourceFromEntity));
        }

        var orders = await orderQueryService.Handle(new GetAllOrdersQuery(), cancellationToken);
        return Ok(orders.Select(OrderResourceFromEntityAssembler.ToResourceFromEntity));
    }

    /// <summary>
    /// Gets an order by its numeric identifier.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOrderById(int id, CancellationToken cancellationToken)
    {
        var order = await orderQueryService.Handle(new GetOrderByIdQuery(id), cancellationToken);
        return order is null ? NotFound() : Ok(OrderResourceFromEntityAssembler.ToResourceFromEntity(order));
    }

    /// <summary>
    /// Gets an order by its business order number.
    /// </summary>
    [HttpGet("by-order-number/{orderNumber}")]
    public async Task<IActionResult> GetOrderByOrderNumber(string orderNumber, CancellationToken cancellationToken)
    {
        var order = await orderQueryService.Handle(new GetOrderByOrderNumberQuery(orderNumber), cancellationToken);
        return order is null ? NotFound() : Ok(OrderResourceFromEntityAssembler.ToResourceFromEntity(order));
    }

    /// <summary>
    /// Gets orders by B2B customer identifier.
    /// </summary>
    [HttpGet("by-customer/{customerId}")]
    public async Task<IActionResult> GetOrdersByCustomerId(string customerId, CancellationToken cancellationToken)
    {
        var orders = await orderQueryService.Handle(new GetOrdersByCustomerIdQuery(customerId), cancellationToken);
        return Ok(orders.Select(OrderResourceFromEntityAssembler.ToResourceFromEntity));
    }

    /// <summary>
    /// Gets orders by order status.
    /// </summary>
    [HttpGet("by-status/{status}")]
    public async Task<IActionResult> GetOrdersByStatus(string status, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
            return BadRequest(new { message = "Invalid order status." });

        var orders = await orderQueryService.Handle(new GetOrdersByStatusQuery(orderStatus), cancellationToken);
        return Ok(orders.Select(OrderResourceFromEntityAssembler.ToResourceFromEntity));
    }

    /// <summary>
    /// Creates an order with at least one order item.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = NexaAuthorizationPolicies.CanCreateOrder)]
    public async Task<IActionResult> CreateOrder(CreateOrderResource resource, CancellationToken cancellationToken)
    {
        var command = CreateOrderCommandFromResourceAssembler.ToCommandFromResource(resource);
        var order = await orderCommandService.CreateAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, OrderResourceFromEntityAssembler.ToResourceFromEntity(order));
    }

    /// <summary>
    /// Updates a pending order.
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanCreateOrder)]
    public async Task<IActionResult> UpdateOrder(int id, UpdateOrderResource resource, CancellationToken cancellationToken)
    {
        var command = UpdateOrderCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var order = await orderCommandService.UpdateAsync(command, cancellationToken);
        return order is null ? NotFound() : Ok(OrderResourceFromEntityAssembler.ToResourceFromEntity(order));
    }

    /// <summary>
    /// Confirms an order after payment and inventory reservation are available.
    /// The legacy /confirm route is kept as a compatibility alias for /confirmations.
    /// </summary>
    [HttpPost("{id:int}/confirm")]
    [HttpPost("{id:int}/confirmations")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanCreateOrder)]
    public async Task<IActionResult> ConfirmOrder(int id, ConfirmOrderResource resource, CancellationToken cancellationToken)
    {
        var order = await orderCommandService.ConfirmAsync(
            new ConfirmOrderCommand(id, new PaymentConfirmation(resource.PaymentConfirmation), new InventoryReservation(resource.InventoryReservation)),
            cancellationToken);
        if (order is null) return NotFound();
        return Ok(OrderResourceFromEntityAssembler.ToResourceFromEntity(order));
    }

    /// <summary>
    /// Rejects an order with the business reason that prevents delivery.
    /// The legacy /reject route is kept as a compatibility alias for /rejections.
    /// </summary>
    [HttpPost("{id:int}/reject")]
    [HttpPost("{id:int}/rejections")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanCreateOrder)]
    public async Task<IActionResult> RejectOrder(int id, RejectOrderResource resource, CancellationToken cancellationToken)
    {
        var order = await orderCommandService.RejectAsync(new RejectOrderCommand(id, new RejectionReason(resource.RejectionReason)), cancellationToken);
        if (order is null) return NotFound();
        return Ok(OrderResourceFromEntityAssembler.ToResourceFromEntity(order));
    }

    /// <summary>
    /// Cancels an order that has not completed the sales workflow.
    /// The legacy /cancel route is kept as a compatibility alias for /cancellations.
    /// </summary>
    [HttpPost("{id:int}/cancel")]
    [HttpPost("{id:int}/cancellations")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanCreateOrder)]
    public async Task<IActionResult> CancelOrder(int id, CancellationToken cancellationToken)
    {
        var order = await orderCommandService.CancelAsync(new CancelOrderCommand(id), cancellationToken);
        if (order is null) return NotFound();
        return Ok(OrderResourceFromEntityAssembler.ToResourceFromEntity(order));
    }

    /// <summary>
    /// Cancels an order through the DELETE semantic.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = NexaAuthorizationPolicies.CanCreateOrder)]
    public async Task<IActionResult> DeleteOrder(int id, CancellationToken cancellationToken)
    {
        var order = await orderCommandService.CancelAsync(new CancelOrderCommand(id), cancellationToken);
        return order is null ? NotFound() : NoContent();
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
