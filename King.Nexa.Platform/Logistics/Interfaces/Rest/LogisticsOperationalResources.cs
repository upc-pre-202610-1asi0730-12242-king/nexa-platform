namespace King.Nexa.Platform.Logistics.Interfaces.Rest;

public record AssignDispatchResource(string Responsible);

public record ScheduleDispatchResource(DateTime Eta, string DeliveryWindow, string? Note);

public record DispatchNoteResource(string? Note);

public record CompletePodResource(
    string ReceivedBy,
    DateTime? CompletedAt,
    bool PhotoReference,
    bool SignatureReference,
    string? Notes);

public record CreateDispatchOrderResource(int? ClientAccountId, string? Code, string? RouteName);

public record DispatchStatusChangeResource(string Status, string? Note, bool? VisibleToBuyer);
