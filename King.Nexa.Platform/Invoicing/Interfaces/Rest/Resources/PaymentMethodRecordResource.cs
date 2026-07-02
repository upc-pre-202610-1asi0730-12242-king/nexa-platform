namespace King.Nexa.Platform.Invoicing.Interfaces.Rest.Resources;

public record PaymentMethodRecordResource(
    int Id,
    int TenantId,
    int ClientAccountId,
    string Type,
    string Label,
    string Status,
    bool IsDefault,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CreatePaymentMethodRecordResource(
    int ClientAccountId,
    string Type,
    string Label,
    bool IsDefault);

public record ChangePaymentMethodRecordStatusResource(string Status, bool? IsDefault);
