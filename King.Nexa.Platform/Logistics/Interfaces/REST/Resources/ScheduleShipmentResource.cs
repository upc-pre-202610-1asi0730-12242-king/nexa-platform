namespace King.Nexa.Platform.Logistics.Interfaces.REST.Resources;

public record ScheduleShipmentResource(string ShipmentCode, int OrderId, DateTimeOffset ScheduledAt);
