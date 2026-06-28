using King.Nexa.Platform.CatalogManagement.Domain.Repositories;
using King.Nexa.Platform.Sales.Application.CommandServices;
using King.Nexa.Platform.Sales.Application.OutboundServices;
using King.Nexa.Platform.Sales.Domain.Model.Aggregates;
using King.Nexa.Platform.Sales.Domain.Model.Commands;
using King.Nexa.Platform.Sales.Domain.Model.Entities;
using King.Nexa.Platform.Sales.Domain.Model.ValueObjects;
using King.Nexa.Platform.Sales.Domain.Repositories;
using King.Nexa.Platform.Shared.Application.Security;
using King.Nexa.Platform.Shared.Domain.Repositories;
using King.Nexa.Platform.Warehouse.Application.CommandServices;
using CatalogItemIdentifier = King.Nexa.Platform.CatalogManagement.Domain.Model.ValueObjects.CatalogItemId;
using CatalogEntity = King.Nexa.Platform.CatalogManagement.Domain.Model.Aggregates.CatalogItem;

namespace King.Nexa.Platform.Sales.Application.Internal.CommandServices;

public class PurchaseRequestCommandService(
    IPurchaseRequestRepository purchaseRequests,
    IOrderRepository orders,
    IClientAccountRepository clientAccounts,
    ICatalogItemRepository catalogItems,
    IUnitOfWork unitOfWork,
    ICurrentWorkspaceContext workspaceContext,
    IInventoryOperationsCommandService inventoryOperations,
    IOrderFulfillmentHandoff fulfillmentHandoff,
    IOrderDocumentProvisioner documentProvisioner) : IPurchaseRequestCommandService
{
    public async Task<PurchaseRequest> CreateAsync(PurchaseRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = CurrentTenantId();
        await EnsureClientIsAccessibleAsync(request.ClientAccountId, tenantId, cancellationToken);
        if (request.Status is not ("draft" or "submitted"))
            throw new InvalidOperationException("A purchase request must start as draft or submitted.");
        request.TenantId = tenantId;
        request.ValidateStructuredFields();
        await purchaseRequests.AddAsync(request, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return request;
    }

    public async Task<PurchaseRequest?> UpdateAsync(int id, PurchaseRequest draft, CancellationToken cancellationToken = default)
    {
        var request = await FindRequestAsync(id, cancellationToken);
        if (request is null) return null;

        await EnsureClientIsAccessibleAsync(request.ClientAccountId, request.TenantId, cancellationToken);
        request.Code = draft.Code;
        request.Origin = draft.Origin;
        request.Priority = draft.Priority;
        request.RequestedDeliveryDate = draft.RequestedDeliveryDate;
        request.DeliveryAddress = draft.DeliveryAddress;
        request.DeliveryDistrict = draft.DeliveryDistrict;
        request.DeliveryCity = draft.DeliveryCity;
        request.DeliveryProvince = draft.DeliveryProvince;
        request.DeliveryReference = draft.DeliveryReference;
        request.PaymentOption = draft.PaymentOption;
        request.ShippingEstimate = draft.ShippingEstimate;
        request.Comments = draft.Comments;
        request.ValidateStructuredFields();
        request.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.CompleteAsync(cancellationToken);
        return request;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var request = await FindRequestAsync(id, cancellationToken);
        if (request is null) return false;
        purchaseRequests.Remove(request);
        await unitOfWork.CompleteAsync(cancellationToken);
        return true;
    }

    public Task<PurchaseRequest?> SubmitAsync(int id, string? note, CancellationToken cancellationToken = default) =>
        ChangeStatusAsync(id, "submitted", note, null, cancellationToken);

    public Task<PurchaseRequest?> RequestAdjustmentAsync(int id, string? note, CancellationToken cancellationToken = default) =>
        ChangeStatusAsync(id, "buyer_adjustment_requested", note, null, cancellationToken);

    public Task<PurchaseRequest?> RejectAsync(int id, string? note, CancellationToken cancellationToken = default) =>
        ChangeStatusAsync(id, "rejected", note, null, cancellationToken);

    public Task<PurchaseRequest?> ValidateCommerciallyAsync(int id, string? commercialOwner, string? comments, CancellationToken cancellationToken = default) =>
        ChangeStatusAsync(id, "commercially_validated", comments, commercialOwner, cancellationToken);

    public Task<PurchaseRequest?> CancelAsync(int id, string? note, CancellationToken cancellationToken = default) =>
        ChangeStatusAsync(id, "cancelled", note, null, cancellationToken);

    public async Task<OrderAcceptanceResult?> AcceptIntoOrderAsync(int id, string? note, CancellationToken cancellationToken = default)
    {
        var request = await FindRequestAsync(id, cancellationToken);
        if (request is null) return null;

        var client = await clientAccounts.FindByIdAsync(request.ClientAccountId, cancellationToken);
        if (client is null) throw new InvalidOperationException("Purchase request client does not exist.");

        var lines = await purchaseRequests.ListLinesByRequestIdAsync(request.Id, cancellationToken);
        if (lines.Count == 0) throw new InvalidOperationException("Purchase request has no lines.");

        var orderNumber = OrderNumberFor(request);
        var existingOrder = await orders.FindByOrderNumberAsync(new OrderNumber(orderNumber), cancellationToken);
        if (existingOrder is not null)
            return new OrderAcceptanceResult(request.Id, existingOrder.Id, null, "already_accepted");

        var catalogById = await LoadCatalogItemsAsync(lines, cancellationToken);
        EnsureCatalogStock(lines, catalogById);
        var order = new Order(new CreateOrderCommand(
            new OrderNumber(orderNumber),
            new CustomerId(client.Code),
            lines.Select(line => ToOrderItemCommand(line, catalogById[line.CatalogItemId])).ToList(),
            PriorityForOrder(request.Priority),
            string.IsNullOrWhiteSpace(note) ? request.Comments : note));
        order.AssignTenant(request.TenantId);
        order.AssignClientAccount(client.Id);

        await orders.AddAsync(order, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        await ReserveInventoryForOrderAsync(order, cancellationToken);
        await ReserveCatalogStockAsync(order.Items, cancellationToken);
        var handoff = await fulfillmentHandoff.HandoffAsync(order, cancellationToken);

        request.MarkAcceptedIntoOrder(order.OrderNumber.Value, note);
        await unitOfWork.CompleteAsync(cancellationToken);

        await documentProvisioner.ProvisionAcceptedOrderAsync(order, client.Id, cancellationToken);
        await purchaseRequests.AddMessageAsync(new ConversationMessage
        {
            TenantId = request.TenantId,
            ClientAccountId = client.Id,
            PurchaseRequestId = request.Id,
            OrderId = order.Id,
            SenderRole = "sales",
            SenderName = string.IsNullOrWhiteSpace(request.CommercialOwner) ? "Sales" : request.CommercialOwner,
            Body = $"Sales accepted request {request.Code}, reserved live stock and created traceable order {order.OrderNumber.Value}. Logistics dispatch {handoff.DispatchOrderId} is ready for operations.",
            VisibleToBuyer = true
        }, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        return new OrderAcceptanceResult(request.Id, order.Id, handoff.DispatchOrderId, "accepted");
    }

    public async Task<ConversationMessage?> CreateMessageAsync(int id, PurchaseRequestMessageDraft draft, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(draft.Body)) throw new ArgumentException("Message body is required.", nameof(draft));
        var request = await FindRequestAsync(id, cancellationToken);
        if (request is null) return null;

        var message = new ConversationMessage
        {
            TenantId = request.TenantId,
            ClientAccountId = request.ClientAccountId,
            PurchaseRequestId = request.Id,
            SenderRole = workspaceContext.ClientAccountId.HasValue
                ? "buyer"
                : string.IsNullOrWhiteSpace(draft.SenderRole) ? "commercial" : draft.SenderRole.Trim().ToLowerInvariant(),
            SenderName = draft.SenderName?.Trim() ?? string.Empty,
            Body = draft.Body.Trim(),
            VisibleToBuyer = workspaceContext.ClientAccountId.HasValue || draft.VisibleToBuyer is not false
        };

        await purchaseRequests.AddMessageAsync(message, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return message;
    }

    public async Task<ConversationMessage> CreateMessageAsync(ConversationMessage draft, CancellationToken cancellationToken = default)
    {
        var tenantId = CurrentTenantId();
        if (string.IsNullOrWhiteSpace(draft.Body)) throw new ArgumentException("Message body is required.", nameof(draft));

        draft.TenantId = tenantId;
        draft.ClientAccountId = await ResolveAccessibleClientIdAsync(draft.ClientAccountId, tenantId, cancellationToken);
        draft.SenderRole = workspaceContext.ClientAccountId.HasValue
            ? "buyer"
            : string.IsNullOrWhiteSpace(draft.SenderRole) ? "commercial" : draft.SenderRole.Trim().ToLowerInvariant();
        if (workspaceContext.ClientAccountId.HasValue) draft.VisibleToBuyer = true;
        draft.SenderName = draft.SenderName?.Trim() ?? string.Empty;
        draft.Body = draft.Body.Trim();
        await purchaseRequests.AddMessageAsync(draft, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return draft;
    }

    public async Task<ConversationMessage?> UpdateMessageAsync(int id, ConversationMessage draft, CancellationToken cancellationToken = default)
    {
        var message = await FindMessageAsync(id, cancellationToken);
        if (message is null) return null;
        if (string.IsNullOrWhiteSpace(draft.Body)) throw new ArgumentException("Message body is required.", nameof(draft));

        message.ClientAccountId = await ResolveAccessibleClientIdAsync(
            draft.ClientAccountId ?? message.ClientAccountId,
            message.TenantId,
            cancellationToken);
        message.PurchaseRequestId = draft.PurchaseRequestId;
        message.OrderId = draft.OrderId;
        message.SenderRole = workspaceContext.ClientAccountId.HasValue
            ? "buyer"
            : string.IsNullOrWhiteSpace(draft.SenderRole) ? message.SenderRole : draft.SenderRole.Trim().ToLowerInvariant();
        message.SenderName = draft.SenderName?.Trim() ?? string.Empty;
        message.Body = draft.Body.Trim();
        message.VisibleToBuyer = workspaceContext.ClientAccountId.HasValue || draft.VisibleToBuyer;
        message.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.CompleteAsync(cancellationToken);
        return message;
    }

    public async Task<bool> DeleteMessageAsync(int id, CancellationToken cancellationToken = default)
    {
        var message = await FindMessageAsync(id, cancellationToken);
        if (message is null) return false;
        purchaseRequests.RemoveMessage(message);
        await unitOfWork.CompleteAsync(cancellationToken);
        return true;
    }

    public async Task<PurchaseRequestLine> CreateLineAsync(PurchaseRequestLine line, CancellationToken cancellationToken = default)
    {
        var tenantId = CurrentTenantId();
        await EnsureRequestBelongsToTenantAsync(line.PurchaseRequestId, tenantId, cancellationToken);
        if (line.CatalogItemId <= 0) throw new ArgumentException("Catalog item is required.", nameof(line));
        await EnsureCatalogItemBelongsToTenantAsync(line.CatalogItemId, tenantId, cancellationToken);
        if (line.Quantity <= 0) throw new ArgumentException("Quantity must be positive.", nameof(line));

        line.TenantId = tenantId;
        line.Unit = string.IsNullOrWhiteSpace(line.Unit) ? "UN" : line.Unit.Trim();
        line.Notes = line.Notes?.Trim() ?? string.Empty;
        await purchaseRequests.AddLineAsync(line, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return line;
    }

    public async Task<PurchaseRequestLine?> UpdateLineAsync(int id, PurchaseRequestLine draft, CancellationToken cancellationToken = default)
    {
        var line = await FindLineAsync(id, cancellationToken);
        if (line is null) return null;
        if (draft.Quantity <= 0) throw new ArgumentException("Quantity must be positive.", nameof(draft));
        await EnsureRequestBelongsToTenantAsync(draft.PurchaseRequestId, line.TenantId, cancellationToken);
        await EnsureCatalogItemBelongsToTenantAsync(draft.CatalogItemId, line.TenantId, cancellationToken);

        line.PurchaseRequestId = draft.PurchaseRequestId;
        line.CatalogItemId = draft.CatalogItemId;
        line.Quantity = draft.Quantity;
        line.Unit = string.IsNullOrWhiteSpace(draft.Unit) ? "UN" : draft.Unit.Trim();
        line.EstimatedWeightKg = draft.EstimatedWeightKg;
        line.Notes = draft.Notes?.Trim() ?? string.Empty;
        line.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.CompleteAsync(cancellationToken);
        return line;
    }

    public async Task<bool> DeleteLineAsync(int id, CancellationToken cancellationToken = default)
    {
        var line = await FindLineAsync(id, cancellationToken);
        if (line is null) return false;
        purchaseRequests.RemoveLine(line);
        await unitOfWork.CompleteAsync(cancellationToken);
        return true;
    }

    public async Task<PurchaseRequestReservationResult?> CreateReservationAsync(int id, PurchaseRequestReservationDraft draft, CancellationToken cancellationToken = default)
    {
        var request = await FindRequestAsync(id, cancellationToken);
        if (request is null) return null;
        if (request.Status is not ("commercially_validated" or "converted_to_order"))
            throw new InvalidOperationException("Only commercially validated purchase requests can reserve stock.");

        var externalId = string.IsNullOrWhiteSpace(draft.ExternalId)
            ? $"RES-PR-{request.Id:D4}"
            : draft.ExternalId.Trim();
        var reservation = await inventoryOperations.CreateReservationAsync(
            new InventoryReservationDraft(
                externalId,
                draft.InventoryItemId,
                draft.ProductId,
                draft.LotCode,
                null,
                request.Id,
                draft.Units),
            cancellationToken);
        return new PurchaseRequestReservationResult(reservation.Id, reservation.Code, reservation.Status);
    }

    private async Task<PurchaseRequest?> ChangeStatusAsync(
        int id,
        string status,
        string? note,
        string? commercialOwner,
        CancellationToken cancellationToken)
    {
        var request = await FindRequestAsync(id, cancellationToken);
        if (request is null) return null;

        request.ChangeStatus(status, note, commercialOwner);
        await unitOfWork.CompleteAsync(cancellationToken);
        return request;
    }

    private async Task<PurchaseRequest?> FindRequestAsync(int id, CancellationToken cancellationToken)
    {
        var tenantId = CurrentTenantId();
        return await purchaseRequests.FindByIdAsync(id, cancellationToken);
    }

    private async Task<PurchaseRequestLine?> FindLineAsync(int id, CancellationToken cancellationToken)
    {
        var tenantId = CurrentTenantId();
        return await purchaseRequests.FindLineByIdAsync(id, cancellationToken);
    }

    private async Task<ConversationMessage?> FindMessageAsync(int id, CancellationToken cancellationToken)
    {
        var tenantId = CurrentTenantId();
        return await purchaseRequests.FindMessageByIdAsync(id, cancellationToken);
    }

    private async Task EnsureRequestBelongsToTenantAsync(int requestId, int tenantId, CancellationToken cancellationToken)
    {
        var exists = await purchaseRequests.RequestBelongsToTenantAsync(tenantId, requestId, cancellationToken);
        if (!exists) throw new InvalidOperationException("Purchase request does not belong to the current tenant.");
    }

    private async Task EnsureCatalogItemBelongsToTenantAsync(int catalogItemId, int tenantId, CancellationToken cancellationToken)
    {
        var catalogItem = await catalogItems.FindByIdAsync(catalogItemId, cancellationToken);
        if (catalogItem is null || catalogItem.TenantId != tenantId)
            throw new InvalidOperationException("Catalog item does not belong to the current tenant.");
    }

    private async Task EnsureClientIsAccessibleAsync(int clientAccountId, int tenantId, CancellationToken cancellationToken)
    {
        _ = await ResolveAccessibleClientIdAsync(clientAccountId, tenantId, cancellationToken);
    }

    private async Task<int> ResolveAccessibleClientIdAsync(int? clientAccountId, int tenantId, CancellationToken cancellationToken)
    {
        if (workspaceContext.ClientAccountId is { } buyerClientAccountId)
        {
            if (clientAccountId.HasValue && clientAccountId.Value != buyerClientAccountId)
                throw new WorkspaceAccessDeniedException("The client account does not belong to the current buyer session.");
            clientAccountId = buyerClientAccountId;
        }

        if (clientAccountId is not > 0)
            throw new InvalidOperationException("Client account is required.");

        var client = await clientAccounts.FindByIdAsync(clientAccountId.Value, cancellationToken);
        if (client is null || client.TenantId != tenantId)
            throw new InvalidOperationException("Client account does not belong to the current tenant.");
        return clientAccountId.Value;
    }

    private async Task<Dictionary<int, CatalogEntity>> LoadCatalogItemsAsync(IEnumerable<PurchaseRequestLine> lines, CancellationToken cancellationToken)
    {
        var ids = lines.Select(line => line.CatalogItemId).Distinct().ToList();
        var tenantId = CurrentTenantId();
        var values = new Dictionary<int, CatalogEntity>();
        foreach (var id in ids)
        {
            var catalogItem = await catalogItems.FindByIdAsync(id, cancellationToken);
            if (catalogItem is not null && catalogItem.TenantId == tenantId)
                values[id] = catalogItem;
        }
        foreach (var id in ids.Where(id => !values.ContainsKey(id)))
            throw new InvalidOperationException($"Catalog item {id} does not exist.");
        return values;
    }

    private int CurrentTenantId() =>
        workspaceContext.TenantId ?? throw new InvalidOperationException("Current tenant is required.");

    private static void EnsureCatalogStock(IEnumerable<PurchaseRequestLine> lines, IReadOnlyDictionary<int, CatalogEntity> catalogById)
    {
        foreach (var line in lines)
        {
            var catalogItem = catalogById[line.CatalogItemId];
            var units = Math.Max(1, Convert.ToInt32(Math.Ceiling(line.Quantity)));
            if (catalogItem.AvailableStock.Value < units)
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
            var catalogItem = await catalogItems.FindByCatalogItemIdAsync(
                new CatalogItemIdentifier(item.CatalogItemId.Value),
                cancellationToken);
            if (catalogItem is null || catalogItem.TenantId != CurrentTenantId())
                throw new InvalidOperationException($"Catalog item {item.CatalogItemId.Value} does not belong to the current tenant.");
            catalogItem.ReserveStock(item.Quantity.Value);
            catalogItems.Update(catalogItem);
        }
        await unitOfWork.CompleteAsync(cancellationToken);
    }

    private static CreateOrderItemCommand ToOrderItemCommand(PurchaseRequestLine line, CatalogEntity catalogItem)
    {
        var quantity = Math.Max(1, Convert.ToInt32(Math.Ceiling(line.Quantity)));
        return new CreateOrderItemCommand(
            new ProductId(catalogItem.ProductId.Value),
            new CatalogItemId(catalogItem.CatalogItemId.Value),
            new ItemName(catalogItem.ItemName.Value),
            new Quantity(quantity),
            new Money(catalogItem.UnitPrice.Amount, catalogItem.UnitPrice.Currency));
    }

    private static string OrderNumberFor(PurchaseRequest request)
    {
        var suffix = request.Code.StartsWith("REQ-", StringComparison.OrdinalIgnoreCase)
            ? request.Code[4..]
            : $"{DateTime.UtcNow:yyyyMMdd}-{request.Id:D4}";
        return $"BUY-ORD-{suffix}";
    }

    private static string PriorityForOrder(string? priority) =>
        string.Equals(priority, "normal", StringComparison.OrdinalIgnoreCase) ? "medium" : priority ?? "medium";
}
