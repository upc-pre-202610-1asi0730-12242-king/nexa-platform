namespace King.Nexa.Platform.Shared.Domain.Model.Events;

public sealed record PurchaseRequestSubmitted(int PurchaseRequestId, int TenantId) : DomainEvent;
public sealed record PurchaseRequestAccepted(int PurchaseRequestId, int TenantId, string OrderNumber) : DomainEvent;
public sealed record OrderConfirmed(int OrderId, int TenantId) : DomainEvent;
public sealed record InventoryReservationCreated(string ReservationCode, int TenantId, int InventoryItemId, int Units) : DomainEvent;
public sealed record DispatchStatusChanged(int DispatchOrderId, int TenantId, string Status) : DomainEvent;
public sealed record InvoicePaid(int InvoiceId, int TenantId) : DomainEvent;
public sealed record PaymentCompleted(int PaymentId, int TenantId) : DomainEvent;
public sealed record WorkspaceCreated(string Slug, int TenantId) : DomainEvent;
