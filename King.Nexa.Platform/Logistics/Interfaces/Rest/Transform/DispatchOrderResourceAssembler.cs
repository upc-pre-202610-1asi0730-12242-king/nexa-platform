using King.Nexa.Platform.Logistics.Application.QueryServices;
using King.Nexa.Platform.Logistics.Domain.Model.Entities;
using King.Nexa.Platform.Logistics.Interfaces.Rest.Resources;

namespace King.Nexa.Platform.Logistics.Interfaces.Rest.Transform;

public static class DispatchOrderResourceAssembler
{
    public static DispatchOrderResource ToResourceFromEntity(DispatchOrder dispatch) =>
        new(
            dispatch.Id,
            dispatch.TenantId,
            dispatch.OrderId,
            dispatch.ClientAccountId,
            dispatch.Code,
            dispatch.Status,
            dispatch.RouteName,
            dispatch.Responsible,
            dispatch.Eta,
            dispatch.DeliveryWindow,
            dispatch.CreatedAt,
            dispatch.UpdatedAt);

    public static DispatchOrder ToEntityFromResource(UpsertDispatchOrderResource resource) =>
        new()
        {
            TenantId = resource.TenantId,
            OrderId = resource.OrderId,
            ClientAccountId = resource.ClientAccountId,
            Code = resource.Code,
            Status = resource.Status,
            RouteName = resource.RouteName,
            Responsible = resource.Responsible,
            Eta = resource.Eta,
            DeliveryWindow = resource.DeliveryWindow
        };

    public static DispatchTrackingResource ToResourceFromSnapshot(DispatchTrackingSnapshot snapshot) =>
        new(
            snapshot.DispatchOrders.Select(ToResourceFromEntity),
            snapshot.Events.Select(ToResourceFromEntity),
            snapshot.TemperatureLogs.Select(ToResourceFromEntity),
            snapshot.ProofsOfDelivery.Select(ToResourceFromEntity));

    public static DispatchEventResource ToResourceFromEntity(DispatchEvent dispatchEvent) =>
        new(
            dispatchEvent.Id,
            dispatchEvent.TenantId,
            dispatchEvent.DispatchOrderId,
            dispatchEvent.Status,
            dispatchEvent.Description,
            dispatchEvent.VisibleToBuyer,
            dispatchEvent.CreatedAt,
            dispatchEvent.UpdatedAt);

    public static DispatchEvent ToEntityFromResource(UpsertDispatchEventResource resource) =>
        new()
        {
            TenantId = resource.TenantId,
            DispatchOrderId = resource.DispatchOrderId,
            Status = resource.Status,
            Description = resource.Description,
            VisibleToBuyer = resource.VisibleToBuyer
        };

    public static ProofOfDeliveryRecordResource ToResourceFromEntity(ProofOfDeliveryRecord pod) =>
        new(
            pod.Id,
            pod.TenantId,
            pod.DispatchOrderId,
            pod.ReceivedBy,
            pod.CompletedAt,
            pod.PhotoReference,
            pod.SignatureReference,
            pod.Notes,
            pod.Status,
            pod.CreatedAt,
            pod.UpdatedAt);

    public static ProofOfDeliveryRecord ToEntityFromResource(UpsertProofOfDeliveryRecordResource resource) =>
        new()
        {
            TenantId = resource.TenantId,
            DispatchOrderId = resource.DispatchOrderId,
            ReceivedBy = resource.ReceivedBy,
            CompletedAt = resource.CompletedAt,
            PhotoReference = resource.PhotoReference,
            SignatureReference = resource.SignatureReference,
            Notes = resource.Notes,
            Status = resource.Status
        };

    public static TemperatureLogResource ToResourceFromEntity(TemperatureLog temperatureLog) =>
        new(
            temperatureLog.Id,
            temperatureLog.TenantId,
            temperatureLog.DispatchOrderId,
            temperatureLog.OrderId,
            temperatureLog.Celsius,
            temperatureLog.Zone,
            temperatureLog.Status,
            temperatureLog.RecordedAt,
            temperatureLog.CreatedAt,
            temperatureLog.UpdatedAt);

    public static TemperatureLog ToEntityFromResource(UpsertTemperatureLogResource resource) =>
        new()
        {
            TenantId = resource.TenantId,
            DispatchOrderId = resource.DispatchOrderId,
            OrderId = resource.OrderId,
            Celsius = resource.Celsius,
            Zone = resource.Zone,
            Status = resource.Status,
            RecordedAt = resource.RecordedAt ?? DateTime.UtcNow
        };
}
