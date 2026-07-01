using King.Nexa.Platform.Logistics.Domain.Model.Entities;

namespace King.Nexa.Platform.Logistics.Domain.Repositories;

public interface IDispatchOrderRepository
{
    Task AddAsync(DispatchOrder dispatch, CancellationToken cancellationToken = default);
    Task<DispatchOrder?> FindByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<DispatchOrder>> ListAsync(CancellationToken cancellationToken = default);
    void Remove(DispatchOrder dispatch);
    Task AddEventAsync(DispatchEvent dispatchEvent, CancellationToken cancellationToken = default);
    Task<DispatchTrackingRecordSet> GetTrackingByOrderAsync(int orderId, CancellationToken cancellationToken = default);
}

public interface ILogisticsReferenceRepository
{
    Task<bool> OrderBelongsToTenantAsync(int tenantId, int orderId, CancellationToken cancellationToken = default);
    Task<bool> ClientBelongsToTenantAsync(int tenantId, int clientAccountId, CancellationToken cancellationToken = default);
    Task<DispatchOrderReference?> FindOrderAsync(int tenantId, int orderId, CancellationToken cancellationToken = default);
    Task<DispatchClientReference?> FindClientByIdAsync(int tenantId, int clientAccountId, CancellationToken cancellationToken = default);
    Task<DispatchClientReference?> FindClientByCodeAsync(int tenantId, string clientCode, CancellationToken cancellationToken = default);
}

public interface ILogisticsSalesReturnRepository
{
    Task AddReturnToSalesAsync(
        int tenantId,
        int clientAccountId,
        int orderId,
        string senderName,
        string dispatchCode,
        string description,
        CancellationToken cancellationToken = default);
}

public interface ILogisticsOperationalRecordRepository
{
    Task AddAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class;
    Task<T?> FindByIdAsync<T>(int id, CancellationToken cancellationToken = default) where T : class;
    void Remove<T>(T entity) where T : class;
    Task<bool> DispatchBelongsToTenantAsync(int tenantId, int dispatchOrderId, CancellationToken cancellationToken = default);
    Task<bool> OrderBelongsToTenantAsync(int tenantId, int orderId, CancellationToken cancellationToken = default);
    Task<DispatchOrder?> FindDispatchByIdAsync(int tenantId, int dispatchOrderId, CancellationToken cancellationToken = default);
    Task<ProofOfDeliveryRecord?> FindProofByDispatchAsync(int tenantId, int dispatchOrderId, CancellationToken cancellationToken = default);
    Task<IEnumerable<DispatchEvent>> ListDispatchEventsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ProofOfDeliveryRecord>> ListProofsOfDeliveryAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ProofOfDeliveryRecord>> ListProofsOfDeliveryByOrderAsync(int orderId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TemperatureLog>> ListTemperatureLogsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TemperatureLog>> ListTemperatureLogsByDispatchAsync(int dispatchOrderId, CancellationToken cancellationToken = default);
}

public record DispatchOrderReference(int Id, int TenantId, string OrderNumber, string CustomerCode, string Status, DateOnly? RequestedDeliveryDate = null);

public record DispatchClientReference(int Id, string Code, string DeliveryPreference);

public record DispatchTrackingRecordSet(
    IEnumerable<DispatchOrder> DispatchOrders,
    IEnumerable<DispatchEvent> Events,
    IEnumerable<TemperatureLog> TemperatureLogs,
    IEnumerable<ProofOfDeliveryRecord> ProofsOfDelivery);
