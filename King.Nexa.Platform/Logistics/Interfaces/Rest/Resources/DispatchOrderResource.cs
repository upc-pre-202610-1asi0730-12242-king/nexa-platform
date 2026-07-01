using King.Nexa.Platform.Logistics.Domain.Model.Entities;

namespace King.Nexa.Platform.Logistics.Interfaces.Rest.Resources;

public record DispatchOrderResource(
    int Id,
    int TenantId,
    int OrderId,
    int ClientAccountId,
    string Code,
    string Status,
    string RouteName,
    string Responsible,
    DateTime? Eta,
    string DeliveryWindow,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public class UpsertDispatchOrderResource
{
    public int TenantId { get; init; }
    public int OrderId { get; init; }
    public int ClientAccountId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string RouteName { get; init; } = string.Empty;
    public string Responsible { get; init; } = string.Empty;
    public DateTime? Eta { get; init; }
    public string DeliveryWindow { get; init; } = string.Empty;
}

public record DispatchTrackingResource(
    IEnumerable<DispatchOrderResource> DispatchOrders,
    IEnumerable<DispatchEventResource> Events,
    IEnumerable<TemperatureLogResource> TemperatureLogs,
    IEnumerable<ProofOfDeliveryRecordResource> ProofsOfDelivery);

public record DispatchEventResource(
    int Id,
    int TenantId,
    int DispatchOrderId,
    string Status,
    string Description,
    bool VisibleToBuyer,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record UpsertDispatchEventResource(
    int TenantId,
    int DispatchOrderId,
    string Status,
    string Description,
    bool VisibleToBuyer);

public record ProofOfDeliveryRecordResource(
    int Id,
    int TenantId,
    int DispatchOrderId,
    string ReceivedBy,
    DateTime? CompletedAt,
    bool PhotoReference,
    bool SignatureReference,
    string Notes,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record UpsertProofOfDeliveryRecordResource(
    int TenantId,
    int DispatchOrderId,
    string ReceivedBy,
    DateTime? CompletedAt,
    bool PhotoReference,
    bool SignatureReference,
    string Notes,
    string Status);

public record TemperatureLogResource(
    int Id,
    int TenantId,
    int? DispatchOrderId,
    int? OrderId,
    decimal Celsius,
    string Zone,
    string Status,
    DateTime RecordedAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record UpsertTemperatureLogResource(
    int TenantId,
    int? DispatchOrderId,
    int? OrderId,
    decimal Celsius,
    string Zone,
    string Status,
    DateTime? RecordedAt);
