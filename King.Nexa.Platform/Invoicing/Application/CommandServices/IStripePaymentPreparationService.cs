namespace King.Nexa.Platform.Invoicing.Application.CommandServices;

public interface IStripePaymentPreparationService
{
    Task<StripePaymentPreparationResult> PrepareCheckoutSessionAsync(
        StripePaymentPreparationCommand command,
        CancellationToken cancellationToken = default);

    Task<StripePaymentPreparationResult> PreparePaymentIntentAsync(
        StripePaymentPreparationCommand command,
        CancellationToken cancellationToken = default);

    Task<bool> VerifyWebhookSignatureAsync(
        string payload,
        string signatureHeader,
        CancellationToken cancellationToken = default);
}

public record StripePaymentPreparationCommand(
    int? PaymentId,
    int? InvoiceId,
    int? OrderId,
    decimal? Amount,
    string? Currency,
    string? SuccessUrl,
    string? CancelUrl);

public record StripePaymentPreparationResult(
    bool Configured,
    bool Ready,
    string Status,
    string Message,
    string? CheckoutUrl = null,
    string? ClientSecret = null);
