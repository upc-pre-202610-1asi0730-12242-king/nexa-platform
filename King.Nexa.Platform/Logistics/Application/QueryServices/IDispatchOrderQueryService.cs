using King.Nexa.Platform.Logistics.Domain.Model.Entities;
using King.Nexa.Platform.Logistics.Domain.Model.Queries;
using King.Nexa.Platform.Shared.Application.Pagination;

namespace King.Nexa.Platform.Logistics.Application.QueryServices;

public interface IDispatchOrderQueryService
{
    Task<IEnumerable<DispatchOrder>> ListAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<DispatchOrder>> SearchAsync(DispatchOrderCollectionQuery query, CancellationToken cancellationToken = default);
    Task<DispatchOrder?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<DispatchTrackingSnapshot?> GetOrderTrackingAsync(int orderId, CancellationToken cancellationToken = default);
}

public record DispatchTrackingSnapshot(
    IEnumerable<DispatchOrder> DispatchOrders,
    IEnumerable<DispatchEvent> Events,
    IEnumerable<TemperatureLog> TemperatureLogs,
    IEnumerable<ProofOfDeliveryRecord> ProofsOfDelivery);
