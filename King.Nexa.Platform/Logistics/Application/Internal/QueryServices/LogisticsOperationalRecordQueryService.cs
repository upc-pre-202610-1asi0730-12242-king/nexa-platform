using King.Nexa.Platform.Logistics.Application.QueryServices;
using King.Nexa.Platform.Logistics.Domain.Model.Entities;
using King.Nexa.Platform.Logistics.Domain.Repositories;

namespace King.Nexa.Platform.Logistics.Application.Internal.QueryServices;

public class LogisticsOperationalRecordQueryService(
    ILogisticsOperationalRecordRepository recordRepository) : ILogisticsOperationalRecordQueryService
{
    public async Task<IEnumerable<DispatchEvent>> ListDispatchEventsAsync(CancellationToken cancellationToken = default) =>
        await recordRepository.ListDispatchEventsAsync(cancellationToken);

    public Task<DispatchEvent?> GetDispatchEventByIdAsync(int id, CancellationToken cancellationToken = default) =>
        recordRepository.FindByIdAsync<DispatchEvent>(id, cancellationToken);

    public async Task<IEnumerable<ProofOfDeliveryRecord>> ListProofsOfDeliveryAsync(CancellationToken cancellationToken = default) =>
        await recordRepository.ListProofsOfDeliveryAsync(cancellationToken);

    public Task<ProofOfDeliveryRecord?> GetProofOfDeliveryByIdAsync(int id, CancellationToken cancellationToken = default) =>
        recordRepository.FindByIdAsync<ProofOfDeliveryRecord>(id, cancellationToken);

    public async Task<IEnumerable<ProofOfDeliveryRecord>> ListProofsOfDeliveryByOrderAsync(int orderId, CancellationToken cancellationToken = default)
        => await recordRepository.ListProofsOfDeliveryByOrderAsync(orderId, cancellationToken);

    public async Task<IEnumerable<TemperatureLog>> ListTemperatureLogsAsync(CancellationToken cancellationToken = default) =>
        await recordRepository.ListTemperatureLogsAsync(cancellationToken);

    public Task<TemperatureLog?> GetTemperatureLogByIdAsync(int id, CancellationToken cancellationToken = default) =>
        recordRepository.FindByIdAsync<TemperatureLog>(id, cancellationToken);

    public async Task<IEnumerable<TemperatureLog>> ListTemperatureLogsByDispatchAsync(int dispatchOrderId, CancellationToken cancellationToken = default) =>
        await recordRepository.ListTemperatureLogsByDispatchAsync(dispatchOrderId, cancellationToken);
}
