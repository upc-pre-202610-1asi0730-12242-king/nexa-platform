using King.Nexa.Platform.Logistics.Domain.Model.Entities;

namespace King.Nexa.Platform.Logistics.Application.CommandServices;

public interface IDispatchOrderCommandService
{
    Task<DispatchOrder> CreateAsync(DispatchOrder dispatch, CancellationToken cancellationToken = default);
    Task<DispatchOrder?> CreateForOrderAsync(int orderId, int? clientAccountId, string? code, string? routeName, CancellationToken cancellationToken = default);
    Task<DispatchOrder?> UpdateAsync(int id, DispatchOrder dispatch, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<DispatchOrder?> AssignAsync(int id, string responsible, CancellationToken cancellationToken = default);
    Task<DispatchOrder?> ScheduleAsync(int id, DateTime eta, string deliveryWindow, string? note, CancellationToken cancellationToken = default);
    Task<DispatchOrder?> StartRouteAsync(int id, CancellationToken cancellationToken = default);
    Task<DispatchOrder?> CompleteAsync(int id, CancellationToken cancellationToken = default);
    Task<DispatchOrder?> IncidentAsync(int id, string? note, CancellationToken cancellationToken = default);
    Task<DispatchOrder?> ReprogramAsync(int id, DateTime eta, string deliveryWindow, string? note, CancellationToken cancellationToken = default);
    Task<DispatchOrder?> ChangeStatusAsync(int id, string status, string description, bool visibleToBuyer, CancellationToken cancellationToken = default);
}
