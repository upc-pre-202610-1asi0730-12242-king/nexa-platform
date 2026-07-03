using King.Nexa.Platform.Invoicing.Domain.Model.Entities;
using King.Nexa.Platform.Logistics.Domain.Model.Entities;
using King.Nexa.Platform.Logistics.Domain.Model.Queries;
using King.Nexa.Platform.Logistics.Domain.Repositories;
using King.Nexa.Platform.Sales.Domain.Model.Entities;
using King.Nexa.Platform.Shared.Application.Pagination;
using King.Nexa.Platform.Shared.Application.Security;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Queries;
using Microsoft.EntityFrameworkCore;

namespace King.Nexa.Platform.Logistics.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class DispatchOrderRepository(AppDbContext context, ICurrentWorkspaceContext workspaceContext) : IDispatchOrderRepository
{
    public async Task AddAsync(DispatchOrder dispatch, CancellationToken cancellationToken = default) =>
        await context.DispatchOrders.AddAsync(dispatch, cancellationToken);

    public Task<DispatchOrder?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Scoped().FirstOrDefaultAsync(row => row.Id == id, cancellationToken);

    public async Task<IEnumerable<DispatchOrder>> ListAsync(CancellationToken cancellationToken = default) =>
        await Scoped()
            .AsNoTracking()
            .OrderByDescending(row => row.UpdatedAt ?? row.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<PagedResult<DispatchOrder>> SearchAsync(DispatchOrderCollectionQuery query, CancellationToken cancellationToken = default)
    {
        var dispatches = Scoped().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Status))
            dispatches = dispatches.Where(row => row.Status == query.Status.Trim().ToLowerInvariant());
        if (query.ClientAccountId.HasValue)
            dispatches = dispatches.Where(row => row.ClientAccountId == query.ClientAccountId.Value);
        if (query.OrderId.HasValue)
            dispatches = dispatches.Where(row => row.OrderId == query.OrderId.Value);
        if (query.CreatedFrom.HasValue)
            dispatches = dispatches.Where(row => row.CreatedAt >= query.CreatedFrom.Value.ToDateTime(TimeOnly.MinValue));
        if (query.CreatedTo.HasValue)
            dispatches = dispatches.Where(row => row.CreatedAt <= query.CreatedTo.Value.ToDateTime(TimeOnly.MaxValue));

        dispatches = dispatches.OrderByDescending(row => row.UpdatedAt ?? row.CreatedAt);
        return await dispatches.ToPagedResultAsync(query.Pagination, cancellationToken);
    }

    public void Remove(DispatchOrder dispatch) => context.DispatchOrders.Remove(dispatch);

    public async Task AddEventAsync(DispatchEvent dispatchEvent, CancellationToken cancellationToken = default) =>
        await context.DispatchEvents.AddAsync(dispatchEvent, cancellationToken);

    public async Task<DispatchTrackingRecordSet> GetTrackingByOrderAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var dispatchesQuery = Scoped().AsNoTracking().Where(row => row.OrderId == orderId);

        var dispatches = await dispatchesQuery.OrderBy(row => row.CreatedAt).ToListAsync(cancellationToken);
        if (dispatches.Count == 0) return new DispatchTrackingRecordSet([], [], [], []);

        var dispatchIds = dispatches.Select(row => row.Id).ToList();
        var events = await Scoped(context.DispatchEvents.AsNoTracking())
            .Where(row => dispatchIds.Contains(row.DispatchOrderId) && row.VisibleToBuyer)
            .OrderBy(row => row.CreatedAt)
            .ToListAsync(cancellationToken);
        var temperatures = await Scoped(context.TemperatureLogs.AsNoTracking())
            .Where(row => row.OrderId == orderId)
            .OrderByDescending(row => row.RecordedAt)
            .ToListAsync(cancellationToken);
        var pods = await Scoped(context.ProofOfDeliveryRecords.AsNoTracking())
            .Where(row => dispatchIds.Contains(row.DispatchOrderId))
            .OrderByDescending(row => row.CompletedAt ?? row.CreatedAt)
            .ToListAsync(cancellationToken);

        return new DispatchTrackingRecordSet(dispatches, events, temperatures, pods);
    }

    private IQueryable<DispatchOrder> Scoped()
    {
        if (workspaceContext.TenantId is not { } tenantId)
            return context.DispatchOrders.Where(_ => false);
        var query = context.DispatchOrders.Where(row => row.TenantId == tenantId);
        return workspaceContext.ClientAccountId is { } clientAccountId
            ? query.Where(row => row.ClientAccountId == clientAccountId)
            : query;
    }

    private IQueryable<T> Scoped<T>(IQueryable<T> query) where T : class
    {
        if (workspaceContext.TenantId is not { } tenantId) return query.Where(_ => false);
        query = query.Where(row => EF.Property<int>(row, "TenantId") == tenantId);
        if (workspaceContext.ClientAccountId is not { } clientAccountId) return query;

        if (typeof(T) == typeof(DispatchEvent))
            return (IQueryable<T>)(object)((IQueryable<DispatchEvent>)(object)query)
                .Where(row => context.DispatchOrders.Any(dispatch =>
                    dispatch.Id == row.DispatchOrderId && dispatch.ClientAccountId == clientAccountId));
        if (typeof(T) == typeof(ProofOfDeliveryRecord))
            return (IQueryable<T>)(object)((IQueryable<ProofOfDeliveryRecord>)(object)query)
                .Where(row => context.DispatchOrders.Any(dispatch =>
                    dispatch.Id == row.DispatchOrderId && dispatch.ClientAccountId == clientAccountId));
        if (typeof(T) == typeof(TemperatureLog))
            return (IQueryable<T>)(object)((IQueryable<TemperatureLog>)(object)query)
                .Where(row => row.DispatchOrderId.HasValue && context.DispatchOrders.Any(dispatch =>
                    dispatch.Id == row.DispatchOrderId.Value && dispatch.ClientAccountId == clientAccountId));
        return query;
    }
}

