using King.Nexa.Platform.CatalogManagement.Domain.Model.Aggregates;
using King.Nexa.Platform.CatalogManagement.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using King.Nexa.Platform.Iam.Domain.Model.Aggregates;
using King.Nexa.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using King.Nexa.Platform.Invoicing.Domain.Model.Aggregates;
using King.Nexa.Platform.Invoicing.Domain.Model.Entities;
using King.Nexa.Platform.Invoicing.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using King.Nexa.Platform.Logistics.Domain.Model.Aggregates;
using King.Nexa.Platform.Logistics.Domain.Model.Entities;
using King.Nexa.Platform.Logistics.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using King.Nexa.Platform.Sales.Domain.Model.Aggregates;
using King.Nexa.Platform.Sales.Domain.Model.Entities;
using King.Nexa.Platform.Sales.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using King.Nexa.Platform.TenantManagement.Domain.Model.Aggregates;
using King.Nexa.Platform.TenantManagement.Domain.Model.Entities;
using King.Nexa.Platform.TenantManagement.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using King.Nexa.Platform.Shared.Domain.Model.Entities;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using King.Nexa.Platform.Warehouse.Domain.Model.Aggregates;
using King.Nexa.Platform.Warehouse.Domain.Model.Entities;
using King.Nexa.Platform.Warehouse.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using Microsoft.EntityFrameworkCore;
using WarehouseAggregate = King.Nexa.Platform.Warehouse.Domain.Model.Aggregates.Warehouse;

