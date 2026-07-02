namespace King.Nexa.Platform.Invoicing.Domain.Model.Commands;

public record RejectPaymentCommand(int PaymentId, string? Reason = null);