public class LogisticsReferenceRepository(AppDbContext context) : ILogisticsReferenceRepository
{
    public Task<bool> OrderBelongsToTenantAsync(int tenantId, int orderId, CancellationToken cancellationToken = default) =>
        context.Orders.AsNoTracking().AnyAsync(row => row.Id == orderId && row.TenantId == tenantId, cancellationToken);

    public Task<bool> ClientBelongsToTenantAsync(int tenantId, int clientAccountId, CancellationToken cancellationToken = default) =>
        context.ClientAccounts.AsNoTracking().AnyAsync(row => row.Id == clientAccountId && row.TenantId == tenantId, cancellationToken);

    public async Task<DispatchOrderReference?> FindOrderAsync(int tenantId, int orderId, CancellationToken cancellationToken = default) =>
        await context.Orders.AsNoTracking()
            .Where(row => row.TenantId == tenantId && row.Id == orderId)
            .Select(row => new DispatchOrderReference(
                row.Id,
                row.TenantId,
                row.OrderNumber.Value,
                row.CustomerId.Value,
                row.Status.ToString(),
                row.Delivery.RequestedDate))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<DispatchClientReference?> FindClientByIdAsync(int tenantId, int clientAccountId, CancellationToken cancellationToken = default) =>
        await context.ClientAccounts.AsNoTracking()
            .Where(row => row.TenantId == tenantId && row.Id == clientAccountId)
            .Select(row => new DispatchClientReference(row.Id, row.Code, row.DeliveryPreference))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<DispatchClientReference?> FindClientByCodeAsync(int tenantId, string clientCode, CancellationToken cancellationToken = default) =>
        await context.ClientAccounts.AsNoTracking()
            .Where(row => row.TenantId == tenantId && row.Code == clientCode)
            .Select(row => new DispatchClientReference(row.Id, row.Code, row.DeliveryPreference))
            .FirstOrDefaultAsync(cancellationToken);
}

public class LogisticsSalesReturnRepository(AppDbContext context) : ILogisticsSalesReturnRepository
{
    public async Task AddReturnToSalesAsync(
        int tenantId,
        int clientAccountId,
        int orderId,
        string senderName,
        string dispatchCode,
        string description,
        CancellationToken cancellationToken = default)
    {
        await context.ConversationMessages.AddAsync(new ConversationMessage
        {
            TenantId = tenantId,
            ClientAccountId = clientAccountId,
            OrderId = orderId,
            SenderRole = "logistics",
            SenderName = string.IsNullOrWhiteSpace(senderName) ? "Logistics" : senderName,
            Body = $"Logistics returned this order to Sales: {description}",
            VisibleToBuyer = false
        }, cancellationToken);

        await context.NotificationRecords.AddAsync(new NotificationRecord
        {
            TenantId = tenantId,
            ClientAccountId = clientAccountId,
            RecipientRole = "sales",
            Type = "dispatch_returned",
            Title = $"Dispatch {dispatchCode} needs Sales follow-up",
            Body = description,
            Read = false
        }, cancellationToken);
    }
}

public class LogisticsOperationalRecordRepository(AppDbContext context, ICurrentWorkspaceContext workspaceContext) : ILogisticsOperationalRecordRepository
{
    public async Task AddAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class =>
        await context.Set<T>().AddAsync(entity, cancellationToken);

