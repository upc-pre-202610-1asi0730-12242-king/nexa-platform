using King.Nexa.Platform.Invoicing.Application.QueryServices;
using King.Nexa.Platform.Invoicing.Domain.Model.Entities;
using King.Nexa.Platform.Invoicing.Domain.Repositories;

namespace King.Nexa.Platform.Invoicing.Application.Internal.QueryServices;

public class BusinessDocumentQueryService(
    IBusinessDocumentRepository documentRepository,
    IConfiguration configuration) : IBusinessDocumentQueryService
{
    public Task<IEnumerable<BusinessDocument>> ListAsync(CancellationToken cancellationToken = default) =>
        documentRepository.ListAsync(cancellationToken);

    public Task<BusinessDocument?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        documentRepository.FindByIdAsync(id, cancellationToken);

    public async Task<BusinessDocumentContent?> GetContentAsync(int id, CancellationToken cancellationToken = default)
    {
        var document = await GetByIdAsync(id, cancellationToken);
        if (document is null || string.IsNullOrWhiteSpace(document.FileName)) return null;

        var storageRoot = configuration["Storage:DocumentsPath"]
            ?? Path.Combine(AppContext.BaseDirectory, "storage", "documents");
        var storedPath = Path.Combine(storageRoot, Path.GetFileName(document.FileName));
        if (!File.Exists(storedPath)) return null;

        var content = await File.ReadAllBytesAsync(storedPath, cancellationToken);
        var extension = Path.GetExtension(document.FileName).ToLowerInvariant();
        var contentType = extension switch
        {
            ".pdf" => "application/pdf",
            ".xml" => "application/xml",
            _ => "application/octet-stream"
        };
        var downloadName = $"{document.Type}{extension}";
        return new BusinessDocumentContent(content, downloadName, contentType, document.VisibleToBuyer);
    }

}
