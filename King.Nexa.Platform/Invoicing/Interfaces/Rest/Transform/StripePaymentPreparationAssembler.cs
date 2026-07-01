using King.Nexa.Platform.Invoicing.Application.CommandServices;
using King.Nexa.Platform.Invoicing.Interfaces.Rest.Resources;

namespace King.Nexa.Platform.Invoicing.Interfaces.Rest.Transform;

public static class StripePaymentPreparationAssembler
{
    public static StripePaymentPreparationCommand ToCommand(StripePaymentPreparationResource resource) =>
        new(
            resource.PaymentId,
            resource.InvoiceId,
            resource.OrderId,
            resource.Amount,
            resource.Currency,
            resource.SuccessUrl,
            resource.CancelUrl);

    public static StripePaymentPreparationResponseResource ToResource(StripePaymentPreparationResult result) =>
        new(
            result.Configured,
            result.Ready,
            result.Status,
            result.Message,
            result.CheckoutUrl,
            result.ClientSecret);
}
