using King.Nexa.Platform.Invoicing.Domain.Model.Entities;

namespace King.Nexa.Platform.Invoicing.Application.QueryServices;

public interface IPaymentProcessRecordQueryService
{
    Task<IEnumerable<PaymentProcessRecord>> ListAsync(CancellationToken cancellationToken = default);
    Task<PaymentProcessRecord?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
