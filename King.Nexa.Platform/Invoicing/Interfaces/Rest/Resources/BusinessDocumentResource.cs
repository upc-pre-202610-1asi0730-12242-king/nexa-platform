namespace King.Nexa.Platform.Invoicing.Interfaces.Rest.Resources;

public record BusinessDocumentResource(
    int Id,
    int TenantId,
    int? OrderId,
    int? ClientAccountId,
    int? DocumentTypeId,
    string Type,
    string Label,
    string Status,
    string? FileName,
    bool VisibleToBuyer,
    bool Required,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CreateBusinessDocumentResource(
    int? OrderId,
    int? ClientAccountId,
    int? DocumentTypeId,
    string Type,
    string Label,
    bool VisibleToBuyer,
    bool Required,
    string? FileName = null);

public record GenerateBusinessDocumentResource(int OrderId, string Type);
