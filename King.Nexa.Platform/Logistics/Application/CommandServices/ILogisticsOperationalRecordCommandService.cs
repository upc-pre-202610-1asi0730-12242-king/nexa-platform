using King.Nexa.Platform.Logistics.Domain.Model.Entities;

namespace King.Nexa.Platform.Logistics.Application.CommandServices;

public interface ILogisticsOperationalRecordCommandService
{
    Task<DispatchEvent> CreateDispatchEventAsync(DispatchEvent dispatchEvent, CancellationToken cancellationToken = default);
    Task<DispatchEvent?> UpdateDispatchEventAsync(int id, DispatchEvent draft, CancellationToken cancellationToken = default);
    Task<bool> DeleteDispatchEventAsync(int id, CancellationToken cancellationToken = default);

    Task<ProofOfDeliveryRecord> CreateProofOfDeliveryAsync(ProofOfDeliveryRecord proof, CancellationToken cancellationToken = default);
    Task<ProofOfDeliveryRecord> CreateProofOfDeliveryForDispatchAsync(int dispatchOrderId, string receivedBy, DateTime? completedAt, bool photoReference, bool signatureReference, string? notes, CancellationToken cancellationToken = default);
    Task<ProofOfDeliveryRecord?> CompleteProofOfDeliveryAsync(int id, string receivedBy, DateTime? completedAt, bool photoReference, bool signatureReference, string? notes, CancellationToken cancellationToken = default);
    Task<ProofOfDeliveryRecord?> UpdateProofOfDeliveryAsync(int id, ProofOfDeliveryRecord draft, CancellationToken cancellationToken = default);
    Task<bool> DeleteProofOfDeliveryAsync(int id, CancellationToken cancellationToken = default);

    Task<TemperatureLog> CreateTemperatureLogAsync(TemperatureLog temperatureLog, CancellationToken cancellationToken = default);
    Task<TemperatureLog> CreateTemperatureLogForDispatchAsync(int dispatchOrderId, TemperatureLog draft, CancellationToken cancellationToken = default);
    Task<TemperatureLog?> UpdateTemperatureLogAsync(int id, TemperatureLog draft, CancellationToken cancellationToken = default);
    Task<TemperatureLog?> ResolveTemperatureAlertAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> DeleteTemperatureLogAsync(int id, CancellationToken cancellationToken = default);
}
