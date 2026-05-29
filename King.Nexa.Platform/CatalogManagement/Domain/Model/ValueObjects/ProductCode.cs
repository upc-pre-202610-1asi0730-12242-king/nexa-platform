namespace King.Nexa.Platform.CatalogManagement.Domain.Model.ValueObjects;

public sealed record ProductCode
{
    public ProductCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Product code is required.", nameof(value));
        Value = value.Trim().ToUpperInvariant();
    }

    public string Value { get; }

    public override string ToString() => Value;
}
