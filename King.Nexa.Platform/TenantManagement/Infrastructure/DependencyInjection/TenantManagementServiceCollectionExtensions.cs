using King.Nexa.Platform.TenantManagement.Application.CommandServices;
using King.Nexa.Platform.TenantManagement.Application.Internal.CommandServices;
using King.Nexa.Platform.TenantManagement.Application.Internal.QueryServices;
using King.Nexa.Platform.TenantManagement.Application.QueryServices;
using King.Nexa.Platform.TenantManagement.Domain.Repositories;
using King.Nexa.Platform.TenantManagement.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

namespace King.Nexa.Platform.TenantManagement.Infrastructure.DependencyInjection;

public static class TenantManagementServiceCollectionExtensions
{
    public static IServiceCollection AddTenantManagement(this IServiceCollection services)
    {
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IOrganizationRegistrationRequestRepository, OrganizationRegistrationRequestRepository>();
        services.AddScoped<ITenantAdministrationRepository, TenantAdministrationRepository>();
        services.AddScoped<ITenantQueryService, TenantQueryService>();
        services.AddScoped<IOrganizationRegistrationQueryService, OrganizationRegistrationQueryService>();
        services.AddScoped<ITenantCommandService, TenantCommandService>();
        services.AddScoped<IOrganizationRegistrationCommandService, OrganizationRegistrationCommandService>();
        services.AddScoped<ITenantAdministrationQueryService, TenantAdministrationQueryService>();
        services.AddScoped<ITenantAdministrationCommandService, TenantAdministrationCommandService>();
        return services;
    }
}
