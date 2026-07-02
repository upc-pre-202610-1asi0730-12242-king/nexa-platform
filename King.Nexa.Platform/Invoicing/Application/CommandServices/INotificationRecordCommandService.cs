using King.Nexa.Platform.Invoicing.Domain.Model.Entities;

namespace King.Nexa.Platform.Invoicing.Application.CommandServices;

public interface INotificationRecordCommandService
{
    Task<NotificationRecord> CreateAsync(NotificationRecord notification, CancellationToken cancellationToken = default);
    Task<NotificationRecord?> UpdateAsync(int id, NotificationRecord draft, CancellationToken cancellationToken = default);
    Task<NotificationRecord?> MarkReadAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
