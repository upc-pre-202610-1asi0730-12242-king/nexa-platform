using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using King.Nexa.Platform.Invoicing.Application.CommandServices;

namespace King.Nexa.Platform.Invoicing.Infrastructure.Integration;

public class StripePaymentPreparationService(IConfiguration configuration) : IStripePaymentPreparationService
{
    private const int SignatureToleranceSeconds = 300;

    public Task<StripePaymentPreparationResult> PrepareCheckoutSessionAsync(
        StripePaymentPreparationCommand command,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(BuildPreparationResult(
            "checkout_session_not_created",
            "Stripe Checkout Sessions require server-side Stripe SDK integration. No payment was created or marked as paid."));
    }

    public Task<StripePaymentPreparationResult> PreparePaymentIntentAsync(
        StripePaymentPreparationCommand command,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(BuildPreparationResult(
            "payment_intent_not_created",
            "Stripe PaymentIntents require server-side Stripe SDK integration. No payment was created or marked as paid."));
    }

    public Task<bool> VerifyWebhookSignatureAsync(
        string payload,
        string signatureHeader,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var webhookSecret = configuration["Stripe:WebhookSecret"];
        if (string.IsNullOrWhiteSpace(webhookSecret) ||
            string.IsNullOrWhiteSpace(payload) ||
            string.IsNullOrWhiteSpace(signatureHeader))
            return Task.FromResult(false);

        var fields = signatureHeader
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(part => part.Split('=', 2))
            .Where(parts => parts.Length == 2)
            .GroupBy(parts => parts[0], StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Select(parts => parts[1]).ToArray(), StringComparer.OrdinalIgnoreCase);

        if (!fields.TryGetValue("t", out var timestamps) ||
            timestamps.FirstOrDefault() is not { } timestampText ||
            !long.TryParse(timestampText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var timestamp) ||
            Math.Abs(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - timestamp) > SignatureToleranceSeconds ||
            !fields.TryGetValue("v1", out var signatures) ||
            signatures.Length == 0)
            return Task.FromResult(false);

        var signedPayload = $"{timestamp}.{payload}";
        var expectedSignature = ComputeHmacSha256Hex(webhookSecret, signedPayload);
        var expectedBytes = Encoding.UTF8.GetBytes(expectedSignature);

        var valid = signatures.Any(signature =>
        {
            var actualBytes = Encoding.UTF8.GetBytes(signature);
            return actualBytes.Length == expectedBytes.Length &&
                   CryptographicOperations.FixedTimeEquals(actualBytes, expectedBytes);
        });

        return Task.FromResult(valid);
    }

    private StripePaymentPreparationResult BuildPreparationResult(string status, string message)
    {
        var secretKey = configuration["Stripe:SecretKey"];
        var configured = !string.IsNullOrWhiteSpace(secretKey);

        return new StripePaymentPreparationResult(
            configured,
            Ready: false,
            status,
            configured
                ? message
                : "STRIPE_SECRET_KEY is not configured. No payment was created or marked as paid.");
    }

    private static string ComputeHmacSha256Hex(string secret, string payload)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        var hash = HMACSHA256.HashData(keyBytes, payloadBytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
