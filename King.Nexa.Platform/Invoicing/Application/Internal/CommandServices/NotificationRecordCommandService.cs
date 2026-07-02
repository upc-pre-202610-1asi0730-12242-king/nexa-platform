using King.Nexa.Platform.Invoicing.Application.CommandServices;
using King.Nexa.Platform.Invoicing.Domain.Model.Entities;
using King.Nexa.Platform.Invoicing.Domain.Repositories;
using King.Nexa.Platform.Shared.Application.Security;
using King.Nexa.Platform.Shared.Domain.Repositories;

namespace King.Nexa.Platform.Invoicing.Application.Internal.CommandServices;

public class NotificationRecordCommandService(
    INotificationRecordRepository notificationRepository,
    IUnitOfWork unitOfWork,
    ICurrentWorkspaceContext workspaceContext) : INotificationRecordCommandService
{
    public async Task<NotificationRecord> CreateAsync(NotificationRecord notification, CancellationToken cancellationToken = default)
    {
        notification.TenantId = CurrentTenantId();
        Normalize(notification);
        await notificationRepository.AddAsync(notification, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return notification;
    }

    public async Task<NotificationRecord?> UpdateAsync(int id, NotificationRecord draft, CancellationToken cancellationToken = default)
    {
        var notification = await FindScopedAsync(id, cancellationToken);
        if (notification is null) return null;
        notification.ClientAccountId = draft.ClientAccountId;
        notification.RecipientRole = draft.RecipientRole;
        notification.Type = draft.Type;
        notification.Title = draft.Title;
        notification.Body = draft.Body;
        notification.Read = draft.Read;
        notification.UpdatedAt = DateTime.UtcNow;
        Normalize(notification);
        await unitOfWork.CompleteAsync(cancellationToken);
        return notification;
    }

    public async Task<NotificationRecord?> MarkReadAsync(int id, CancellationToken cancellationToken = default)
    {
        var notification = await FindScopedAsync(id, cancellationToken);
        if (notification is null) return null;
        notification.Read = true;
        notification.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.CompleteAsync(cancellationToken);
        return notification;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var notification = await FindScopedAsync(id, cancellationToken);
        if (notification is null) return false;
        notificationRepository.Remove(notification);
        await unitOfWork.CompleteAsync(cancellationToken);
        return true;
    }

    private Task<NotificationRecord?> FindScopedAsync(int id, CancellationToken cancellationToken) =>
        notificationRepository.FindByIdAsync(id, cancellationToken);

    private int CurrentTenantId() =>
        workspaceContext.TenantId ?? throw new InvalidOperationException("Current tenant is required.");

    private static void Normalize(NotificationRecord notification)
    {
        notification.RecipientRole = string.IsNullOrWhiteSpace(notification.RecipientRole) ? "buyer" : notification.RecipientRole.Trim().ToLowerInvariant();
        notification.Type = string.IsNullOrWhiteSpace(notification.Type) ? "status" : notification.Type.Trim().ToLowerInvariant();
        notification.Title = notification.Title.Trim();
        notification.Body = notification.Body?.Trim() ?? string.Empty;
    }
}
