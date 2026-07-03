namespace King.Nexa.Platform.Invoicing.Interfaces.Rest.Resources;

public record StripePaymentPreparationResource(
    int? PaymentId,
    int? InvoiceId,
    int? OrderId,
    decimal? Amount,
    string? Currency,
    string? SuccessUrl,
    string? CancelUrl);

public record StripePaymentPreparationResponseResource(
    bool Configured,
    bool Ready,
    string Status,
    string Message,
    string? CheckoutUrl,
    string? ClientSecret);