namespace King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<CatalogItem> CatalogItems => Set<CatalogItem>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Brand> Brands => Set<Brand>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public DbSet<ClientAccount> ClientAccounts => Set<ClientAccount>();
    public DbSet<PurchaseRequest> PurchaseRequests => Set<PurchaseRequest>();
    public DbSet<PurchaseRequestLine> PurchaseRequestLines => Set<PurchaseRequestLine>();
    public DbSet<Promotion> Promotions => Set<Promotion>();
    public DbSet<PromotionCatalogItem> PromotionCatalogItems => Set<PromotionCatalogItem>();
    public DbSet<ConversationMessage> ConversationMessages => Set<ConversationMessage>();
    public DbSet<CreditRequest> CreditRequests => Set<CreditRequest>();

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantMember> TenantMembers => Set<TenantMember>();
    public DbSet<TenantRule> TenantRules => Set<TenantRule>();
    public DbSet<TenantCustomField> TenantCustomFields => Set<TenantCustomField>();
    public DbSet<TenantSubscription> TenantSubscriptions => Set<TenantSubscription>();
    public DbSet<WorkspaceFeature> WorkspaceFeatures => Set<WorkspaceFeature>();
    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<UserWorkspaceMembership> UserWorkspaceMemberships => Set<UserWorkspaceMembership>();
    public DbSet<WorkspacePreference> WorkspacePreferences => Set<WorkspacePreference>();
    public DbSet<OrganizationRegistrationRequest> OrganizationRegistrationRequests => Set<OrganizationRegistrationRequest>();

    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<InventoryLot> InventoryLots => Set<InventoryLot>();
    public DbSet<InventoryMovement> InventoryMovements => Set<InventoryMovement>();
    public DbSet<InventoryReservationRecord> InventoryReservations => Set<InventoryReservationRecord>();

    public DbSet<WarehouseAggregate> Warehouses => Set<WarehouseAggregate>();

    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<DispatchOrder> DispatchOrders => Set<DispatchOrder>();
    public DbSet<DispatchEvent> DispatchEvents => Set<DispatchEvent>();
    public DbSet<ProofOfDeliveryRecord> ProofOfDeliveryRecords => Set<ProofOfDeliveryRecord>();
    public DbSet<TemperatureLog> TemperatureLogs => Set<TemperatureLog>();
    public DbSet<CustomerPortalTask> CustomerPortalTasks => Set<CustomerPortalTask>();

    public DbSet<Invoice> Invoices => Set<Invoice>();

    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<BusinessDocument> BusinessDocuments => Set<BusinessDocument>();
    public DbSet<PaymentMethodRecord> PaymentMethodRecords => Set<PaymentMethodRecord>();
    public DbSet<PaymentProcessRecord> PaymentProcessRecords => Set<PaymentProcessRecord>();
    public DbSet<NotificationRecord> NotificationRecords => Set<NotificationRecord>();

    public DbSet<User> Users => Set<User>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<PaymentOption> PaymentOptions => Set<PaymentOption>();
    public DbSet<DocumentType> DocumentTypes => Set<DocumentType>();
    public DbSet<UnitOfMeasure> UnitsOfMeasure => Set<UnitOfMeasure>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Province> Provinces => Set<Province>();
    public DbSet<District> Districts => Set<District>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyCatalogManagementConfiguration();
        builder.ApplyTenantManagementConfiguration();
        builder.ApplySalesConfiguration();
        builder.ApplyWarehouseConfiguration();
        builder.ApplyLogisticsConfiguration();
        builder.ApplyInvoicingConfiguration();
        builder.ApplyIamConfiguration();

        var organizationRegistrationRequest = builder.Entity<OrganizationRegistrationRequest>();
        organizationRegistrationRequest.ToTable("organization_registration_requests");
        organizationRegistrationRequest.HasKey(row => row.Id);
        organizationRegistrationRequest.Property(row => row.ExternalId).HasMaxLength(120).IsRequired();
        organizationRegistrationRequest.Property(row => row.Status).HasMaxLength(60).IsRequired();
        organizationRegistrationRequest.Property(row => row.CompanyName).HasMaxLength(180).IsRequired();
        organizationRegistrationRequest.Property(row => row.WorkspaceName).HasMaxLength(140).IsRequired();
        organizationRegistrationRequest.Property(row => row.WorkspaceSlug).HasMaxLength(64).IsRequired();
        organizationRegistrationRequest.Property(row => row.AdminEmail).HasMaxLength(180).IsRequired();
        organizationRegistrationRequest.Property(row => row.PayloadJson).HasColumnType("jsonb").IsRequired();
        organizationRegistrationRequest.HasIndex(row => row.ExternalId).IsUnique();
        organizationRegistrationRequest.HasIndex(row => new { row.WorkspaceSlug, row.Status });
        organizationRegistrationRequest.HasIndex(row => row.AdminEmail);

        var auditLog = builder.Entity<AuditLog>();
        auditLog.ToTable("audit_logs");
        auditLog.HasKey(row => row.Id);
        auditLog.Property(row => row.Action).HasMaxLength(120).IsRequired();
        auditLog.Property(row => row.ResourceType).HasMaxLength(120).IsRequired();
        auditLog.Property(row => row.ResourceId).HasMaxLength(120).IsRequired();
        auditLog.Property(row => row.MetadataJson).HasColumnType("jsonb");
        auditLog.Property(row => row.IpAddress).HasMaxLength(80);
        auditLog.Property(row => row.UserAgent).HasMaxLength(360);
        auditLog.HasIndex(row => new { row.TenantId, row.Action, row.CreatedAt });
        auditLog.HasIndex(row => new { row.TenantId, row.ResourceType, row.ResourceId });
        auditLog.HasOne<Tenant>().WithMany().HasForeignKey(row => row.TenantId).OnDelete(DeleteBehavior.Cascade);

        var outboxMessage = builder.Entity<OutboxMessage>();
        outboxMessage.ToTable("outbox_messages");
        outboxMessage.HasKey(row => row.Id);
        outboxMessage.Property(row => row.Type).HasMaxLength(500).IsRequired();
        outboxMessage.Property(row => row.Payload).HasColumnType("jsonb").IsRequired();
        outboxMessage.Property(row => row.Error).HasMaxLength(2000);
        outboxMessage.HasIndex(row => new { row.ProcessedOnUtc, row.OccurredOnUtc });
        outboxMessage.HasIndex(row => new { row.TenantId, row.OccurredOnUtc });

        var paymentOption = builder.Entity<PaymentOption>();
        paymentOption.ToTable("payment_options");
        paymentOption.HasKey(row => row.Id);
        paymentOption.Property(row => row.Key).HasMaxLength(80).IsRequired();
        paymentOption.Property(row => row.Label).HasMaxLength(160).IsRequired();
        paymentOption.HasIndex(row => row.Key).IsUnique();

        var documentType = builder.Entity<DocumentType>();
        documentType.ToTable("document_types");
        documentType.HasKey(row => row.Id);
        documentType.Property(row => row.Key).HasMaxLength(80).IsRequired();
        documentType.Property(row => row.Label).HasMaxLength(160).IsRequired();
        documentType.HasIndex(row => row.Key).IsUnique();

        var unitOfMeasure = builder.Entity<UnitOfMeasure>();
        unitOfMeasure.ToTable("units_of_measure");
        unitOfMeasure.HasKey(row => row.Id);
        unitOfMeasure.Property(row => row.Key).HasMaxLength(40).IsRequired();
        unitOfMeasure.Property(row => row.Label).HasMaxLength(120).IsRequired();
        unitOfMeasure.HasIndex(row => row.Key).IsUnique();

        var country = builder.Entity<Country>();
        country.ToTable("countries");
        country.HasKey(row => row.Id);
        country.Property(row => row.Code).HasMaxLength(8).IsRequired();
        country.Property(row => row.Label).HasMaxLength(120).IsRequired();
        country.HasIndex(row => row.Code).IsUnique();

        var department = builder.Entity<Department>();
        department.ToTable("departments");
        department.HasKey(row => row.Id);
        department.Property(row => row.Code).HasMaxLength(40).IsRequired();
        department.Property(row => row.Label).HasMaxLength(120).IsRequired();
        department.HasIndex(row => row.Code).IsUnique();
        department.HasOne<Country>().WithMany().HasForeignKey(row => row.CountryId).OnDelete(DeleteBehavior.Restrict);

        var province = builder.Entity<Province>();
        province.ToTable("provinces");
        province.HasKey(row => row.Id);
        province.Property(row => row.Code).HasMaxLength(40).IsRequired();
        province.Property(row => row.Label).HasMaxLength(120).IsRequired();
        province.HasIndex(row => row.Code).IsUnique();
        province.HasOne<Department>().WithMany().HasForeignKey(row => row.DepartmentId).OnDelete(DeleteBehavior.Restrict);

        var district = builder.Entity<District>();
        district.ToTable("districts");
        district.HasKey(row => row.Id);
        district.Property(row => row.Code).HasMaxLength(60).IsRequired();
        district.Property(row => row.Label).HasMaxLength(120).IsRequired();
        district.HasIndex(row => row.Code).IsUnique();
        district.HasOne<Province>().WithMany().HasForeignKey(row => row.ProvinceId).OnDelete(DeleteBehavior.Restrict);

        builder.UseSnakeCasePluralNamingConvention();
    }
}
