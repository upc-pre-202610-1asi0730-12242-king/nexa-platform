using King.Nexa.Platform.Logistics.Domain.Model.Aggregates;
using King.Nexa.Platform.Logistics.Interfaces.REST.Resources;

namespace King.Nexa.Platform.Logistics.Interfaces.REST.Transform;

public static class ShipmentResourceFromEntityAssembler
{
    public static ShipmentResource ToResourceFromEntity(Shipment entity) =>
        new(entity.Id, entity.ShipmentCode.Value, entity.OrderId, entity.Status.ToString(), entity.ScheduledAt, entity.DeliveredAt, entity.LastTemperatureRecord?.Celsius);
}
