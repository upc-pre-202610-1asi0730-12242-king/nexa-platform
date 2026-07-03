namespace King.Nexa.Platform.Sales.Domain.Model.ValueObjects;

public class DeliveryDetails
{
    private DeliveryDetails()
    {
        AddressType = string.Empty;
        Address = string.Empty;
        District = string.Empty;
        City = string.Empty;
        Province = string.Empty;
        Reference = string.Empty;
        DispatchNote = string.Empty;
    }

    public DeliveryDetails(
        string? addressType,
        string? address,
        string? district,
        string? city,
        string? province,
        string? reference,
        DateOnly? requestedDate,
        string? dispatchNote)
    {
        AddressType = addressType?.Trim() ?? string.Empty;
        Address = address?.Trim() ?? string.Empty;
        District = district?.Trim() ?? string.Empty;
        City = city?.Trim() ?? string.Empty;
        Province = province?.Trim() ?? string.Empty;
        Reference = reference?.Trim() ?? string.Empty;
        RequestedDate = requestedDate;
        DispatchNote = dispatchNote?.Trim() ?? string.Empty;
    }

    public string AddressType { get; private set; }
    public string Address { get; private set; }
    public string District { get; private set; }
    public string City { get; private set; }
    public string Province { get; private set; }
    public string Reference { get; private set; }
    public DateOnly? RequestedDate { get; private set; }
    public string DispatchNote { get; private set; }

    public string FullAddress => string.Join(", ", new[]
    {
        string.Join(" ", new[] { AddressType, Address }.Where(value => !string.IsNullOrWhiteSpace(value))),
        District,
        Province,
        City
    }.Where(value => !string.IsNullOrWhiteSpace(value)));

    public static DeliveryDetails Empty() => new(null, null, null, null, null, null, null, null);
}