    public Task<T?> FindByIdAsync<T>(int id, CancellationToken cancellationToken = default) where T : class =>
        Scoped(context.Set<T>()).FirstOrDefaultAsync(row => EF.Property<int>(row, "Id") == id, cancellationToken);

    public void Remove<T>(T entity) where T : class => context.Set<T>().Remove(entity);

    public Task<bool> DispatchBelongsToTenantAsync(int tenantId, int dispatchOrderId, CancellationToken cancellationToken = default) =>
        context.DispatchOrders.AsNoTracking().AnyAsync(row => row.Id == dispatchOrderId && row.TenantId == tenantId, cancellationToken);

    public Task<bool> OrderBelongsToTenantAsync(int tenantId, int orderId, CancellationToken cancellationToken = default) =>
        context.Orders.AsNoTracking().AnyAsync(row => row.Id == orderId && row.TenantId == tenantId, cancellationToken);

    public Task<DispatchOrder?> FindDispatchByIdAsync(int tenantId, int dispatchOrderId, CancellationToken cancellationToken = default) =>
        context.DispatchOrders.AsNoTracking()
            .FirstOrDefaultAsync(row => row.TenantId == tenantId && row.Id == dispatchOrderId, cancellationToken);

    public Task<ProofOfDeliveryRecord?> FindProofByDispatchAsync(int tenantId, int dispatchOrderId, CancellationToken cancellationToken = default) =>
        context.ProofOfDeliveryRecords
            .FirstOrDefaultAsync(row => row.TenantId == tenantId && row.DispatchOrderId == dispatchOrderId, cancellationToken);

    public async Task<IEnumerable<DispatchEvent>> ListDispatchEventsAsync(CancellationToken cancellationToken = default) =>
        await Scoped(context.DispatchEvents.AsNoTracking())
            .OrderByDescending(row => row.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<ProofOfDeliveryRecord>> ListProofsOfDeliveryAsync(CancellationToken cancellationToken = default) =>
        await Scoped(context.ProofOfDeliveryRecords.AsNoTracking())
            .OrderByDescending(row => row.CompletedAt ?? row.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<ProofOfDeliveryRecord>> ListProofsOfDeliveryByOrderAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var dispatchIds = await Scoped(context.DispatchOrders.AsNoTracking())
            .Where(row => row.OrderId == orderId)
            .Select(row => row.Id)
            .ToListAsync(cancellationToken);

        return await Scoped(context.ProofOfDeliveryRecords.AsNoTracking())
            .Where(row => dispatchIds.Contains(row.DispatchOrderId))
            .OrderByDescending(row => row.CompletedAt ?? row.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TemperatureLog>> ListTemperatureLogsAsync(CancellationToken cancellationToken = default) =>
        await Scoped(context.TemperatureLogs.AsNoTracking())
            .OrderByDescending(row => row.RecordedAt)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<TemperatureLog>> ListTemperatureLogsByDispatchAsync(int dispatchOrderId, CancellationToken cancellationToken = default) =>
        await Scoped(context.TemperatureLogs.AsNoTracking())
            .Where(row => row.DispatchOrderId == dispatchOrderId)
            .OrderByDescending(row => row.RecordedAt)
            .ToListAsync(cancellationToken);

    private IQueryable<T> Scoped<T>(IQueryable<T> query) where T : class
    {
        if (workspaceContext.TenantId is not { } tenantId) return query.Where(_ => false);
        query = query.Where(row => EF.Property<int>(row, "TenantId") == tenantId);
        if (workspaceContext.ClientAccountId is not { } clientAccountId) return query;

        if (typeof(T) == typeof(DispatchEvent))
            return (IQueryable<T>)(object)((IQueryable<DispatchEvent>)(object)query)
                .Where(row => context.DispatchOrders.Any(dispatch => dispatch.Id == row.DispatchOrderId && dispatch.ClientAccountId == clientAccountId));
        if (typeof(T) == typeof(ProofOfDeliveryRecord))
            return (IQueryable<T>)(object)((IQueryable<ProofOfDeliveryRecord>)(object)query)
                .Where(row => context.DispatchOrders.Any(dispatch => dispatch.Id == row.DispatchOrderId && dispatch.ClientAccountId == clientAccountId));
        if (typeof(T) == typeof(TemperatureLog))
            return (IQueryable<T>)(object)((IQueryable<TemperatureLog>)(object)query)
                .Where(row => row.DispatchOrderId.HasValue && context.DispatchOrders.Any(dispatch => dispatch.Id == row.DispatchOrderId.Value && dispatch.ClientAccountId == clientAccountId));
        if (typeof(T) == typeof(DispatchOrder))
            return (IQueryable<T>)(object)((IQueryable<DispatchOrder>)(object)query)
                .Where(row => row.ClientAccountId == clientAccountId);
        return query;
    }
}
