using King.Nexa.Platform.Invoicing.Domain.Model.Entities;

namespace King.Nexa.Platform.Invoicing.Application.QueryServices;

public interface IPaymentMethodRecordQueryService
{
    Task<IEnumerable<PaymentMethodRecord>> ListAsync(CancellationToken cancellationToken = default);
    Task<PaymentMethodRecord?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
