namespace King.Nexa.Platform.Invoicing.Interfaces.Rest.Resources;

public record NotificationRecordResource(
    int Id,
    int TenantId,
    int? ClientAccountId,
    string RecipientRole,
    string Type,
    string Title,
    string Body,
    bool Read,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record UpsertNotificationRecordResource(
    int TenantId,
    int? ClientAccountId,
    string RecipientRole,
    string Type,
    string Title,
    string Body,
    bool Read);
