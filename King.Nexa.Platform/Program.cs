using King.Nexa.Platform.CatalogManagement.Infrastructure.DependencyInjection;
using King.Nexa.Platform.Iam.Infrastructure.DependencyInjection;
using King.Nexa.Platform.Invoicing.Infrastructure.DependencyInjection;
using King.Nexa.Platform.Logistics.Infrastructure.DependencyInjection;
using King.Nexa.Platform.Sales.Infrastructure.DependencyInjection;
using King.Nexa.Platform.Shared.Infrastructure.DependencyInjection;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using King.Nexa.Platform.Shared.Infrastructure.Interfaces.AspNetCore.Configuration;
using King.Nexa.Platform.Shared.Infrastructure.Pipeline.Middleware.Extensions;
using King.Nexa.Platform.Shared.Infrastructure.Security.Middleware;
using King.Nexa.Platform.Shared.Infrastructure.Seed;
using King.Nexa.Platform.TenantManagement.Infrastructure.DependencyInjection;
using King.Nexa.Platform.Warehouse.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Npgsql;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

var renderPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(renderPort) && string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{renderPort}");
}

// Map uppercase/underscore environment variables to ASP.NET Core configuration keys
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                      ?? Environment.GetEnvironmentVariable("CONNECTION_STRINGS__DEFAULT_CONNECTION")
                      ?? Environment.GetEnvironmentVariable("CONNECTIONSTRINGS__DEFAULTCONNECTION")
                      ?? NormalizeDatabaseUrl(Environment.GetEnvironmentVariable("DATABASE_URL"));
if (!string.IsNullOrEmpty(connectionString))
{
    builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;
}

var seedDataEnabled = Environment.GetEnvironmentVariable("SEED_DEMO_DATA")
                     ?? Environment.GetEnvironmentVariable("SEED_DATA__ENABLED")
                     ?? Environment.GetEnvironmentVariable("SEEDDATA__ENABLED");
if (!string.IsNullOrEmpty(seedDataEnabled))
{
    builder.Configuration["SeedData:Enabled"] = seedDataEnabled;
}

var allowedOrigins = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS")
                    ?? Environment.GetEnvironmentVariable("ALLOWED_ORIGINS__0")
                    ?? Environment.GetEnvironmentVariable("ALLOWEDORIGINS__0");
if (!string.IsNullOrEmpty(allowedOrigins))
{
    var origins = allowedOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    for (var index = 0; index < origins.Length; index++)
        builder.Configuration[$"AllowedOrigins:{index}"] = origins[index];
}

var jwtSecret = Environment.GetEnvironmentVariable("NEXA_JWT_SECRET");
if (!string.IsNullOrWhiteSpace(jwtSecret))
{
    builder.Configuration["Jwt:SigningKey"] = jwtSecret;
}

var jwtIssuer = Environment.GetEnvironmentVariable("NEXA_JWT_ISSUER");
if (!string.IsNullOrWhiteSpace(jwtIssuer))
{
    builder.Configuration["Jwt:Issuer"] = jwtIssuer;
}

var jwtAudience = Environment.GetEnvironmentVariable("NEXA_JWT_AUDIENCE");
if (!string.IsNullOrWhiteSpace(jwtAudience))
{
    builder.Configuration["Jwt:Audience"] = jwtAudience;
}

var stripeSecretKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY");
if (!string.IsNullOrWhiteSpace(stripeSecretKey))
{
    builder.Configuration["Stripe:SecretKey"] = stripeSecretKey;
}

var stripeWebhookSecret = Environment.GetEnvironmentVariable("STRIPE_WEBHOOK_SECRET");
if (!string.IsNullOrWhiteSpace(stripeWebhookSecret))
{
    builder.Configuration["Stripe:WebhookSecret"] = stripeWebhookSecret;
}

const string frontendCorsPolicy = "AllowFrontend";

builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddCors(options =>
{
    options.AddPolicy(frontendCorsPolicy, policy =>
    {
        var origins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        var finalOrigins = new List<string>();
        if (builder.Environment.IsDevelopment())
        {
            finalOrigins.AddRange([
                "http://localhost:5173",
                "https://localhost:5173",
                "http://127.0.0.1:5173",
                "https://127.0.0.1:5173"
            ]);
        }
        finalOrigins.AddRange(origins.Where(origin => !string.IsNullOrWhiteSpace(origin)));
        if (finalOrigins.Count > 0)
        {
            policy
                .WithOrigins(finalOrigins.Distinct(StringComparer.OrdinalIgnoreCase).ToArray())
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});

builder.Services.AddSharedInfrastructure(builder.Configuration);
builder.Services.AddTenantManagement();
builder.Services.AddCatalogManagement();
builder.Services.AddSales();
builder.Services.AddWarehouse();
builder.Services.AddLogistics();
builder.Services.AddInvoicing();
builder.Services.AddIam();

var dataProtection = builder.Services.AddDataProtection()
    .SetApplicationName(builder.Configuration["DataProtection:ApplicationName"] ?? "Nexa");
var dataProtectionKeysPath = builder.Configuration["DataProtection:KeysPath"]
    ?? Environment.GetEnvironmentVariable("DATA_PROTECTION__KEYS_PATH");
if (!string.IsNullOrWhiteSpace(dataProtectionKeysPath))
{
    dataProtection.PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath));
}

builder.Services.AddControllers(options =>
{
    options.Conventions.Add(new KebabCaseRouteNamingConvention());
});
builder.Services.AddLocalization();
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document, null)] = []
    });
    options.OperationFilter<PublicEndpointOperationFilter>();

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// Configure Database Context and route EF logs through the app logger pipeline.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        
        var applyMigrations = app.Environment.IsDevelopment() ||
                              app.Environment.IsEnvironment("Testing") ||
                              string.Equals(builder.Configuration["APPLY_MIGRATIONS_ON_STARTUP"], "true", StringComparison.OrdinalIgnoreCase);
        if (applyMigrations)
        {
            context.Database.Migrate();
            logger.LogInformation("Database migrations applied successfully.");
        }
        else
        {
            logger.LogInformation("Database migrations were skipped. Set APPLY_MIGRATIONS_ON_STARTUP=true to enable startup migrations.");
        }

        var seedDemoData = app.Environment.IsDevelopment() &&
                           string.Equals(builder.Configuration["SeedData:Enabled"], "true", StringComparison.OrdinalIgnoreCase);
        if (seedDemoData)
        {
            var seedDataService = services.GetRequiredService<ISeedDataService>();
            await seedDataService.SeedAsync();
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while applying database migrations.");
        throw;
    }
}

app.UseGlobalExceptionHandling();

if (app.Environment.IsDevelopment() ||
    string.Equals(builder.Configuration["ENABLE_SWAGGER"], "true", StringComparison.OrdinalIgnoreCase))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseCors(frontendCorsPolicy);
app.UseAuthentication();
app.UseMiddleware<WorkspaceMembershipValidationMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.MapGet("/health/live", () => Results.Ok(new { status = "Healthy" })).AllowAnonymous();
app.MapGet("/health/ready", async (AppDbContext context, CancellationToken cancellationToken) =>
{
    var canConnect = await context.Database.CanConnectAsync(cancellationToken);
    return canConnect
        ? Results.Ok(new { status = "Healthy" })
        : Results.Problem("Database is not reachable.", statusCode: StatusCodes.Status503ServiceUnavailable);
});

app.Run();

static string? NormalizeDatabaseUrl(string? databaseUrl)
{
    if (string.IsNullOrWhiteSpace(databaseUrl)) return null;
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':', 2);
    var builder = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port > 0 ? uri.Port : 5432,
        Database = uri.AbsolutePath.TrimStart('/'),
        Username = Uri.UnescapeDataString(userInfo.ElementAtOrDefault(0) ?? string.Empty),
        Password = Uri.UnescapeDataString(userInfo.ElementAtOrDefault(1) ?? string.Empty),
        SslMode = SslMode.Require
    };
    return builder.ConnectionString;
}

public partial class Program;

