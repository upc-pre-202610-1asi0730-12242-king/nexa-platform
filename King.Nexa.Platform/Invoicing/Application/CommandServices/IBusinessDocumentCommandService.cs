using King.Nexa.Platform.Invoicing.Domain.Model.Entities;

namespace King.Nexa.Platform.Invoicing.Application.CommandServices;

public interface IBusinessDocumentCommandService
{
    Task<BusinessDocument> CreateAsync(BusinessDocument document, CancellationToken cancellationToken = default);
    Task<BusinessDocument> UploadAsync(UploadBusinessDocumentCommand command, CancellationToken cancellationToken = default);
    Task<BusinessDocument> GenerateAsync(GenerateBusinessDocumentCommand command, CancellationToken cancellationToken = default);
    Task<BusinessDocument?> ChangeStatusAsync(ChangeBusinessDocumentStatusCommand command, CancellationToken cancellationToken = default);
    Task<BusinessDocument?> MarkReadyAsync(int id, CancellationToken cancellationToken = default);
    Task<BusinessDocument?> AuthorizeBuyerAsync(int id, CancellationToken cancellationToken = default);
    Task<BusinessDocument?> MarkMissingAsync(int id, CancellationToken cancellationToken = default);
}

public record GenerateBusinessDocumentCommand(int OrderId, string Type);

public record UploadBusinessDocumentCommand(
    int TenantId,
    int? OrderId,
    int? ClientAccountId,
    string Type,
    string Label,
    bool VisibleToBuyer,
    bool Required,
    IFormFile File);

public record ChangeBusinessDocumentStatusCommand(int Id, string Status, bool? VisibleToBuyer = null);
