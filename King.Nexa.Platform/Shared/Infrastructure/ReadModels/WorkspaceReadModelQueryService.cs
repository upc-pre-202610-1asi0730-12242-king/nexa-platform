using King.Nexa.Platform.CatalogManagement.Domain.Model.Aggregates;
using King.Nexa.Platform.Invoicing.Domain.Model.Aggregates;
using King.Nexa.Platform.Invoicing.Domain.Model.Entities;
using King.Nexa.Platform.Logistics.Domain.Model.Entities;
using King.Nexa.Platform.Sales.Domain.Model.Aggregates;
using King.Nexa.Platform.Sales.Domain.Model.Entities;
using King.Nexa.Platform.Shared.Application.Pagination;
using King.Nexa.Platform.Shared.Application.ReadModels;
using King.Nexa.Platform.Shared.Application.Security;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Queries;
using King.Nexa.Platform.Warehouse.Domain.Model.Aggregates;
using King.Nexa.Platform.Warehouse.Domain.Model.Entities;
using Microsoft.EntityFrameworkCore;
using WarehouseCatalogItemId = King.Nexa.Platform.Warehouse.Domain.Model.ValueObjects.CatalogItemId;

namespace King.Nexa.Platform.Shared.Infrastructure.ReadModels;

public class WorkspaceReadModelQueryService(AppDbContext context, ICurrentWorkspaceContext workspaceContext) : IWorkspaceReadModelQueryService
{
    public async Task<BuyerDashboardSummaryReadModel> GetBuyerDashboardSummaryAsync(CancellationToken cancellationToken = default)
    {
        var requests = Scoped(context.PurchaseRequests.AsNoTracking());
        var orders = Scoped(context.Orders.AsNoTracking());
        var documents = Scoped(context.BusinessDocuments.AsNoTracking());
        var invoices = Scoped(context.Invoices.AsNoTracking());
        var notifications = Scoped(context.NotificationRecords.AsNoTracking());

        if (workspaceContext.ClientAccountId is { } clientAccountId)
        {
            requests = requests.Where(row => row.ClientAccountId == clientAccountId);
            orders = orders.Where(row => row.ClientAccountId == clientAccountId);
            documents = documents.Where(row => row.ClientAccountId == clientAccountId);
            notifications = notifications.Where(row => row.ClientAccountId == clientAccountId);
            invoices = invoices.Where(row => context.Orders.Any(order =>
                order.TenantId == row.TenantId &&
                order.Id == row.OrderId &&
                order.ClientAccountId == clientAccountId));
        }

        var recentRequestsPage = await requests.OrderByDescending(row => row.CreatedAt)
            .ToPagedResultAsync(new PaginationRequest(1, 5), cancellationToken);
        var recentOrdersPage = await orders.Include(order => order.Items).OrderByDescending(row => row.CreatedAt)
            .ToPagedResultAsync(new PaginationRequest(1, 5), cancellationToken);

        var recentRequests = await MapPurchaseRequestInboxAsync(recentRequestsPage.Items, cancellationToken);
        var recentOrders = await MapOrderSummariesAsync(recentOrdersPage.Items, cancellationToken);

        return new BuyerDashboardSummaryReadModel(
            await requests.CountAsync(row => row.Status != "converted_to_order" && row.Status != "rejected" && row.Status != "cancelled", cancellationToken),
            await orders.CountAsync(row => row.Status.ToString() != "Cancelled" && row.Status.ToString() != "Rejected", cancellationToken),
            await documents.CountAsync(row => row.VisibleToBuyer && row.Status != "accepted", cancellationToken),
            await invoices.CountAsync(row => row.PaymentStatus.ToString() == "Pending", cancellationToken),
            recentRequests,
            recentOrders,
            await notifications.OrderByDescending(row => row.CreatedAt)
                .Take(5)
                .Select(row => new NotificationPreviewReadModel(row.Id, row.Title, row.Body, row.Read, row.CreatedAt))
                .ToListAsync(cancellationToken),
            await GetCreditSummaryForCurrentBuyerAsync(cancellationToken));
    }

