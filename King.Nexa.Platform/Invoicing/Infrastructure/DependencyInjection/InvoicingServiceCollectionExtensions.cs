using King.Nexa.Platform.Invoicing.Application.Internal.CommandServices;
using King.Nexa.Platform.Invoicing.Application.Internal.QueryServices;
using King.Nexa.Platform.Invoicing.Application.CommandServices;
using King.Nexa.Platform.Invoicing.Application.QueryServices;
using King.Nexa.Platform.Invoicing.Infrastructure.Integration;
using King.Nexa.Platform.Sales.Application.OutboundServices;
using King.Nexa.Platform.Invoicing.Domain.Repositories;
using King.Nexa.Platform.Invoicing.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using King.Nexa.Platform.Invoicing.Application.OutboundServices;

namespace King.Nexa.Platform.Invoicing.Infrastructure.DependencyInjection;

public static class InvoicingServiceCollectionExtensions
{
    public static IServiceCollection AddInvoicing(this IServiceCollection services)
    {
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IInvoiceCommandService, InvoiceCommandService>();
        services.AddScoped<IInvoiceQueryService, InvoiceQueryService>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IPaymentCommandService, PaymentCommandService>();
        services.AddScoped<IPaymentQueryService, PaymentQueryService>();
        services.AddScoped<IBusinessDocumentRepository, BusinessDocumentRepository>();
        services.AddScoped<IPaymentMethodRecordRepository, PaymentMethodRecordRepository>();
        services.AddScoped<IPaymentProcessRecordRepository, PaymentProcessRecordRepository>();
        services.AddScoped<INotificationRecordRepository, NotificationRecordRepository>();
        services.AddScoped<IInvoicingTenantReferenceRepository, InvoicingTenantReferenceRepository>();
        services.AddScoped<IBusinessDocumentCommandService, BusinessDocumentCommandService>();
        services.AddScoped<IBusinessDocumentContentGenerator, BusinessDocumentContentGenerator>();
        services.AddScoped<IBusinessDocumentQueryService, BusinessDocumentQueryService>();
        services.AddScoped<IPaymentMethodRecordCommandService, PaymentMethodRecordCommandService>();
        services.AddScoped<IPaymentMethodRecordQueryService, PaymentMethodRecordQueryService>();
        services.AddScoped<IPaymentProcessRecordCommandService, PaymentProcessRecordCommandService>();
        services.AddScoped<IPaymentProcessRecordQueryService, PaymentProcessRecordQueryService>();
        services.AddScoped<INotificationRecordCommandService, NotificationRecordCommandService>();
        services.AddScoped<INotificationRecordQueryService, NotificationRecordQueryService>();
        services.AddScoped<IOrderDocumentProvisioner, SalesOrderDocumentProvisioner>();
        services.AddScoped<IStripePaymentPreparationService, StripePaymentPreparationService>();

        return services;
    }
}
