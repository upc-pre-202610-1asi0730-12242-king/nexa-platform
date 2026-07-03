using King.Nexa.Platform.CatalogManagement.Domain.Model.ValueObjects;
using King.Nexa.Platform.CatalogManagement.Domain.Repositories;
using King.Nexa.Platform.Sales.Application.CommandServices;
using King.Nexa.Platform.Sales.Application.QueryServices;
using King.Nexa.Platform.Sales.Application.OutboundServices;
using King.Nexa.Platform.Sales.Domain.Model.Aggregates;
using King.Nexa.Platform.Sales.Domain.Model.Commands;
using King.Nexa.Platform.Sales.Domain.Model.Entities;
using King.Nexa.Platform.Sales.Domain.Repositories;
using King.Nexa.Platform.Shared.Application.Security;
using King.Nexa.Platform.Shared.Domain.Repositories;
using King.Nexa.Platform.Warehouse.Application.CommandServices;

namespace King.Nexa.Platform.Sales.Application.Internal.CommandServices;

public class OrderCommandService(
    IOrderRepository orderRepository,
    IClientAccountRepository clientAccountRepository,
    ICatalogItemRepository catalogItemRepository,
    IUnitOfWork unitOfWork,
    ICurrentWorkspaceContext workspaceContext,
    IInventoryOperationsCommandService inventoryOperations,
    IOrderFulfillmentHandoff fulfillmentHandoff,
    IOrderDocumentProvisioner documentProvisioner) : IOrderCommandService
{
    public async Task<Order> CreateAsync(CreateOrderCommand command, CancellationToken cancellationToken = default)
    {
        var client = await ResolveClientAsync(command.CustomerId, command.ClientAccountId, cancellationToken);
        await EnsureCatalogStockAsync(command.Items, cancellationToken);
        command = command with { OrderNumber = await ResolveUniqueOrderNumberAsync(command.OrderNumber, cancellationToken) };
        var order = new Order(command);
        order.AssignTenant(workspaceContext.TenantId
            ?? throw new InvalidOperationException("Current tenant is required to create orders."));
        order.AssignClientAccount(client.Id);
        await orderRepository.AddAsync(order, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        await ReserveInventoryForOrderAsync(order, cancellationToken);
        await ReserveCatalogStockAsync(order.Items, cancellationToken);
        await documentProvisioner.ProvisionAcceptedOrderAsync(order, client.Id, command.ShippingEstimate ?? 0, cancellationToken);
        await fulfillmentHandoff.HandoffAsync(order, cancellationToken);
        return order;
    }

    private async Task<Domain.Model.ValueObjects.OrderNumber> ResolveUniqueOrderNumberAsync(
        Domain.Model.ValueObjects.OrderNumber requested,
        CancellationToken cancellationToken)
    {
        if (!await orderRepository.ExistsByOrderNumberAsync(requested, cancellationToken)) return requested;

        var value = requested.Value;
        var separator = value.LastIndexOf('-');
        var suffix = separator >= 0 ? value[(separator + 1)..] : string.Empty;
        var prefix = separator >= 0 ? value[..separator] : value;
        var width = Math.Max(4, suffix.Length);
        var sequence = int.TryParse(suffix, out var parsed) ? parsed + 1 : 1;

        while (true)
        {
            var candidate = new Domain.Model.ValueObjects.OrderNumber($"{prefix}-{sequence.ToString().PadLeft(width, '0')}");
            if (!await orderRepository.ExistsByOrderNumberAsync(candidate, cancellationToken)) return candidate;
            sequence++;
        }
    }

    public async Task<Order?> UpdateAsync(UpdateOrderCommand command, CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.FindByIdWithItemsAsync(command.OrderId, cancellationToken);
        if (order is null) return null;

        var client = await ResolveClientAsync(command.CustomerId, command.ClientAccountId, cancellationToken);
        order.Update(command);
        order.AssignClientAccount(client.Id);
        orderRepository.Update(order);
        await unitOfWork.CompleteAsync(cancellationToken);
        return order;
    }

    private async Task<ClientAccount> ResolveClientAsync(
        Domain.Model.ValueObjects.CustomerId customerId,
        int? clientAccountId,
        CancellationToken cancellationToken)
    {
        var client = clientAccountId.HasValue
            ? await clientAccountRepository.FindByIdAsync(clientAccountId.Value, cancellationToken)
            : await clientAccountRepository.FindByCodeAsync(customerId.Value, cancellationToken);
        if (client is null || !client.Code.Equals(customerId.Value, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Order client account does not belong to the current workspace scope.");
        return client;
    }

    private async Task EnsureCatalogStockAsync(IEnumerable<CreateOrderItemCommand> items, CancellationToken cancellationToken)
    {
        foreach (var item in items)
        {
            var catalogItem = await catalogItemRepository.FindByCatalogItemIdAsync(
                new CatalogItemId(item.CatalogItemId.Value),
                cancellationToken);
            if (catalogItem is null || catalogItem.TenantId != workspaceContext.TenantId)
                throw new InvalidOperationException($"Catalog item {item.CatalogItemId.Value} does not belong to the current tenant.");
            if (catalogItem.AvailableStock.Value < item.Quantity.Value)
                throw new InvalidOperationException($"Insufficient catalog stock for {catalogItem.ItemName.Value}.");
        }
    }

    private async Task ReserveInventoryForOrderAsync(Order order, CancellationToken cancellationToken)
    {
        var index = 1;
        foreach (var item in order.Items)
        {
            await inventoryOperations.CreateReservationAsync(
                new InventoryReservationDraft(
                    $"RES-ORD-{order.Id:D4}-{index++:D2}",
                    null,
                    item.ProductId.Value,
                    null,
                    order.OrderNumber.Value,
                    null,
                    item.Quantity.Value),
                cancellationToken);
        }
    }

    private async Task ReserveCatalogStockAsync(IEnumerable<OrderItem> items, CancellationToken cancellationToken)
    {
        foreach (var item in items)
        {
            var catalogItem = await catalogItemRepository.FindByCatalogItemIdAsync(
                new CatalogItemId(item.CatalogItemId.Value),
                cancellationToken);
            if (catalogItem is null || catalogItem.TenantId != workspaceContext.TenantId)
                throw new InvalidOperationException($"Catalog item {item.CatalogItemId.Value} does not belong to the current tenant.");
            catalogItem.ReserveStock(item.Quantity.Value);
            catalogItemRepository.Update(catalogItem);
        }
        await unitOfWork.CompleteAsync(cancellationToken);
    }

    public async Task<Order?> ConfirmAsync(ConfirmOrderCommand command, CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.FindByIdAsync(command.OrderId, cancellationToken);
        if (order is null) return null;

        order.Confirm(command.PaymentConfirmation, command.InventoryReservation);
        orderRepository.Update(order);
        await unitOfWork.CompleteAsync(cancellationToken);
        return order;
    }

    public async Task<Order?> RejectAsync(RejectOrderCommand command, CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.FindByIdAsync(command.OrderId, cancellationToken);
        if (order is null) return null;

        order.Reject(command.RejectionReason);
        orderRepository.Update(order);
        await unitOfWork.CompleteAsync(cancellationToken);
        return order;
    }

    public async Task<Order?> CancelAsync(CancelOrderCommand command, CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.FindByIdAsync(command.OrderId, cancellationToken);
        if (order is null) return null;

        order.Cancel();
        orderRepository.Update(order);
        await unitOfWork.CompleteAsync(cancellationToken);
        return order;
    }
}