    public async Task<OrderLifecycleReadModel?> GetBuyerOrderLifecycleAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var order = await Scoped(context.Orders.Include(row => row.Items).AsNoTracking())
            .FirstOrDefaultAsync(row => row.Id == orderId, cancellationToken);
        if (order is null) return null;

        var orderSummary = (await MapOrderSummariesAsync([order], cancellationToken)).Single();
        var dispatches = await Scoped(context.DispatchOrders.AsNoTracking())
            .Where(row => row.OrderId == order.Id)
            .OrderBy(row => row.CreatedAt)
            .ToListAsync(cancellationToken);
        var dispatchSummaries = new List<DispatchOrderSummaryReadModel>();
        foreach (var dispatch in dispatches)
        {
            var summary = await GetDispatchOrderSummaryAsync(dispatch.Id, cancellationToken);
            if (summary is not null) dispatchSummaries.Add(summary);
        }

        var dispatchIds = dispatches.Select(row => row.Id).ToArray();
        var documents = await Scoped(context.BusinessDocuments.AsNoTracking())
            .Where(row => row.OrderId == order.Id && row.VisibleToBuyer)
            .Select(row => new BusinessDocumentPreviewReadModel(row.Id, row.Type, row.Label, row.Status, row.VisibleToBuyer, row.Required))
            .ToListAsync(cancellationToken);
        var invoices = await Scoped(context.Invoices.AsNoTracking())
            .Where(row => row.OrderId == order.Id)
            .Select(row => new InvoicePreviewReadModel(row.Id, row.InvoiceNumber.Value, row.BillingAmount.Amount, row.BillingAmount.Currency, row.PaymentStatus.ToString()))
            .ToListAsync(cancellationToken);
        var payments = await Scoped(context.Payments.AsNoTracking())
            .Where(row => row.OrderId == order.Id)
            .Select(row => new PaymentPreviewReadModel(row.Id, row.ReferenceCode, row.BillingAmount.Amount, row.BillingAmount.Currency, row.Status.ToString()))
            .ToListAsync(cancellationToken);

