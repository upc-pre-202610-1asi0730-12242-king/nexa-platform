using King.Nexa.Platform.Logistics.Application.CommandServices;
using King.Nexa.Platform.Logistics.Domain.Model.Entities;
using King.Nexa.Platform.Logistics.Domain.Repositories;
using King.Nexa.Platform.Shared.Application.Security;
using King.Nexa.Platform.Shared.Domain.Repositories;

namespace King.Nexa.Platform.Logistics.Application.Internal.CommandServices;

public class DispatchOrderCommandService(
    IDispatchOrderRepository dispatchRepository,
    ILogisticsReferenceRepository referenceRepository,
    ILogisticsSalesReturnRepository salesReturnRepository,
    IUnitOfWork unitOfWork,
    ICurrentWorkspaceContext workspaceContext) : IDispatchOrderCommandService
{
    public async Task<DispatchOrder> CreateAsync(DispatchOrder dispatch, CancellationToken cancellationToken = default)
    {
        var tenantId = workspaceContext.TenantId
            ?? throw new InvalidOperationException("Current tenant is required to create dispatch orders.");
        await EnsureOrderBelongsToTenantAsync(dispatch.OrderId, tenantId, cancellationToken);
        await EnsureClientBelongsToTenantAsync(dispatch.ClientAccountId, tenantId, cancellationToken);

        dispatch.TenantId = tenantId;
        Normalize(dispatch);
        await dispatchRepository.AddAsync(dispatch, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        await AddEvent(dispatch, dispatch.Status, $"Dispatch {dispatch.Code} created.", true, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return dispatch;
    }

    public async Task<DispatchOrder?> CreateForOrderAsync(int orderId, int? clientAccountId, string? code, string? routeName, CancellationToken cancellationToken = default)
    {
        var tenantId = workspaceContext.TenantId ?? throw new InvalidOperationException("Current tenant is required to create dispatch orders.");
        var order = await referenceRepository.FindOrderAsync(tenantId, orderId, cancellationToken);
        if (order is null) return null;
        if (string.Equals(order.Status, "Cancelled", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(order.Status, "Rejected", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Dispatch order cannot be created for cancelled or rejected orders.");

        var client = clientAccountId.HasValue
            ? await referenceRepository.FindClientByIdAsync(tenantId, clientAccountId.Value, cancellationToken)
            : await referenceRepository.FindClientByCodeAsync(tenantId, order.CustomerCode, cancellationToken);
        if (client is null) throw new InvalidOperationException("Client account is required for dispatch.");

        var dispatch = new DispatchOrder
        {
            TenantId = tenantId,
            OrderId = order.Id,
            ClientAccountId = client.Id,
            Code = string.IsNullOrWhiteSpace(code) ? $"DSP-{order.OrderNumber.Replace("ORD-", string.Empty, StringComparison.OrdinalIgnoreCase)}" : code.Trim(),
            RouteName = routeName?.Trim() ?? string.Empty,
            Status = "ready_for_operations",
            Eta = order.RequestedDeliveryDate.HasValue
                ? DateTime.SpecifyKind(order.RequestedDeliveryDate.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc)
                : null
        };

        await dispatchRepository.AddAsync(dispatch, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        await AddEvent(dispatch, "ready_for_operations", $"Order {order.OrderNumber} received by Logistics.", true, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return dispatch;
    }

    public async Task<DispatchOrder?> UpdateAsync(int id, DispatchOrder draft, CancellationToken cancellationToken = default)
    {
        var dispatch = await FindDispatchAsync(id, cancellationToken);
        if (dispatch is null) return null;
        await EnsureOrderBelongsToTenantAsync(draft.OrderId, dispatch.TenantId, cancellationToken);
        await EnsureClientBelongsToTenantAsync(draft.ClientAccountId, dispatch.TenantId, cancellationToken);

        dispatch.OrderId = draft.OrderId;
        dispatch.ClientAccountId = draft.ClientAccountId;
        dispatch.Code = draft.Code;
        dispatch.RouteName = draft.RouteName;
        dispatch.Responsible = draft.Responsible;
        dispatch.Eta = draft.Eta;
        dispatch.DeliveryWindow = draft.DeliveryWindow;
        if (!string.Equals(dispatch.Status, draft.Status, StringComparison.OrdinalIgnoreCase))
        {
            dispatch.ChangeStatus(draft.Status);
            await AddEvent(dispatch, dispatch.Status, $"Dispatch status changed to {dispatch.Status}.", true, cancellationToken);
        }
        Normalize(dispatch);
        dispatch.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.CompleteAsync(cancellationToken);
        return dispatch;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var dispatch = await FindDispatchAsync(id, cancellationToken);
        if (dispatch is null) return false;
        dispatchRepository.Remove(dispatch);
        await unitOfWork.CompleteAsync(cancellationToken);
        return true;
    }

    public async Task<DispatchOrder?> AssignAsync(int id, string responsible, CancellationToken cancellationToken = default)
    {
        var dispatch = await FindDispatchAsync(id, cancellationToken);
        if (dispatch is null) return null;
        dispatch.Assign(responsible);
        await AddEvent(dispatch, "assigned", $"Assigned to {responsible}", true, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return dispatch;
    }

    public async Task<DispatchOrder?> ScheduleAsync(int id, DateTime eta, string deliveryWindow, string? note, CancellationToken cancellationToken = default)
    {
        var dispatch = await FindDispatchAsync(id, cancellationToken);
        if (dispatch is null) return null;
        dispatch.Schedule(eta, deliveryWindow, "scheduled");
        await AddEvent(dispatch, "scheduled", note ?? $"Scheduled for {deliveryWindow}", true, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return dispatch;
    }

    public Task<DispatchOrder?> StartRouteAsync(int id, CancellationToken cancellationToken = default) =>
        ChangeStatusAsync(id, "in_route", "Dispatch started", true, cancellationToken);

    public Task<DispatchOrder?> CompleteAsync(int id, CancellationToken cancellationToken = default) =>
        ChangeStatusAsync(id, "delivered", "Dispatch delivered", true, cancellationToken);

    public Task<DispatchOrder?> IncidentAsync(int id, string? note, CancellationToken cancellationToken = default) =>
        ChangeStatusAsync(id, "incident", note ?? "Operational incident registered", true, cancellationToken);

    public async Task<DispatchOrder?> ReprogramAsync(int id, DateTime eta, string deliveryWindow, string? note, CancellationToken cancellationToken = default)
    {
        var dispatch = await FindDispatchAsync(id, cancellationToken);
        if (dispatch is null) return null;
        dispatch.Schedule(eta, deliveryWindow, "reprogrammed");
        await AddEvent(dispatch, "reprogrammed", note ?? $"Reprogrammed for {deliveryWindow}", true, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return dispatch;
    }

    public async Task<DispatchOrder?> ChangeStatusAsync(
        int id,
        string status,
        string description,
        bool visibleToBuyer,
        CancellationToken cancellationToken)
    {
        var dispatch = await FindDispatchAsync(id, cancellationToken);
        if (dispatch is null) return null;
        dispatch.ChangeStatus(status);
        await AddEvent(dispatch, status, description, visibleToBuyer, cancellationToken);
        if (IsReturnedToSales(status))
            await AddSalesReturnMessage(dispatch, description, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return dispatch;
    }

    private Task<DispatchOrder?> FindDispatchAsync(int id, CancellationToken cancellationToken) =>
        dispatchRepository.FindByIdAsync(id, cancellationToken);

    private async Task EnsureOrderBelongsToTenantAsync(int orderId, int tenantId, CancellationToken cancellationToken)
    {
        var exists = await referenceRepository.OrderBelongsToTenantAsync(tenantId, orderId, cancellationToken);
        if (!exists) throw new InvalidOperationException("Order does not belong to the current tenant.");
    }

    private async Task EnsureClientBelongsToTenantAsync(int clientAccountId, int tenantId, CancellationToken cancellationToken)
    {
        var exists = await referenceRepository.ClientBelongsToTenantAsync(tenantId, clientAccountId, cancellationToken);
        if (!exists) throw new InvalidOperationException("Client account does not belong to the current tenant.");
    }

    private static void Normalize(DispatchOrder dispatch)
    {
        dispatch.Code = dispatch.Code.Trim();
        dispatch.Status = string.IsNullOrWhiteSpace(dispatch.Status) ? "ready_for_operations" : dispatch.Status.Trim().ToLowerInvariant();
        dispatch.RouteName = dispatch.RouteName?.Trim() ?? string.Empty;
        dispatch.Responsible = dispatch.Responsible?.Trim() ?? string.Empty;
        dispatch.DeliveryWindow = dispatch.DeliveryWindow?.Trim() ?? string.Empty;
    }

    private async Task AddEvent(
        DispatchOrder dispatch,
        string status,
        string description,
        bool visibleToBuyer,
        CancellationToken cancellationToken)
    {
        await dispatchRepository.AddEventAsync(new DispatchEvent
        {
            TenantId = dispatch.TenantId,
            DispatchOrderId = dispatch.Id,
            Status = status,
            Description = description,
            VisibleToBuyer = visibleToBuyer
        }, cancellationToken);
    }

    private static bool IsReturnedToSales(string status) =>
        status.Trim().ToLowerInvariant() is "incident" or "cancelled" or "rejected";

    private async Task AddSalesReturnMessage(
        DispatchOrder dispatch,
        string description,
        CancellationToken cancellationToken)
    {
        await salesReturnRepository.AddReturnToSalesAsync(
            dispatch.TenantId,
            dispatch.ClientAccountId,
            dispatch.OrderId,
            dispatch.Responsible,
            dispatch.Code,
            description,
            cancellationToken);
    }
}
