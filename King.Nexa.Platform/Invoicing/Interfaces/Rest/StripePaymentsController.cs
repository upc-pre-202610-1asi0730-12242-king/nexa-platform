using King.Nexa.Platform.Invoicing.Application.CommandServices;
using King.Nexa.Platform.Invoicing.Interfaces.Rest.Resources;
using King.Nexa.Platform.Invoicing.Interfaces.Rest.Transform;
using King.Nexa.Platform.Shared.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace King.Nexa.Platform.Invoicing.Interfaces.Rest;

[ApiController]
[Authorize(Policy = NexaAuthorizationPolicies.WorkspaceMember)]
[Route("api/v1/payments/stripe")]
public class StripePaymentsController(IStripePaymentPreparationService stripePayments) : ControllerBase
{
    [HttpPost("checkout-sessions")]
    [ProducesResponseType(typeof(StripePaymentPreparationResponseResource), StatusCodes.Status501NotImplemented)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateCheckoutSession(
        StripePaymentPreparationResource resource,
        CancellationToken cancellationToken)
    {
        var result = await stripePayments.PrepareCheckoutSessionAsync(
            StripePaymentPreparationAssembler.ToCommand(resource),
            cancellationToken);

        return StatusCode(
            result.Configured ? StatusCodes.Status501NotImplemented : StatusCodes.Status503ServiceUnavailable,
            StripePaymentPreparationAssembler.ToResource(result));
    }

    [HttpPost("payment-intents")]
    [ProducesResponseType(typeof(StripePaymentPreparationResponseResource), StatusCodes.Status501NotImplemented)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreatePaymentIntent(
        StripePaymentPreparationResource resource,
        CancellationToken cancellationToken)
    {
        var result = await stripePayments.PreparePaymentIntentAsync(
            StripePaymentPreparationAssembler.ToCommand(resource),
            cancellationToken);

        return StatusCode(
            result.Configured ? StatusCodes.Status501NotImplemented : StatusCodes.Status503ServiceUnavailable,
            StripePaymentPreparationAssembler.ToResource(result));
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Webhook(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(HttpContext.RequestServices.GetRequiredService<IConfiguration>()["Stripe:WebhookSecret"]))
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "STRIPE_WEBHOOK_SECRET is not configured." });

        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync(cancellationToken);
        var signature = Request.Headers["Stripe-Signature"].ToString();

        if (!await stripePayments.VerifyWebhookSignatureAsync(payload, signature, cancellationToken))
            return Unauthorized(new { message = "Invalid Stripe webhook signature." });

        return Accepted(new
        {
            received = true,
            processed = false,
            message = "Stripe webhook signature verified. Event-specific payment state handling is pending."
        });
    }
}
