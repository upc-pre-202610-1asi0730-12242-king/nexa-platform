using King.Nexa.Platform.CatalogManagement.Infrastructure.DependencyInjection;
using King.Nexa.Platform.Iam.Infrastructure.DependencyInjection;
using King.Nexa.Platform.Invoicing.Infrastructure.DependencyInjection;
using King.Nexa.Platform.Logistics.Infrastructure.DependencyInjection;
using King.Nexa.Platform.Sales.Infrastructure.DependencyInjection;
using King.Nexa.Platform.Shared.Infrastructure.DependencyInjection;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using King.Nexa.Platform.Shared.Infrastructure.Interfaces.AspNetCore.Configuration;
using King.Nexa.Platform.Shared.Infrastructure.Pipeline.Middleware.Extensions;
using King.Nexa.Platform.Shared.Infrastructure.Seed;
using King.Nexa.Platform.Warehouse.Infrastructure.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

const string frontendCorsPolicy = "AllowFrontend";
var allowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins")
    .Get<string[]>() ?? ["http://localhost:5173", "https://localhost:5173"];

builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});
builder.Services.AddCors(options =>
{
    options.AddPolicy(frontendCorsPolicy, policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddSharedInfrastructure(builder.Configuration);
builder.Services.AddCatalogManagement();
builder.Services.AddSales();
builder.Services.AddWarehouse();
builder.Services.AddLogistics();
builder.Services.AddInvoicing();
builder.Services.AddIam();

builder.Services.AddControllers(options =>
{
    options.Conventions.Add(new KebabCaseRouteNamingConvention());
});
builder.Services.AddLocalization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
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

        var seedDataOptions = services.GetRequiredService<Microsoft.Extensions.Options.IOptions<SeedDataOptions>>();
        if (seedDataOptions.Value.Enabled)
        {
            var seedDataService = services.GetRequiredService<ISeedDataService>();
            await seedDataService.SeedAsync();
            logger.LogInformation("Seed data applied successfully.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while applying database migrations.");
        throw;
    }
}

app.UseForwardedHeaders();
app.UseGlobalExceptionHandling();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors(frontendCorsPolicy);
app.UseAuthorization();
app.MapControllers();

app.Run();
