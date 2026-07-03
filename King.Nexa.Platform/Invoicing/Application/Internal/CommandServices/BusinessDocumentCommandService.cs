using King.Nexa.Platform.Invoicing.Application.CommandServices;
using King.Nexa.Platform.Invoicing.Domain.Model.Entities;
using King.Nexa.Platform.Invoicing.Domain.Repositories;
using King.Nexa.Platform.Invoicing.Application.OutboundServices;
using King.Nexa.Platform.Shared.Application.Auditing;
using King.Nexa.Platform.Shared.Application.Security;
using King.Nexa.Platform.Shared.Domain.Repositories;

namespace King.Nexa.Platform.Invoicing.Application.Internal.CommandServices;

public class BusinessDocumentCommandService(
    IBusinessDocumentRepository documentRepository,
    IInvoicingTenantReferenceRepository referenceRepository,
    IUnitOfWork unitOfWork,
    IBusinessDocumentContentGenerator contentGenerator,
    IConfiguration configuration,
    ICurrentWorkspaceContext workspaceContext,
    IAuditLogger auditLogger,
    ILogger<BusinessDocumentCommandService> logger) : IBusinessDocumentCommandService
{
    public async Task<BusinessDocument> CreateAsync(BusinessDocument document, CancellationToken cancellationToken = default)
    {
        var tenantId = workspaceContext.TenantId
            ?? throw new InvalidOperationException("Current tenant is required.");
        var typeKey = document.Type.Trim().ToLowerInvariant();
        var documentType = await referenceRepository.FindActiveDocumentTypeAsync(document.DocumentTypeId, typeKey, cancellationToken)
            ?? throw new InvalidOperationException("Document type is not active.");

        await ValidateRelationsAsync(tenantId, document.OrderId, document.ClientAccountId, cancellationToken);
        document.TenantId = tenantId;
        document.DocumentTypeId = documentType.Id;
        document.Type = documentType.Key;
        document.Label = document.Label.Trim();
        document.Status = "pending";
        document.FileName = document.FileName?.Trim() ?? string.Empty;
        await documentRepository.AddAsync(document, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        logger.LogInformation("Business document {DocumentId} created for tenant {TenantId}.", document.Id, tenantId);
        await auditLogger.RecordAsync(new AuditLogEntry("business_document.created", "business_document", document.Id.ToString(), TenantId: tenantId), cancellationToken);
        return document;
    }

    public async Task<BusinessDocument> UploadAsync(UploadBusinessDocumentCommand command, CancellationToken cancellationToken = default)
    {
        if (command.File.Length == 0) throw new InvalidOperationException("Document file is required.");
        var tenantId = workspaceContext.TenantId
            ?? throw new InvalidOperationException("Current tenant is required.");
        var typeKey = string.IsNullOrWhiteSpace(command.Type) ? "business_document" : command.Type.Trim().ToLowerInvariant();
        var documentType = await referenceRepository.FindActiveDocumentTypeAsync(null, typeKey, cancellationToken)
            ?? throw new InvalidOperationException("Document type is not active.");
        await ValidateRelationsAsync(tenantId, command.OrderId, command.ClientAccountId, cancellationToken);

        var storageRoot = configuration["Storage:DocumentsPath"]
            ?? Path.Combine(AppContext.BaseDirectory, "storage", "documents");
        Directory.CreateDirectory(storageRoot);

        var safeName = Path.GetFileName(command.File.FileName);
        var storedName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}-{safeName}";
        var storedPath = Path.Combine(storageRoot, storedName);
        await using (var stream = File.Create(storedPath))
        {
            await command.File.CopyToAsync(stream, cancellationToken);
        }

        var document = new BusinessDocument
        {
            TenantId = tenantId,
            OrderId = command.OrderId,
            ClientAccountId = command.ClientAccountId,
            DocumentTypeId = documentType.Id,
            Type = documentType.Key,
            Label = string.IsNullOrWhiteSpace(command.Label) ? documentType.Label : command.Label,
            Status = "uploaded",
            FileName = storedName,
            VisibleToBuyer = command.VisibleToBuyer,
            Required = command.Required
        };

        await documentRepository.AddAsync(document, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        logger.LogInformation("Business document {DocumentId} uploaded for tenant {TenantId}.", document.Id, document.TenantId);
        await auditLogger.RecordAsync(new AuditLogEntry("business_document.uploaded", "business_document", document.Id.ToString(), TenantId: document.TenantId), cancellationToken);
        return document;
    }

    public async Task<BusinessDocument> GenerateAsync(GenerateBusinessDocumentCommand command, CancellationToken cancellationToken = default)
    {
        var tenantId = workspaceContext.TenantId
            ?? throw new InvalidOperationException("Current tenant is required.");
        var typeKey = command.Type.Trim().ToLowerInvariant();
        var documentType = await referenceRepository.FindActiveDocumentTypeAsync(null, typeKey, cancellationToken)
            ?? throw new InvalidOperationException("Document type is not active.");
        var generated = await contentGenerator.GenerateAsync(tenantId, command.OrderId, typeKey, cancellationToken);

        var storageRoot = configuration["Storage:DocumentsPath"]
            ?? Path.Combine(AppContext.BaseDirectory, "storage", "documents");
        Directory.CreateDirectory(storageRoot);
        var storedPath = Path.Combine(storageRoot, Path.GetFileName(generated.FileName));
        await File.WriteAllBytesAsync(storedPath, generated.Content, cancellationToken);

        var document = await documentRepository.FindByOrderAndTypeAsync(command.OrderId, typeKey, cancellationToken);
        if (document is null)
        {
            document = new BusinessDocument
            {
                TenantId = tenantId,
                OrderId = command.OrderId,
                ClientAccountId = generated.ClientAccountId,
                DocumentTypeId = documentType.Id,
                Type = typeKey,
                Label = generated.Label,
                FileName = generated.FileName,
                Required = true,
                VisibleToBuyer = true
            };
            document.ChangeStatus("ready", true);
            await documentRepository.AddAsync(document, cancellationToken);
        }
        else
        {
            document.DocumentTypeId = documentType.Id;
            document.ClientAccountId = generated.ClientAccountId;
            document.Label = generated.Label;
            document.FileName = generated.FileName;
            if (!string.Equals(document.Status, "accepted", StringComparison.OrdinalIgnoreCase))
                document.ChangeStatus("ready", true);
            else
                document.VisibleToBuyer = true;
        }

        await unitOfWork.CompleteAsync(cancellationToken);
        logger.LogInformation("Business document {DocumentId} generated for order {OrderId} and tenant {TenantId}.", document.Id, command.OrderId, tenantId);
        await auditLogger.RecordAsync(
            new AuditLogEntry("business_document.generated", "business_document", document.Id.ToString(), $"{{\"type\":\"{typeKey}\",\"orderId\":{command.OrderId}}}", tenantId),
            cancellationToken);
        return document;
    }

    private async Task ValidateRelationsAsync(int tenantId, int? orderId, int? clientAccountId, CancellationToken cancellationToken)
    {
        if (orderId.HasValue)
        {
            var exists = await referenceRepository.OrderBelongsToTenantAsync(tenantId, orderId.Value, cancellationToken);
            if (!exists) throw new InvalidOperationException("Order does not belong to the current tenant.");
        }

        if (clientAccountId.HasValue)
        {
            var exists = await referenceRepository.ClientAccountBelongsToTenantAsync(tenantId, clientAccountId.Value, cancellationToken);
            if (!exists) throw new InvalidOperationException("Client account does not belong to the current tenant.");
        }
    }

    public Task<BusinessDocument?> ChangeStatusAsync(ChangeBusinessDocumentStatusCommand command, CancellationToken cancellationToken = default) =>
        ChangeStatusAsync(command.Id, command.Status, command.VisibleToBuyer, cancellationToken);

    public Task<BusinessDocument?> MarkReadyAsync(int id, CancellationToken cancellationToken = default) =>
        ChangeStatusAsync(id, "ready", null, cancellationToken);

    public Task<BusinessDocument?> AuthorizeBuyerAsync(int id, CancellationToken cancellationToken = default) =>
        ChangeStatusAsync(id, "ready", true, cancellationToken);

    public Task<BusinessDocument?> MarkMissingAsync(int id, CancellationToken cancellationToken = default) =>
        ChangeStatusAsync(id, "missing", false, cancellationToken);

    private async Task<BusinessDocument?> ChangeStatusAsync(int id, string status, bool? visibleToBuyer, CancellationToken cancellationToken)
    {
        var document = await documentRepository.FindByIdAsync(id, cancellationToken);
        if (document is null) return null;

        document.ChangeStatus(status, visibleToBuyer);
        await unitOfWork.CompleteAsync(cancellationToken);
        logger.LogInformation("Business document {DocumentId} status changed to {Status} for tenant {TenantId}.", document.Id, document.Status, document.TenantId);
        await auditLogger.RecordAsync(
            new AuditLogEntry("business_document.status_changed", "business_document", document.Id.ToString(), $"{{\"status\":\"{document.Status}\"}}", document.TenantId),
            cancellationToken);
        return document;
    }
}
