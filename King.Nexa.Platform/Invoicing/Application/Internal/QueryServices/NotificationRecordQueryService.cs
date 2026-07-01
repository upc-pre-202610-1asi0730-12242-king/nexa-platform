using King.Nexa.Platform.Invoicing.Application.QueryServices;
using King.Nexa.Platform.Invoicing.Domain.Model.Entities;
using King.Nexa.Platform.Invoicing.Domain.Repositories;

namespace King.Nexa.Platform.Invoicing.Application.Internal.QueryServices;

public class NotificationRecordQueryService(
    INotificationRecordRepository notificationRepository) : INotificationRecordQueryService
{
    public Task<IEnumerable<NotificationRecord>> ListAsync(CancellationToken cancellationToken = default) =>
        notificationRepository.ListAsync(cancellationToken);

    public Task<NotificationRecord?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        notificationRepository.FindByIdAsync(id, cancellationToken);
}
