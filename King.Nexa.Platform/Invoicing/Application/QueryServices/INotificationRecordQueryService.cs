using King.Nexa.Platform.Invoicing.Domain.Model.Entities;

namespace King.Nexa.Platform.Invoicing.Application.QueryServices;

public interface INotificationRecordQueryService
{
    Task<IEnumerable<NotificationRecord>> ListAsync(CancellationToken cancellationToken = default);
    Task<NotificationRecord?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
