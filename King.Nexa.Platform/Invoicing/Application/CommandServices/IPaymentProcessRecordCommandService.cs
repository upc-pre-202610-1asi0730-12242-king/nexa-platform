using King.Nexa.Platform.Invoicing.Domain.Model.Entities;

namespace King.Nexa.Platform.Invoicing.Application.CommandServices;

public interface IPaymentProcessRecordCommandService
{
    Task<PaymentProcessRecord> CreateAsync(PaymentProcessRecord record, CancellationToken cancellationToken = default);
    Task<PaymentProcessRecord?> ChangeStatusAsync(ChangePaymentProcessStatusCommand command, CancellationToken cancellationToken = default);
}

public record ChangePaymentProcessStatusCommand(int Id, string Status);
