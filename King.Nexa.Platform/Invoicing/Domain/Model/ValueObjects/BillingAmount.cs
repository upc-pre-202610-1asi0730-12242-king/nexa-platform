namespace King.Nexa.Platform.Invoicing.Domain.Model.ValueObjects;

public sealed record BillingAmount
{
    public BillingAmount(decimal amount, string currency)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "Billing amount must be positive.");
        if (string.IsNullOrWhiteSpace(currency)) throw new ArgumentException("Currency is required.", nameof(currency));

        var normalizedCurrency = currency.Trim().ToUpperInvariant();
        if (normalizedCurrency.Length != 3) throw new ArgumentException("Currency must use ISO 4217 code.", nameof(currency));

        Amount = amount;
        Currency = normalizedCurrency;
    }

    public BillingAmount(decimal amount) : this(amount, "PEN")
    {
    }

    public decimal Amount { get; }

    public string Currency { get; }
}

