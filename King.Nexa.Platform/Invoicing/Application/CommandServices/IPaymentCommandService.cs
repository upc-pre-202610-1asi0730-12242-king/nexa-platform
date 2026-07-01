using King.Nexa.Platform.Invoicing.Domain.Model.Aggregates;
using King.Nexa.Platform.Invoicing.Domain.Model.Commands;

namespace King.Nexa.Platform.Invoicing.Application.CommandServices;

public interface IPaymentCommandService
{
    Task<Payment> CreateAsync(RegisterPaymentCommand command, CancellationToken cancellationToken = default);

    Task<Payment?> UpdateAsync(UpdatePaymentCommand command, CancellationToken cancellationToken = default);

    Task<Payment?> ConfirmAsync(ConfirmPaymentCommand command, CancellationToken cancellationToken = default);

    Task<Payment?> RejectAsync(RejectPaymentCommand command, CancellationToken cancellationToken = default);

    Task<Payment?> CancelAsync(CancelPaymentCommand command, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(DeletePaymentCommand command, CancellationToken cancellationToken = default);
}
