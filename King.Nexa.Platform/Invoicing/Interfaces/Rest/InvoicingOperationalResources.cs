namespace King.Nexa.Platform.Invoicing.Interfaces.Rest;

public record UploadBusinessDocumentResource(
    int TenantId,
    int? OrderId,
    int? ClientAccountId,
    string Type,
    string Label,
    bool VisibleToBuyer,
    bool Required,
    IFormFile File);

public record ChangePaymentProcessStatusResource(string Status);

public record ChangeBusinessDocumentStatusResource(string Status, bool? VisibleToBuyer = null);
