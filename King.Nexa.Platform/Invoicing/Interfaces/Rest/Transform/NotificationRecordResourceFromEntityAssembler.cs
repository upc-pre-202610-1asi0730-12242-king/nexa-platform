using King.Nexa.Platform.Invoicing.Domain.Model.Entities;
using King.Nexa.Platform.Invoicing.Interfaces.Rest.Resources;

namespace King.Nexa.Platform.Invoicing.Interfaces.Rest.Transform;

public static class NotificationRecordResourceFromEntityAssembler
{
    public static NotificationRecordResource ToResourceFromEntity(NotificationRecord entity) =>
        new(
            entity.Id,
            entity.TenantId,
            entity.ClientAccountId,
            entity.RecipientRole,
            entity.Type,
            entity.Title,
            entity.Body,
            entity.Read,
            entity.CreatedAt,
            entity.UpdatedAt);

    public static NotificationRecord ToEntityFromResource(UpsertNotificationRecordResource resource) =>
        new()
        {
            TenantId = resource.TenantId,
            ClientAccountId = resource.ClientAccountId,
            RecipientRole = resource.RecipientRole,
            Type = resource.Type,
            Title = resource.Title,
            Body = resource.Body,
            Read = resource.Read
        };
}
