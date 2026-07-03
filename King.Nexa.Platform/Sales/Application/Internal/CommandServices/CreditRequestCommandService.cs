using King.Nexa.Platform.Sales.Application.CommandServices;
using King.Nexa.Platform.Sales.Domain.Model.Entities;
using King.Nexa.Platform.Sales.Domain.Repositories;
using King.Nexa.Platform.Shared.Application.Auditing;
using King.Nexa.Platform.Shared.Application.Security;
using King.Nexa.Platform.Shared.Domain.Repositories;

namespace King.Nexa.Platform.Sales.Application.Internal.CommandServices;

public class CreditRequestCommandService(
    ICreditRequestRepository repository,
    IUnitOfWork unitOfWork,
    ICurrentWorkspaceContext workspaceContext,
    IAuditLogger auditLogger) : ICreditRequestCommandService
{
    public async Task<CreditRequest> CreateAsync(CreditRequest entity, CancellationToken cancellationToken = default)
    {
        var tenantId = TenantId();
        entity.TenantId = tenantId;
        entity.ClientAccountId = await ResolveAccessibleClientIdAsync(entity.ClientAccountId, tenantId, cancellationToken);
        entity.Status = "submitted";
        entity.Validate();
        await repository.AddAsync(entity, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        await auditLogger.RecordAsync(new AuditLogEntry("credit_request.created", "credit_request", entity.Id.ToString(), TenantId: entity.TenantId), cancellationToken);
        return entity;
    }

    public async Task<CreditRequest?> ResolveAsync(int id, string status, string reviewedBy, string note, CancellationToken cancellationToken = default)
    {
        var entity = await repository.FindAsync(TenantId(), id, cancellationToken: cancellationToken);
        if (entity is null) return null;
        entity.Resolve(status, reviewedBy, note);
        await unitOfWork.CompleteAsync(cancellationToken);
        await auditLogger.RecordAsync(new AuditLogEntry("credit_request.resolved", "credit_request", entity.Id.ToString(), $"{{\"status\":\"{entity.Status}\"}}", entity.TenantId), cancellationToken);
        return entity;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await repository.FindAsync(TenantId(), id, cancellationToken: cancellationToken);
        if (entity is null) return false;
        repository.Remove(entity);
        await unitOfWork.CompleteAsync(cancellationToken);
        return true;
    }

    private int TenantId() => workspaceContext.TenantId ?? throw new InvalidOperationException("Current tenant is required.");

    private async Task<int> ResolveAccessibleClientIdAsync(int clientAccountId, int tenantId, CancellationToken cancellationToken)
    {
        if (workspaceContext.ClientAccountId is { } buyerClientAccountId)
        {
            if (clientAccountId > 0 && clientAccountId != buyerClientAccountId)
                throw new WorkspaceAccessDeniedException("The client account does not belong to the current buyer session.");
            clientAccountId = buyerClientAccountId;
        }

        if (clientAccountId <= 0) throw new InvalidOperationException("Client account is required.");
        var exists = await repository.ClientBelongsToTenantAsync(tenantId, clientAccountId, cancellationToken);
        if (!exists) throw new InvalidOperationException("Client account does not belong to the current tenant.");
        return clientAccountId;
    }
}
