using King.Nexa.Platform.Shared.Domain.Repositories;
using King.Nexa.Platform.Shared.Application.Auditing;
using King.Nexa.Platform.Shared.Application.Security;
using King.Nexa.Platform.Shared.Infrastructure.Auditing;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Interceptors;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using King.Nexa.Platform.Shared.Infrastructure.Security.Authentication;
using King.Nexa.Platform.Shared.Infrastructure.Security.Authorization;
using King.Nexa.Platform.Shared.Infrastructure.Security.Context;
using King.Nexa.Platform.Shared.Infrastructure.Seed;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace King.Nexa.Platform.Shared.Infrastructure.DependencyInjection;

public static class SharedServiceCollectionExtensions
{
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddProblemDetails();
        services.AddHttpContextAccessor();
        services.AddSingleton(TimeProvider.System);
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection("Jwt"))
            .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer), "Jwt:Issuer is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Audience), "Jwt:Audience is required.")
            .Validate(options => options.ExpirationMinutes is > 0 and <= 1440, "Jwt:ExpirationMinutes must be between 1 and 1440.")
            .Validate(options =>
                    options.SigningKey.Length >= 32 &&
                    !options.SigningKey.Contains("replace-with", StringComparison.OrdinalIgnoreCase) &&
                    !options.SigningKey.Contains("change-me", StringComparison.OrdinalIgnoreCase),
                "Jwt:SigningKey must be a non-placeholder secret of at least 32 characters.")
            .ValidateOnStart();
        services.AddScoped<JwtTokenCodec>();
        services.AddScoped<ICurrentUserContext, HttpCurrentUserContext>();
        services.AddScoped<ICurrentWorkspaceContext, HttpCurrentWorkspaceContext>();
        services.AddAuthentication(NexaAuthenticationConstants.Scheme)
            .AddScheme<AuthenticationSchemeOptions, NexaJwtAuthenticationHandler>(
                NexaAuthenticationConstants.Scheme,
                _ => { });
        services.AddAuthorization(options =>
        {
            options.AddPolicy(NexaAuthorizationPolicies.WorkspaceMember, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim("tenant_id")
                    .RequireClaim("workspace_id")
                    .RequireClaim("membership_id"));
            options.AddPolicy(NexaAuthorizationPolicies.CanManageWorkspace, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim("membership_id")
                    .AddRequirements(new WorkspaceRoleRequirement(NexaAuthorizationPolicies.WorkspaceManagers)));
            options.AddPolicy(NexaAuthorizationPolicies.CanCreateOrder, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim("membership_id")
                    .AddRequirements(new WorkspaceRoleRequirement(NexaAuthorizationPolicies.SalesRoles)));
            options.AddPolicy(NexaAuthorizationPolicies.CanAcceptPurchaseRequest, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim("membership_id")
                    .AddRequirements(new WorkspaceRoleRequirement(NexaAuthorizationPolicies.SalesRoles)));
            options.AddPolicy(NexaAuthorizationPolicies.CanStartDispatch, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim("membership_id")
                    .AddRequirements(new WorkspaceRoleRequirement(NexaAuthorizationPolicies.LogisticsRoles)));
            options.AddPolicy(NexaAuthorizationPolicies.CanManageDocuments, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim("membership_id")
                    .AddRequirements(new WorkspaceRoleRequirement(NexaAuthorizationPolicies.DocumentRoles)));
            options.AddPolicy(NexaAuthorizationPolicies.CanManagePaymentMethods, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim("membership_id"));
            options.AddPolicy(NexaAuthorizationPolicies.CanManageCatalog, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim("membership_id")
                    .AddRequirements(new WorkspaceRoleRequirement(NexaAuthorizationPolicies.SalesRoles)));
            options.AddPolicy(NexaAuthorizationPolicies.CanManagePromotions, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim("membership_id")
                    .AddRequirements(new WorkspaceRoleRequirement(NexaAuthorizationPolicies.DocumentRoles)));
            options.AddPolicy(NexaAuthorizationPolicies.CanManageInventory, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim("membership_id")
                    .AddRequirements(new WorkspaceRoleRequirement(NexaAuthorizationPolicies.LogisticsRoles)));
            options.AddPolicy(NexaAuthorizationPolicies.CanManageSharedReferenceData, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim("membership_id")
                    .AddRequirements(new WorkspaceRoleRequirement(NexaAuthorizationPolicies.SharedReferenceDataManagers)));
        });
        services.AddScoped<IAuthorizationHandler, WorkspaceRoleAuthorizationHandler>();
        services.Configure<SeedDataOptions>(configuration.GetSection("SeedData"));
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            var connectionStringTemplate = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionStringTemplate))
                throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

            var connectionString = NormalizePostgresConnectionString(Environment.ExpandEnvironmentVariables(connectionStringTemplate));
            var environment = serviceProvider.GetRequiredService<IHostEnvironment>();
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

            options.UseNpgsql(connectionString)
                .UseLoggerFactory(loggerFactory)
                .EnableDetailedErrors()
                .AddInterceptors(serviceProvider.GetRequiredService<AuditableEntityInterceptor>());

            if (environment.IsDevelopment())
            {
                LogSafeDatabaseTarget(
                    connectionString,
                    serviceProvider.GetRequiredService<ILoggerFactory>()
                        .CreateLogger(nameof(SharedServiceCollectionExtensions)));
            }
        });

        services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ISeedDataService, SeedDataService>();
        services.AddScoped<IAuditLogger, EfAuditLogger>();
        services.AddScoped<IAuditLogQueryService, AuditLogQueryService>();

        return services;
    }

    private static void LogSafeDatabaseTarget(string connectionString, ILogger logger)
    {
        var builder = new DbConnectionStringBuilder { ConnectionString = connectionString };
        var server = GetConnectionStringValue(builder, "server", "host", "data source") ?? "configured server";
        var database = GetConnectionStringValue(builder, "database", "initial catalog") ?? "configured database";

        logger.LogInformation("Database configured for server {Server}, database {Database}.", server, database);
    }

    private static string? GetConnectionStringValue(DbConnectionStringBuilder builder, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (builder.TryGetValue(key, out var value) && value is not null)
                return value.ToString();
        }

        return null;
    }

    private static string NormalizePostgresConnectionString(string connectionString)
    {
        if (!Uri.TryCreate(connectionString, UriKind.Absolute, out var uri))
            return connectionString;

        if (uri.Scheme is not ("postgres" or "postgresql"))
            return connectionString;

        var credentials = uri.UserInfo.Split(':', 2);
        var username = credentials.Length > 0 ? Uri.UnescapeDataString(credentials[0]) : string.Empty;
        var password = credentials.Length > 1 ? Uri.UnescapeDataString(credentials[1]) : string.Empty;
        var port = uri.IsDefaultPort ? 5432 : uri.Port;
        var database = uri.AbsolutePath.TrimStart('/');

        return $"Host={uri.Host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
    }
}
