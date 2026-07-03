using King.Nexa.Platform.Logistics.Application.Internal.CommandServices;
using King.Nexa.Platform.Logistics.Application.Internal.QueryServices;
using King.Nexa.Platform.Logistics.Application.CommandServices;
using King.Nexa.Platform.Logistics.Application.QueryServices;
using King.Nexa.Platform.Logistics.Domain.Repositories;
using King.Nexa.Platform.Logistics.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using King.Nexa.Platform.Logistics.Infrastructure.Integration;
using King.Nexa.Platform.Sales.Application.OutboundServices;

namespace King.Nexa.Platform.Logistics.Infrastructure.DependencyInjection;

public static class LogisticsServiceCollectionExtensions
{
    public static IServiceCollection AddLogistics(this IServiceCollection services)
    {
        services.AddScoped<IShipmentRepository, ShipmentRepository>();
        services.AddScoped<IShipmentCommandService, ShipmentCommandService>();
        services.AddScoped<IShipmentQueryService, ShipmentQueryService>();
        services.AddScoped<IDispatchOrderRepository, DispatchOrderRepository>();
        services.AddScoped<ILogisticsReferenceRepository, LogisticsReferenceRepository>();
        services.AddScoped<ILogisticsSalesReturnRepository, LogisticsSalesReturnRepository>();
        services.AddScoped<ILogisticsOperationalRecordRepository, LogisticsOperationalRecordRepository>();
        services.AddScoped<IDispatchOrderCommandService, DispatchOrderCommandService>();
        services.AddScoped<IDispatchOrderQueryService, DispatchOrderQueryService>();
        services.AddScoped<ILogisticsOperationalRecordCommandService, LogisticsOperationalRecordCommandService>();
        services.AddScoped<ILogisticsOperationalRecordQueryService, LogisticsOperationalRecordQueryService>();
        services.AddScoped<IOrderFulfillmentHandoff, SalesOrderFulfillmentHandoff>();

        return services;
    }
}
