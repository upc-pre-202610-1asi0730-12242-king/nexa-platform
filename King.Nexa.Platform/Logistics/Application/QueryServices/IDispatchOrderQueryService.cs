using King.Nexa.Platform.Logistics.Domain.Model.Entities;

namespace King.Nexa.Platform.Logistics.Application.QueryServices;

public interface IDispatchOrderQueryService
{
    Task<IEnumerable<DispatchOrder>> ListAsync(CancellationToken cancellationToken = default);
    Task<DispatchOrder?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<DispatchTrackingSnapshot?> GetOrderTrackingAsync(int orderId, CancellationToken cancellationToken = default);
}

public record DispatchTrackingSnapshot(
    IEnumerable<DispatchOrder> DispatchOrders,
    IEnumerable<DispatchEvent> Events,
    IEnumerable<TemperatureLog> TemperatureLogs,
    IEnumerable<ProofOfDeliveryRecord> ProofsOfDelivery);