        return new OrderLifecycleReadModel(
            orderSummary,
            order.Items.Select(ToOrderLine).ToList(),
            dispatchSummaries,
            await Scoped(context.DispatchEvents.AsNoTracking())
                .Where(row => dispatchIds.Contains(row.DispatchOrderId) && row.VisibleToBuyer)
                .OrderBy(row => row.CreatedAt)
                .Select(row => new DispatchEventTimelineReadModel(row.Id, row.Status, row.Description, row.VisibleToBuyer, row.CreatedAt))
                .ToListAsync(cancellationToken),
            await Scoped(context.TemperatureLogs.AsNoTracking())
                .Where(row => row.OrderId == order.Id || (row.DispatchOrderId.HasValue && dispatchIds.Contains(row.DispatchOrderId.Value)))
                .OrderByDescending(row => row.RecordedAt)
                .Select(row => new TemperatureReadingReadModel(row.Id, row.Celsius, row.Zone, row.Status, row.RecordedAt))
                .ToListAsync(cancellationToken),
            documents,
            invoices,
            payments);
    }

    public async Task<ClientFinancialProfileReadModel?> GetBuyerFinancialProfileAsync(CancellationToken cancellationToken = default)
    {
        if (workspaceContext.ClientAccountId is not { } clientAccountId) return null;
        return await GetClientFinancialProfileAsync(clientAccountId, cancellationToken);
    }

    public async Task<PagedResult<OrderSummaryReadModel>> GetSalesOrderSummariesAsync(PaginationRequest pagination, CancellationToken cancellationToken = default)
    {
        var page = await Scoped(context.Orders.Include(order => order.Items).AsNoTracking())
            .OrderByDescending(row => row.CreatedAt)
            .ToPagedResultAsync(pagination, cancellationToken);
        return new PagedResult<OrderSummaryReadModel>(
            await MapOrderSummariesAsync(page.Items, cancellationToken),
            page.Page,
            page.PageSize,
            page.TotalItems,
            page.TotalPages);
    }

    public async Task<PagedResult<PurchaseRequestInboxReadModel>> GetSalesPurchaseRequestInboxAsync(PaginationRequest pagination, CancellationToken cancellationToken = default)
    {
        var page = await Scoped(context.PurchaseRequests.AsNoTracking())
            .Where(row => row.Status == "submitted" || row.Status == "buyer_adjustment_requested" || row.Status == "commercially_validated")
            .OrderByDescending(row => row.UpdatedAt ?? row.CreatedAt)
            .ToPagedResultAsync(pagination, cancellationToken);
        return new PagedResult<PurchaseRequestInboxReadModel>(
            await MapPurchaseRequestInboxAsync(page.Items, cancellationToken),
            page.Page,
            page.PageSize,
            page.TotalItems,
            page.TotalPages);
    }

    public async Task<ClientFinancialProfileReadModel?> GetClientFinancialProfileAsync(int clientAccountId, CancellationToken cancellationToken = default)
    {
        var client = await Scoped(context.ClientAccounts.AsNoTracking())
            .FirstOrDefaultAsync(row => row.Id == clientAccountId, cancellationToken);
        if (client is null) return null;

        var pendingInvoices = await Scoped(context.Invoices.AsNoTracking())
            .Where(row => row.PaymentStatus.ToString() == "Pending")
            .Where(row => context.Orders.Any(order => order.TenantId == row.TenantId && order.Id == row.OrderId && order.ClientAccountId == client.Id))
            .OrderByDescending(row => row.CreatedAt)
            .Take(10)
            .Select(row => new InvoicePreviewReadModel(row.Id, row.InvoiceNumber.Value, row.BillingAmount.Amount, row.BillingAmount.Currency, row.PaymentStatus.ToString()))
            .ToListAsync(cancellationToken);

        var recentPayments = await Scoped(context.Payments.AsNoTracking())
            .Where(row => row.ClientAccountId == client.Id)
            .OrderByDescending(row => row.CreatedAt)
            .Take(10)
            .Select(row => new PaymentPreviewReadModel(row.Id, row.ReferenceCode, row.BillingAmount.Amount, row.BillingAmount.Currency, row.Status.ToString()))
            .ToListAsync(cancellationToken);

        return new ClientFinancialProfileReadModel(
            ToClientSummary(client),
            new CreditSummaryReadModel(client.MonthlyCreditLimit, client.MonthlyCreditUsed, client.MonthlyCreditAvailable, client.MonthlyCreditStatus, false),
            await Scoped(context.Orders.AsNoTracking()).CountAsync(row => row.ClientAccountId == client.Id && row.Status.ToString() == "Pending", cancellationToken),
            pendingInvoices,
            recentPayments,
            await Scoped(context.PaymentMethodRecords.AsNoTracking()).CountAsync(row => row.ClientAccountId == client.Id, cancellationToken),
            await Scoped(context.BusinessDocuments.AsNoTracking()).CountAsync(row => row.ClientAccountId == client.Id, cancellationToken));
    }

    public async Task<DispatchOrderSummaryReadModel?> GetDispatchOrderSummaryAsync(int dispatchOrderId, CancellationToken cancellationToken = default)
    {
        var dispatch = await Scoped(context.DispatchOrders.AsNoTracking())
            .FirstOrDefaultAsync(row => row.Id == dispatchOrderId, cancellationToken);
        if (dispatch is null) return null;

        var order = await Scoped(context.Orders.Include(row => row.Items).AsNoTracking())
            .FirstOrDefaultAsync(row => row.Id == dispatch.OrderId, cancellationToken);
        var client = await Scoped(context.ClientAccounts.AsNoTracking())
            .FirstOrDefaultAsync(row => row.Id == dispatch.ClientAccountId, cancellationToken);
        var events = await Scoped(context.DispatchEvents.AsNoTracking())
            .Where(row => row.DispatchOrderId == dispatch.Id)
            .OrderBy(row => row.CreatedAt)
            .Select(row => new DispatchEventTimelineReadModel(row.Id, row.Status, row.Description, row.VisibleToBuyer, row.CreatedAt))
            .ToListAsync(cancellationToken);
        var latestTemperature = await Scoped(context.TemperatureLogs.AsNoTracking())
            .Where(row => row.DispatchOrderId == dispatch.Id)
            .OrderByDescending(row => row.RecordedAt)
            .Select(row => new TemperatureReadingReadModel(row.Id, row.Celsius, row.Zone, row.Status, row.RecordedAt))
            .FirstOrDefaultAsync(cancellationToken);
        var podStatus = await Scoped(context.ProofOfDeliveryRecords.AsNoTracking())
            .Where(row => row.DispatchOrderId == dispatch.Id)
            .OrderByDescending(row => row.CompletedAt ?? row.CreatedAt)
            .Select(row => row.Status)
            .FirstOrDefaultAsync(cancellationToken);

        return new DispatchOrderSummaryReadModel(
            dispatch.Id,
            dispatch.Code,
            dispatch.Status,
            dispatch.RouteName,
            dispatch.Responsible,
            dispatch.Eta,
            dispatch.DeliveryWindow,
            order is null ? null : (await MapOrderSummariesAsync([order], cancellationToken)).Single(),
            client is null ? null : ToClientSummary(client),
            events.LastOrDefault(),
            events,
            podStatus,
            latestTemperature);
    }

    public async Task<OrderTimelineReadModel?> GetOrderTimelineAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var order = await Scoped(context.Orders.AsNoTracking()).FirstOrDefaultAsync(row => row.Id == orderId, cancellationToken);
        if (order is null) return null;

        var events = new List<TimelineEventReadModel>
        {
            new("order", order.Status.ToString(), $"Order {order.OrderNumber.Value} created.", order.CreatedAt)
        };
        events.AddRange(await Scoped(context.DispatchEvents.AsNoTracking())
            .Where(row => context.DispatchOrders.Any(dispatch => dispatch.Id == row.DispatchOrderId && dispatch.OrderId == order.Id))
            .Select(row => new TimelineEventReadModel("dispatch", row.Status, row.Description, row.CreatedAt))
            .ToListAsync(cancellationToken));
        events.AddRange(await Scoped(context.Invoices.AsNoTracking())
            .Where(row => row.OrderId == order.Id)
            .Select(row => new TimelineEventReadModel("invoice", row.PaymentStatus.ToString(), $"Invoice {row.InvoiceNumber.Value}.", row.CreatedAt))
            .ToListAsync(cancellationToken));
        events.AddRange(await Scoped(context.Payments.AsNoTracking())
            .Where(row => row.OrderId == order.Id)
            .Select(row => new TimelineEventReadModel("payment", row.Status.ToString(), $"Payment {row.ReferenceCode}.", row.CreatedAt))
            .ToListAsync(cancellationToken));

        return new OrderTimelineReadModel(order.Id, order.OrderNumber.Value, events.OrderBy(row => row.OccurredAt).ToList());
    }

    public async Task<CatalogItemAvailabilityReadModel?> GetCatalogItemAvailabilityAsync(int catalogItemId, CancellationToken cancellationToken = default)
    {
        var catalogItem = await Scoped(context.CatalogItems.AsNoTracking())
            .FirstOrDefaultAsync(row => row.Id == catalogItemId, cancellationToken);
        if (catalogItem is null) return null;

        var inventoryItem = await Scoped(context.InventoryItems.AsNoTracking())
            .FirstOrDefaultAsync(row => row.CatalogItemId == new WarehouseCatalogItemId(catalogItem.CatalogItemId.Value), cancellationToken);
        var lots = inventoryItem is null
            ? []
            : await Scoped(context.InventoryLots.AsNoTracking())
                .Where(row => row.InventoryItemId == inventoryItem.Id)
                .OrderBy(row => row.ExpirationDate)
                .Select(row => new LotSummaryReadModel(row.Id, row.LotCode, row.Quantity, row.ReservedQuantity, row.ExpirationDate, row.Status))
                .ToListAsync(cancellationToken);
        var lastMovementType = inventoryItem is null
            ? null
            : await Scoped(context.InventoryMovements.AsNoTracking())
                .Where(row => row.InventoryItemId == inventoryItem.Id)
                .OrderByDescending(row => row.OccurredAt)
                .Select(row => row.MovementType)
                .FirstOrDefaultAsync(cancellationToken);

        return new CatalogItemAvailabilityReadModel(
            catalogItem.Id,
            catalogItem.ProductId.Value,
            catalogItem.ItemName.Value,
            catalogItem.AvailableStock.Value,
            inventoryItem?.Id,
            inventoryItem?.AvailableQuantity.Value,
            inventoryItem?.ReservedQuantity.Value,
            lots,
            catalogItem.ColdChainRequirement.ToString(),
            lastMovementType);
    }

    public async Task<PagedResult<PromotionalCatalogItemReadModel>> GetPromotionalCatalogAsync(PaginationRequest pagination, CancellationToken cancellationToken = default)
    {
        var page = await Scoped(context.CatalogItems.AsNoTracking())
            .Where(row => row.IsActive)
            .OrderBy(row => row.ItemName)
            .ToPagedResultAsync(pagination, cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var catalogIds = page.Items.Select(row => row.Id).ToArray();
        var promotions = await Scoped(context.PromotionCatalogItems.AsNoTracking())
            .Where(row => catalogIds.Contains(row.CatalogItemId))
            .Join(Scoped(context.Promotions.AsNoTracking()),
                link => link.PromotionId,
                promotion => promotion.Id,
                (link, promotion) => new { link.CatalogItemId, promotion })
            .Where(row => row.promotion.Status == "active")
            .Where(row => !row.promotion.StartsOn.HasValue || row.promotion.StartsOn.Value <= today)
            .Where(row => !row.promotion.EndsOn.HasValue || row.promotion.EndsOn.Value >= today)
            .ToListAsync(cancellationToken);

        return new PagedResult<PromotionalCatalogItemReadModel>(
            page.Items.Select(item =>
            {
                var promotion = promotions.FirstOrDefault(row => row.CatalogItemId == item.Id)?.promotion;
                return new PromotionalCatalogItemReadModel(
                    item.Id,
                    item.ProductId.Value,
                    item.ItemName.Value,
                    item.BrandName.Value,
                    item.CategoryName.Value,
                    item.UnitPrice.Amount,
                    item.UnitPrice.Currency,
                    promotion?.Code,
                    promotion?.DiscountLabel,
                    item.AvailableStock.Value);
            }).ToArray(),
            page.Page,
            page.PageSize,
            page.TotalItems,
            page.TotalPages);
    }

    private async Task<IReadOnlyCollection<OrderSummaryReadModel>> MapOrderSummariesAsync(
        IReadOnlyCollection<Order> orders,
        CancellationToken cancellationToken)
    {
        var clientIds = orders.Select(row => row.ClientAccountId).OfType<int>().Distinct().ToArray();
        var orderIds = orders.Select(row => row.Id).ToArray();
        var clients = await Scoped(context.ClientAccounts.AsNoTracking())
            .Where(row => clientIds.Contains(row.Id))
            .ToDictionaryAsync(row => row.Id, ToClientSummary, cancellationToken);
        var dispatchStatuses = await Scoped(context.DispatchOrders.AsNoTracking())
            .Where(row => orderIds.Contains(row.OrderId))
            .GroupBy(row => row.OrderId)
            .Select(group => new { OrderId = group.Key, Status = group.OrderByDescending(row => row.CreatedAt).Select(row => row.Status).FirstOrDefault() })
            .ToDictionaryAsync(row => row.OrderId, row => row.Status, cancellationToken);
        var paymentStatuses = await Scoped(context.Invoices.AsNoTracking())
            .Where(row => orderIds.Contains(row.OrderId))
            .GroupBy(row => row.OrderId)
            .Select(group => new { OrderId = group.Key, Status = group.OrderByDescending(row => row.CreatedAt).Select(row => row.PaymentStatus.ToString()).FirstOrDefault() })
            .ToDictionaryAsync(row => row.OrderId, row => row.Status, cancellationToken);

        return orders.Select(order => new OrderSummaryReadModel(
            order.Id,
            order.OrderNumber.Value,
            order.Status.ToString(),
            order.ClientAccountId.HasValue && clients.TryGetValue(order.ClientAccountId.Value, out var client) ? client : null,
            order.Total.Amount,
            order.Total.Currency,
            order.CreatedAt,
            order.Delivery.RequestedDate,
            dispatchStatuses.GetValueOrDefault(order.Id),
            paymentStatuses.GetValueOrDefault(order.Id),
            order.Items.Count)).ToArray();
    }

    private async Task<IReadOnlyCollection<PurchaseRequestInboxReadModel>> MapPurchaseRequestInboxAsync(
        IReadOnlyCollection<PurchaseRequest> requests,
        CancellationToken cancellationToken)
    {
        var clientIds = requests.Select(row => row.ClientAccountId).Distinct().ToArray();
        var requestIds = requests.Select(row => row.Id).ToArray();
        var clients = await Scoped(context.ClientAccounts.AsNoTracking())
            .Where(row => clientIds.Contains(row.Id))
            .ToDictionaryAsync(row => row.Id, ToClientSummary, cancellationToken);
        var lineCounts = await Scoped(context.PurchaseRequestLines.AsNoTracking())
            .Where(row => requestIds.Contains(row.PurchaseRequestId))
            .GroupBy(row => row.PurchaseRequestId)
            .Select(group => new { RequestId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(row => row.RequestId, row => row.Count, cancellationToken);
        var messages = await Scoped(context.ConversationMessages.AsNoTracking())
            .Where(row => row.PurchaseRequestId.HasValue && requestIds.Contains(row.PurchaseRequestId.Value))
            .GroupBy(row => row.PurchaseRequestId!.Value)
            .Select(group => new { RequestId = group.Key, Body = group.OrderByDescending(row => row.CreatedAt).Select(row => row.Body).FirstOrDefault() })
            .ToDictionaryAsync(row => row.RequestId, row => row.Body, cancellationToken);

        return requests.Select(request => new PurchaseRequestInboxReadModel(
            request.Id,
            request.Code,
            clients.GetValueOrDefault(request.ClientAccountId),
            request.Status,
            request.Priority,
            request.CreatedAt,
            request.RequestedDeliveryDate,
            lineCounts.GetValueOrDefault(request.Id),
            messages.GetValueOrDefault(request.Id),
            request.CommercialOwner)).ToArray();
    }

    private async Task<CreditSummaryReadModel?> GetCreditSummaryForCurrentBuyerAsync(CancellationToken cancellationToken)
    {
        if (workspaceContext.ClientAccountId is not { } clientAccountId) return null;
        var client = await Scoped(context.ClientAccounts.AsNoTracking())
            .FirstOrDefaultAsync(row => row.Id == clientAccountId, cancellationToken);
        return client is null
            ? null
            : new CreditSummaryReadModel(client.MonthlyCreditLimit, client.MonthlyCreditUsed, client.MonthlyCreditAvailable, client.MonthlyCreditStatus, false);
    }

    private IQueryable<T> Scoped<T>(IQueryable<T> query) where T : class
    {
        if (workspaceContext.TenantId is not { } tenantId) return query.Where(_ => false);
        return query.Where(row => EF.Property<int>(row, "TenantId") == tenantId);
    }

    private static ClientSummaryReadModel ToClientSummary(ClientAccount client) =>
        new(client.Id, client.Code, client.BusinessName, client.CommercialName);

    private static OrderLineReadModel ToOrderLine(OrderItem item) =>
        new(item.Id, item.ProductId.Value, item.CatalogItemId.Value, item.ItemName.Value, item.Quantity.Value, item.UnitPrice.Amount, item.Subtotal.Amount);
}
