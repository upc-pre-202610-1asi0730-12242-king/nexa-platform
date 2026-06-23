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
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// Map uppercase/underscore environment variables to ASP.NET Core configuration keys
var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRINGS__DEFAULT_CONNECTION") 
                      ?? Environment.GetEnvironmentVariable("CONNECTIONSTRINGS__DEFAULTCONNECTION");
if (!string.IsNullOrEmpty(connectionString))
{
    builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;
}

var seedDataEnabled = Environment.GetEnvironmentVariable("SEED_DATA__ENABLED")
                     ?? Environment.GetEnvironmentVariable("SEEDDATA__ENABLED");
if (!string.IsNullOrEmpty(seedDataEnabled))
{
    builder.Configuration["SeedData:Enabled"] = seedDataEnabled;
}

var allowedOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS__0")
                    ?? Environment.GetEnvironmentVariable("ALLOWEDORIGINS__0");
if (!string.IsNullOrEmpty(allowedOrigins))
{
    builder.Configuration["AllowedOrigins:0"] = allowedOrigins;
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

const string frontendCorsPolicy = "AllowFrontend";

builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddCors(options =>
{
    options.AddPolicy(frontendCorsPolicy, policy =>
    {
        var origins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        var finalOrigins = new List<string>
        {
            "http://localhost:5173",
            "https://localhost:5173",
            "http://127.0.0.1:5173",
            "https://127.0.0.1:5173"
        };
        if (origins.Length > 0)
        {
            finalOrigins.AddRange(origins);
        }
        policy
            .WithOrigins(finalOrigins.ToArray())
            .AllowAnyHeader()
            .AllowAnyMethod();
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
        
        context.Database.Migrate();
        logger.LogInformation("Database migrations applied successfully.");

        if (app.Environment.IsDevelopment())
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

if (app.Environment.IsDevelopment())
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

app.Run();

public partial class Program;
