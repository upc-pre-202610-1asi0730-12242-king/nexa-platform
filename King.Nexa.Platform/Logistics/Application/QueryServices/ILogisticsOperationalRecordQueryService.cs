using King.Nexa.Platform.Logistics.Domain.Model.Entities;

namespace King.Nexa.Platform.Logistics.Application.QueryServices;

public interface ILogisticsOperationalRecordQueryService
{
    Task<IEnumerable<DispatchEvent>> ListDispatchEventsAsync(CancellationToken cancellationToken = default);
    Task<DispatchEvent?> GetDispatchEventByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IEnumerable<ProofOfDeliveryRecord>> ListProofsOfDeliveryAsync(CancellationToken cancellationToken = default);
    Task<ProofOfDeliveryRecord?> GetProofOfDeliveryByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProofOfDeliveryRecord>> ListProofsOfDeliveryByOrderAsync(int orderId, CancellationToken cancellationToken = default);

    Task<IEnumerable<TemperatureLog>> ListTemperatureLogsAsync(CancellationToken cancellationToken = default);
    Task<TemperatureLog?> GetTemperatureLogByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TemperatureLog>> ListTemperatureLogsByDispatchAsync(int dispatchOrderId, CancellationToken cancellationToken = default);
}
