using King.Nexa.Platform.Invoicing.Application.QueryServices;
using King.Nexa.Platform.Invoicing.Domain.Model.Entities;
using King.Nexa.Platform.Invoicing.Domain.Repositories;

namespace King.Nexa.Platform.Invoicing.Application.Internal.QueryServices;

public class PaymentProcessRecordQueryService(
    IPaymentProcessRecordRepository paymentProcessRepository) : IPaymentProcessRecordQueryService
{
    public Task<IEnumerable<PaymentProcessRecord>> ListAsync(CancellationToken cancellationToken = default) =>
        paymentProcessRepository.ListAsync(cancellationToken);

    public Task<PaymentProcessRecord?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        paymentProcessRepository.FindByIdAsync(id, cancellationToken);
}
