using King.Nexa.Platform.Sales.Application.Internal.CommandServices;
using King.Nexa.Platform.Sales.Application.Internal.QueryServices;
using King.Nexa.Platform.Sales.Application.CommandServices;
using King.Nexa.Platform.Sales.Application.QueryServices;
using King.Nexa.Platform.Sales.Domain.Repositories;
using King.Nexa.Platform.Sales.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

namespace King.Nexa.Platform.Sales.Infrastructure.DependencyInjection;

public static class SalesServiceCollectionExtensions
{
    public static IServiceCollection AddSales(this IServiceCollection services)
    {
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderCommandService, OrderCommandService>();
        services.AddScoped<IOrderQueryService, OrderQueryService>();
        services.AddScoped<IClientAccountRepository, ClientAccountRepository>();
        services.AddScoped<IClientAccountCommandService, ClientAccountCommandService>();
        services.AddScoped<IClientAccountQueryService, ClientAccountQueryService>();
        services.AddScoped<IPurchaseRequestRepository, PurchaseRequestRepository>();
        services.AddScoped<IPurchaseRequestCommandService, PurchaseRequestCommandService>();
        services.AddScoped<IPurchaseRequestQueryService, PurchaseRequestQueryService>();
        services.AddScoped<IPromotionRepository, PromotionRepository>();
        services.AddScoped<IPromotionCommandService, PromotionCommandService>();
        services.AddScoped<IPromotionQueryService, PromotionQueryService>();
        services.AddScoped<ICreditRequestRepository, CreditRequestRepository>();
        services.AddScoped<ICreditRequestCommandService, CreditRequestCommandService>();
        services.AddScoped<ICreditRequestQueryService, CreditRequestQueryService>();

        return services;
    }
}
