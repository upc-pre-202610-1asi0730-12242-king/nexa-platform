using King.Nexa.Platform.Invoicing.Domain.Model.Entities;

namespace King.Nexa.Platform.Invoicing.Application.QueryServices;

public interface IBusinessDocumentQueryService
{
    Task<IEnumerable<BusinessDocument>> ListAsync(CancellationToken cancellationToken = default);
    Task<BusinessDocument?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<BusinessDocumentContent?> GetContentAsync(int id, CancellationToken cancellationToken = default);
}

public record BusinessDocumentContent(byte[] Content, string FileName, string ContentType, bool VisibleToBuyer);
