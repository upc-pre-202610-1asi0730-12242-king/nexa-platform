using King.Nexa.Platform.Logistics.Application.CommandServices;
using King.Nexa.Platform.Logistics.Domain.Model.Entities;
using King.Nexa.Platform.Logistics.Domain.Repositories;
using King.Nexa.Platform.Shared.Application.Security;
using King.Nexa.Platform.Shared.Domain.Repositories;

namespace King.Nexa.Platform.Logistics.Application.Internal.CommandServices;

public class LogisticsOperationalRecordCommandService(
    ILogisticsOperationalRecordRepository recordRepository,
    IUnitOfWork unitOfWork,
    ICurrentWorkspaceContext workspaceContext) : ILogisticsOperationalRecordCommandService
{
    public async Task<DispatchEvent> CreateDispatchEventAsync(DispatchEvent dispatchEvent, CancellationToken cancellationToken = default)
    {
        var tenantId = CurrentTenantId();
        await EnsureDispatchBelongsToTenantAsync(dispatchEvent.DispatchOrderId, tenantId, cancellationToken);
        dispatchEvent.TenantId = tenantId;
        Normalize(dispatchEvent);
        await recordRepository.AddAsync(dispatchEvent, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return dispatchEvent;
    }

    public async Task<DispatchEvent?> UpdateDispatchEventAsync(int id, DispatchEvent draft, CancellationToken cancellationToken = default)
    {
        var dispatchEvent = await FindScopedAsync<DispatchEvent>(id, cancellationToken);
        if (dispatchEvent is null) return null;
        await EnsureDispatchBelongsToTenantAsync(draft.DispatchOrderId, dispatchEvent.TenantId, cancellationToken);
        dispatchEvent.DispatchOrderId = draft.DispatchOrderId;
        dispatchEvent.Status = draft.Status;
        dispatchEvent.Description = draft.Description;
        dispatchEvent.VisibleToBuyer = draft.VisibleToBuyer;
        dispatchEvent.UpdatedAt = DateTime.UtcNow;
        Normalize(dispatchEvent);
        await unitOfWork.CompleteAsync(cancellationToken);
        return dispatchEvent;
    }

    public Task<bool> DeleteDispatchEventAsync(int id, CancellationToken cancellationToken = default) =>
        DeleteScopedAsync<DispatchEvent>(id, cancellationToken);

    public async Task<ProofOfDeliveryRecord> CreateProofOfDeliveryAsync(ProofOfDeliveryRecord proof, CancellationToken cancellationToken = default)
    {
        var tenantId = CurrentTenantId();
        await EnsureDispatchBelongsToTenantAsync(proof.DispatchOrderId, tenantId, cancellationToken);
        proof.TenantId = tenantId;
        Normalize(proof);
        await recordRepository.AddAsync(proof, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return proof;
    }

    public async Task<ProofOfDeliveryRecord> CreateProofOfDeliveryForDispatchAsync(
        int dispatchOrderId,
        string receivedBy,
        DateTime? completedAt,
        bool photoReference,
        bool signatureReference,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        ValidateProof(receivedBy, photoReference, signatureReference);
        var tenantId = workspaceContext.TenantId ?? throw new InvalidOperationException("Current tenant is required.");
        await EnsureDispatchBelongsToTenantAsync(dispatchOrderId, tenantId, cancellationToken);
        var proof = await recordRepository.FindProofByDispatchAsync(tenantId, dispatchOrderId, cancellationToken);
        proof ??= new ProofOfDeliveryRecord
        {
            TenantId = tenantId,
            DispatchOrderId = dispatchOrderId
        };
        proof.ReceivedBy = receivedBy;
        proof.CompletedAt = completedAt ?? DateTime.UtcNow;
        proof.PhotoReference = photoReference;
        proof.SignatureReference = signatureReference;
        proof.Notes = notes ?? string.Empty;
        proof.Status = "completed";
        Normalize(proof);
        if (proof.Id == 0) await recordRepository.AddAsync(proof, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return proof;
    }

    public async Task<ProofOfDeliveryRecord?> CompleteProofOfDeliveryAsync(
        int id,
        string receivedBy,
        DateTime? completedAt,
        bool photoReference,
        bool signatureReference,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        ValidateProof(receivedBy, photoReference, signatureReference);
        var proof = await FindScopedAsync<ProofOfDeliveryRecord>(id, cancellationToken);
        if (proof is null) return null;
        proof.ReceivedBy = receivedBy;
        proof.CompletedAt = completedAt ?? DateTime.UtcNow;
        proof.PhotoReference = photoReference;
        proof.SignatureReference = signatureReference;
        proof.Notes = notes ?? proof.Notes;
        proof.Status = "completed";
        proof.UpdatedAt = DateTime.UtcNow;
        Normalize(proof);
        await unitOfWork.CompleteAsync(cancellationToken);
        return proof;
    }

    public async Task<ProofOfDeliveryRecord?> UpdateProofOfDeliveryAsync(int id, ProofOfDeliveryRecord draft, CancellationToken cancellationToken = default)
    {
        var proof = await FindScopedAsync<ProofOfDeliveryRecord>(id, cancellationToken);
        if (proof is null) return null;
        await EnsureDispatchBelongsToTenantAsync(draft.DispatchOrderId, proof.TenantId, cancellationToken);
        proof.DispatchOrderId = draft.DispatchOrderId;
        proof.ReceivedBy = draft.ReceivedBy;
        proof.CompletedAt = draft.CompletedAt;
        proof.PhotoReference = draft.PhotoReference;
        proof.SignatureReference = draft.SignatureReference;
        proof.Notes = draft.Notes;
        proof.Status = draft.Status;
        proof.UpdatedAt = DateTime.UtcNow;
        Normalize(proof);
        await unitOfWork.CompleteAsync(cancellationToken);
        return proof;
    }

    public Task<bool> DeleteProofOfDeliveryAsync(int id, CancellationToken cancellationToken = default) =>
        DeleteScopedAsync<ProofOfDeliveryRecord>(id, cancellationToken);

    public async Task<TemperatureLog> CreateTemperatureLogAsync(TemperatureLog temperatureLog, CancellationToken cancellationToken = default)
    {
        var tenantId = CurrentTenantId();
        await ValidateTemperatureScopeAsync(temperatureLog, tenantId, cancellationToken);
        temperatureLog.TenantId = tenantId;
        Normalize(temperatureLog);
        await recordRepository.AddAsync(temperatureLog, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return temperatureLog;
    }

    public async Task<TemperatureLog> CreateTemperatureLogForDispatchAsync(int dispatchOrderId, TemperatureLog draft, CancellationToken cancellationToken = default)
    {
        var tenantId = workspaceContext.TenantId ?? throw new InvalidOperationException("Current tenant is required.");
        var dispatch = await recordRepository.FindDispatchByIdAsync(tenantId, dispatchOrderId, cancellationToken)
            ?? throw new InvalidOperationException("Dispatch order does not belong to the current tenant.");
        draft.TenantId = tenantId;
        draft.DispatchOrderId = dispatch.Id;
        draft.OrderId = dispatch.OrderId;
        Normalize(draft);
        await recordRepository.AddAsync(draft, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return draft;
    }

    public async Task<TemperatureLog?> UpdateTemperatureLogAsync(int id, TemperatureLog draft, CancellationToken cancellationToken = default)
    {
        var log = await FindScopedAsync<TemperatureLog>(id, cancellationToken);
        if (log is null) return null;
        await ValidateTemperatureScopeAsync(draft, log.TenantId, cancellationToken);
        log.DispatchOrderId = draft.DispatchOrderId;
        log.OrderId = draft.OrderId;
        log.Celsius = draft.Celsius;
        log.Zone = draft.Zone;
        log.Status = draft.Status;
        log.RecordedAt = draft.RecordedAt == default ? log.RecordedAt : draft.RecordedAt;
        log.UpdatedAt = DateTime.UtcNow;
        Normalize(log);
        await unitOfWork.CompleteAsync(cancellationToken);
        return log;
    }

    public async Task<TemperatureLog?> ResolveTemperatureAlertAsync(int id, CancellationToken cancellationToken = default)
    {
        var log = await FindScopedAsync<TemperatureLog>(id, cancellationToken);
        if (log is null) return null;
        log.Status = "reviewed";
        log.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.CompleteAsync(cancellationToken);
        return log;
    }

    public Task<bool> DeleteTemperatureLogAsync(int id, CancellationToken cancellationToken = default) =>
        DeleteScopedAsync<TemperatureLog>(id, cancellationToken);

    private int CurrentTenantId() =>
        workspaceContext.TenantId ?? throw new InvalidOperationException("Current tenant is required.");

    private Task<T?> FindScopedAsync<T>(int id, CancellationToken cancellationToken) where T : class =>
        recordRepository.FindByIdAsync<T>(id, cancellationToken);

    private async Task<bool> DeleteScopedAsync<T>(int id, CancellationToken cancellationToken) where T : class
    {
        var entity = await FindScopedAsync<T>(id, cancellationToken);
        if (entity is null) return false;
        recordRepository.Remove(entity);
        await unitOfWork.CompleteAsync(cancellationToken);
        return true;
    }

    private async Task EnsureDispatchBelongsToTenantAsync(int dispatchOrderId, int tenantId, CancellationToken cancellationToken)
    {
        var exists = await recordRepository.DispatchBelongsToTenantAsync(tenantId, dispatchOrderId, cancellationToken);
        if (!exists) throw new InvalidOperationException("Dispatch order does not belong to the current tenant.");
    }

    private async Task ValidateTemperatureScopeAsync(TemperatureLog log, int tenantId, CancellationToken cancellationToken)
    {
        if (log.DispatchOrderId.HasValue)
            await EnsureDispatchBelongsToTenantAsync(log.DispatchOrderId.Value, tenantId, cancellationToken);
        if (log.OrderId.HasValue)
        {
            var exists = await recordRepository.OrderBelongsToTenantAsync(tenantId, log.OrderId.Value, cancellationToken);
            if (!exists) throw new InvalidOperationException("Order does not belong to the current tenant.");
        }
    }

    private static void Normalize(DispatchEvent dispatchEvent)
    {
        dispatchEvent.Status = dispatchEvent.Status.Trim().ToLowerInvariant();
        dispatchEvent.Description = dispatchEvent.Description.Trim();
    }

    private static void Normalize(ProofOfDeliveryRecord proof)
    {
        proof.ReceivedBy = proof.ReceivedBy.Trim();
        proof.Notes = proof.Notes?.Trim() ?? string.Empty;
        proof.Status = string.IsNullOrWhiteSpace(proof.Status) ? "pending" : proof.Status.Trim().ToLowerInvariant();
    }

    private static void Normalize(TemperatureLog log)
    {
        log.Status = string.IsNullOrWhiteSpace(log.Status)
            ? log.Celsius is < -30 or > 20 ? "alert" : "ok"
            : log.Status.Trim().ToLowerInvariant();
        log.Zone = log.Zone?.Trim() ?? string.Empty;
        log.RecordedAt = log.RecordedAt == default ? DateTime.UtcNow : log.RecordedAt;
    }

    private static void ValidateProof(string receivedBy, bool photoReference, bool signatureReference)
    {
        if (string.IsNullOrWhiteSpace(receivedBy)) throw new InvalidOperationException("Receiver is required.");
        if (!photoReference && !signatureReference) throw new InvalidOperationException("Proof of delivery requires photo or signature evidence.");
    }
}
