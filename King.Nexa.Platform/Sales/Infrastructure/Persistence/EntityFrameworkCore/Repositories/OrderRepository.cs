using King.Nexa.Platform.Sales.Domain.Model.Aggregates;
using King.Nexa.Platform.Sales.Domain.Model.Queries;
using King.Nexa.Platform.Sales.Domain.Model.ValueObjects;
using King.Nexa.Platform.Sales.Domain.Repositories;
using King.Nexa.Platform.Shared.Application.Security;
using King.Nexa.Platform.Shared.Application.Pagination;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Queries;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace King.Nexa.Platform.Sales.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class OrderRepository(AppDbContext context, ICurrentWorkspaceContext workspaceContext) : BaseRepository<Order>(context), IOrderRepository
{
    public override async Task<Order?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await Scoped().FirstOrDefaultAsync(order => order.Id == id, cancellationToken);

    public override async Task<IEnumerable<Order>> ListAsync(CancellationToken cancellationToken = default) =>
        await Scoped().ToListAsync(cancellationToken);

    public async Task<Order?> FindByOrderNumberAsync(OrderNumber orderNumber, CancellationToken cancellationToken = default) =>
        await ScopedWithItems().FirstOrDefaultAsync(order => order.OrderNumber == orderNumber, cancellationToken);

    public async Task<bool> ExistsByOrderNumberAsync(OrderNumber orderNumber, CancellationToken cancellationToken = default)
    {
        if (workspaceContext.TenantId is not { } tenantId) return false;

        return await Context.Orders
            .AsNoTracking()
            .AnyAsync(order => order.TenantId == tenantId && order.OrderNumber == orderNumber, cancellationToken);
    }

    public async Task<Order?> FindByIdWithItemsAsync(int id, CancellationToken cancellationToken = default) =>
        await ScopedWithItems().FirstOrDefaultAsync(order => order.Id == id, cancellationToken);

    public async Task<IEnumerable<Order>> ListWithItemsAsync(CancellationToken cancellationToken = default) =>
        await ScopedWithItems().ToListAsync(cancellationToken);

    public async Task<IEnumerable<Order>> ListByCustomerIdAsync(CustomerId customerId, CancellationToken cancellationToken = default) =>
        await ScopedWithItems().Where(order => order.CustomerId == customerId).ToListAsync(cancellationToken);

    public async Task<IEnumerable<Order>> ListByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default) =>
        await ScopedWithItems().Where(order => order.Status == status).ToListAsync(cancellationToken);

    public async Task<PagedResult<Order>> SearchAsync(OrderCollectionQuery query, CancellationToken cancellationToken = default)
    {
        var orders = ScopedWithItems().AsNoTracking();

        if (query.Status.HasValue)
            orders = orders.Where(order => order.Status == query.Status.Value);
        if (query.ClientAccountId.HasValue)
            orders = orders.Where(order => order.ClientAccountId == query.ClientAccountId.Value);
        if (query.CreatedFrom.HasValue)
            orders = orders.Where(order => order.CreatedAt >= query.CreatedFrom.Value.ToDateTime(TimeOnly.MinValue));
        if (query.CreatedTo.HasValue)
            orders = orders.Where(order => order.CreatedAt <= query.CreatedTo.Value.ToDateTime(TimeOnly.MaxValue));
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            orders = orders.Where(order => order.OrderNumber == new OrderNumber(search) || order.CustomerId == new CustomerId(search));
        }

        orders = query.Sort?.Trim().ToLowerInvariant() switch
        {
            "created" or "created_asc" => orders.OrderBy(order => order.CreatedAt),
            "status" => orders.OrderBy(order => order.Status).ThenByDescending(order => order.CreatedAt),
            "total" => orders.OrderByDescending(order => order.Total.Amount),
            _ => orders.OrderByDescending(order => order.CreatedAt)
        };

        return await orders.ToPagedResultAsync(query.Pagination, cancellationToken);
    }

    private IQueryable<Order> Scoped()
    {
        var query = Context.Orders.AsQueryable();
        if (workspaceContext.TenantId is not { } tenantId)
            return query.Where(_ => false);

        query = query.Where(order => order.TenantId == tenantId);
        if (workspaceContext.ClientAccountId is not { } clientAccountId) return query;

        return query.Where(order => order.ClientAccountId == clientAccountId);
    }

    private IQueryable<Order> ScopedWithItems() => Scoped().Include(order => order.Items);
}
