using King.Nexa.Platform.Invoicing.Domain.Model.Entities;
using King.Nexa.Platform.Shared.Domain.Model.Entities;

namespace King.Nexa.Platform.Invoicing.Domain.Repositories;

public interface IBusinessDocumentRepository
{
    Task AddAsync(BusinessDocument document, CancellationToken cancellationToken = default);
    Task<BusinessDocument?> FindByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<BusinessDocument?> FindByOrderAndTypeAsync(int orderId, string type, CancellationToken cancellationToken = default);
    Task<IEnumerable<BusinessDocument>> ListAsync(CancellationToken cancellationToken = default);
}

public interface IPaymentMethodRecordRepository
{
    Task AddAsync(PaymentMethodRecord record, CancellationToken cancellationToken = default);
    Task<PaymentMethodRecord?> FindByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<PaymentMethodRecord>> ListAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<PaymentMethodRecord>> ListDefaultsAsync(int tenantId, int clientAccountId, int? excludedId, CancellationToken cancellationToken = default);
}

public interface IPaymentProcessRecordRepository
{
    Task AddAsync(PaymentProcessRecord record, CancellationToken cancellationToken = default);
    Task<PaymentProcessRecord?> FindByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<PaymentProcessRecord>> ListAsync(CancellationToken cancellationToken = default);
}

public interface INotificationRecordRepository
{
    Task AddAsync(NotificationRecord notification, CancellationToken cancellationToken = default);
    Task<NotificationRecord?> FindByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<NotificationRecord>> ListAsync(CancellationToken cancellationToken = default);
    void Remove(NotificationRecord notification);
}

public interface IInvoicingTenantReferenceRepository
{
    Task<DocumentType?> FindActiveDocumentTypeAsync(int? id, string key, CancellationToken cancellationToken = default);
    Task<bool> OrderBelongsToTenantAsync(int tenantId, int orderId, CancellationToken cancellationToken = default);
    Task<bool> ClientAccountBelongsToTenantAsync(int tenantId, int clientAccountId, CancellationToken cancellationToken = default);
    Task<bool> PaymentBelongsToTenantAsync(int tenantId, int paymentId, CancellationToken cancellationToken = default);
    Task<bool> PaymentOptionIsActiveAsync(int paymentOptionId, CancellationToken cancellationToken = default);
    Task<bool> PaymentMethodBelongsToTenantAsync(int tenantId, int paymentMethodRecordId, CancellationToken cancellationToken = default);
}

