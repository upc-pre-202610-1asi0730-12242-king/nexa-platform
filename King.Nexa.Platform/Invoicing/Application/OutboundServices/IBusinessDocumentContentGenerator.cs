namespace King.Nexa.Platform.Invoicing.Application.OutboundServices;

public interface IBusinessDocumentContentGenerator
{
    Task<GeneratedBusinessDocumentContent> GenerateAsync(
        int tenantId,
        int orderId,
        string type,
        CancellationToken cancellationToken = default);
}

public record GeneratedBusinessDocumentContent(
    byte[] Content,
    string FileName,
    string ContentType,
    string Label,
    int ClientAccountId);
