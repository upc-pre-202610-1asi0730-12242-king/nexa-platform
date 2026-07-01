using King.Nexa.Platform.Invoicing.Application.QueryServices;
using King.Nexa.Platform.Invoicing.Domain.Model.Entities;
using King.Nexa.Platform.Invoicing.Domain.Repositories;

namespace King.Nexa.Platform.Invoicing.Application.Internal.QueryServices;

public class PaymentMethodRecordQueryService(
    IPaymentMethodRecordRepository paymentMethodRepository) : IPaymentMethodRecordQueryService
{
    public Task<IEnumerable<PaymentMethodRecord>> ListAsync(CancellationToken cancellationToken = default) =>
        paymentMethodRepository.ListAsync(cancellationToken);

    public Task<PaymentMethodRecord?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        paymentMethodRepository.FindByIdAsync(id, cancellationToken);
}
