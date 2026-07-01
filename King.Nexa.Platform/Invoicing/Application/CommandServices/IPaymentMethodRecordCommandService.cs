using King.Nexa.Platform.Invoicing.Domain.Model.Entities;

namespace King.Nexa.Platform.Invoicing.Application.CommandServices;

public interface IPaymentMethodRecordCommandService
{
    Task<PaymentMethodRecord> CreateAsync(PaymentMethodRecord record, CancellationToken cancellationToken = default);
    Task<PaymentMethodRecord?> ChangeStatusAsync(int id, string status, bool? isDefault, CancellationToken cancellationToken = default);
}
