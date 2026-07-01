using King.Nexa.Platform.Sales.Domain.Model.Aggregates;
using King.Nexa.Platform.Sales.Domain.Model.Queries;
using King.Nexa.Platform.Shared.Application.Pagination;

namespace King.Nexa.Platform.Sales.Application.QueryServices;

public interface IOrderQueryService
{
    Task<IEnumerable<Order>> Handle(GetAllOrdersQuery query, CancellationToken cancellationToken = default);

    Task<Order?> Handle(GetOrderByIdQuery query, CancellationToken cancellationToken = default);

    Task<Order?> Handle(GetOrderByOrderNumberQuery query, CancellationToken cancellationToken = default);

    Task<IEnumerable<Order>> Handle(GetOrdersByCustomerIdQuery query, CancellationToken cancellationToken = default);

    Task<IEnumerable<Order>> Handle(GetOrdersByStatusQuery query, CancellationToken cancellationToken = default);

    Task<PagedResult<Order>> SearchAsync(OrderCollectionQuery query, CancellationToken cancellationToken = default);
}
